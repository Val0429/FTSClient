using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using Tencent.Configurations;
using WebSocketSharp;

namespace Tencent.DataSources {
    public class FaceItem {
        public string sourceid { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public double facewidth { get; set; }
        public double faceheight { get; set; }
        public double quality { get; set; }
        public long createtime { get; set; }
        public string groupname { get; set; }
    }

    public class SearchParam {
        public string searchid { get; set; }
        public string name { get; set; }
        public long starttime { get; set; }
        public long endtime { get; set; }
        public string image { get; set; }
        public double score { get; set; }
    }

    public class SearchInfo {
        public string searchid { get; set; }
        public string status { get; set; }
    }

    public class SearchItem {
        public string searchid { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string sourceid { get; set; }
        public string image { get; set; }
        public long createtime { get; set; }
        public double score { get; set; }
    }

    public class PeopleGroup {
        [XmlAttribute("name")]
        public string name { get; set; }
        [XmlAttribute("color")]
        public string color { get; set; }
        [XmlAttribute("glowcolor")]
        public string glowcolor { get; set; }
    }

    public class Camera : DependencyObject {
        [XmlAttribute("name")]
        public string name { get; set; }
        [XmlAttribute("sourceid")]
        public string sourceid { get; set; }
        [XmlAttribute("type")]
        public int type { get; set; }
        [XmlAttribute("X")]
        public double X { get; set; }
        [XmlAttribute("Y")]
        public double Y { get; set; }
        [XmlAttribute("Angle")]
        public double Angle { get; set; }

        public SearchItem Face {
            get { return (SearchItem)GetValue(FaceProperty); }
            set { SetValue(FaceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Face.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FaceProperty =
            DependencyProperty.Register("Face", typeof(SearchItem), typeof(Camera), new PropertyMetadata(null));


    }

    public class TraceItem : DependencyObject {
        public TraceItem() {
            Faces = new ObservableCollection<SearchItem>();
        }


        public Camera Camera { get; set; }

        //public Camera Camera {
        //    get { return (Camera)GetValue(CameraProperty); }
        //    set { SetValue(CameraProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Camera.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty CameraProperty =
        //    DependencyProperty.Register("Camera", typeof(Camera), typeof(TraceItem), new PropertyMetadata(null));

        public long starttime { get; set; }
        public long endtime { get; set; }
        public ObservableCollection<SearchItem> Faces { get; private set; }
    }

    public class FaceDetail : DependencyObject {
        public FaceDetail() {
            PossibleContacts = new ObservableCollection<SearchItem>();
            Traces = new ObservableCollection<TraceItem>();
        }
        #region "Dependency Properties"
        public FaceItem CurrentFace {
            get { return (FaceItem)GetValue(CurrentFaceProperty); }
            set { SetValue(CurrentFaceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for currentFace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentFaceProperty =
            DependencyProperty.Register("CurrentFace", typeof(FaceItem), typeof(FaceDetail), new PropertyMetadata(null));

        public long EntryTime {
            get { return (long)GetValue(EntryTimeProperty); }
            set { SetValue(EntryTimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for EntryTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EntryTimeProperty =
            DependencyProperty.Register("EntryTime", typeof(long), typeof(FaceDetail), new PropertyMetadata(null));

        public long LastTime {
            get { return (long)GetValue(LastTimeProperty); }
            set { SetValue(LastTimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for LastTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastTimeProperty =
            DependencyProperty.Register("LastTime", typeof(long), typeof(FaceDetail), new PropertyMetadata(null));

        #endregion "Dependency Properties"

        public ObservableCollection<TraceItem> Traces { get; private set; }

        public ObservableCollection<SearchItem> PossibleContacts { get; private set; }
    }

    public class FaceListenerSource : DispatcherObject {
        private string Host { get; set; }

        public FaceListenerSource() {
            Faces = new ObservableCollection<FaceItem>();
            FaceDetail = new FaceDetail();
            Cameras = new Dictionary<string, Camera>();
            PeopleGroups = new Dictionary<string, PeopleGroup>();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            List<Camera> config = (List<Camera>)ConfigurationManager.GetSection("CameraInfo");
            foreach (var camera in config)
                Cameras[camera.sourceid] = camera;

            List<PeopleGroup> pg = (List<PeopleGroup>)ConfigurationManager.GetSection("PeopleGroupInfo");
            foreach (var peoplegroup in pg) {
                if (peoplegroup.glowcolor == null) peoplegroup.glowcolor = peoplegroup.color;
                PeopleGroups[peoplegroup.name] = peoplegroup;
            }

            StartServer();
        }

        public Subject<bool> TrackChanged = new Subject<bool>();

        public ObservableCollection<FaceItem> Faces { get; private set; }

        public FaceDetail FaceDetail { get; private set; }

        public Dictionary<string, Camera> Cameras { get; private set; }

        public Dictionary<string, PeopleGroup> PeopleGroups { get; private set; }

        public void StartServer() {
            string ip = ConfigurationManager.AppSettings["ip"];
            string port = ConfigurationManager.AppSettings["port"];

            Host = string.Format("ws://{0}:{1}", ip, port);
            /// Fetch Latest
            var wsl = new WebSocket(string.Format("{0}/latestImages", Host));
            wsl.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);

                /// workaround, todo remove
                var names = new List<string> { "VIP", "Blacklist", null, null, null, null, null, null, null };
                var random = new Random();
                int index = random.Next(names.Count);
                if (face.name == "Val") face.groupname = "VIP";
                else if (face.name == "") {
                    face.name = null;
                    face.groupname = "Stranger";
                } else {
                    face.groupname = names[index];
                }
                /// workaround, todo remove

                this.Dispatcher.BeginInvoke(new Action(() => {
                    Faces.Add(face);
                }));
            };
            wsl.ConnectAsync();

            /// Start Server
            var ws = new WebSocket(string.Format("{0}/listen", Host));
            ws.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);
                this.Dispatcher.BeginInvoke(new Action(() => {
                    Faces.Add(face);
                }));
            };
            ws.OnClose += (sender, e) => {
                Console.WriteLine("Close reason {0} {1}", e.Code, e.Reason);
                ws.ConnectAsync();
                Console.WriteLine("Do reconnect");
            };
            ws.OnError += (sender, e) => {
                Console.WriteLine("Error reason {0}", e.Message);
                ws.ConnectAsync();
                Console.WriteLine("Do reconnect");
            };
            ws.OnOpen += (sender, e) => {
                Console.WriteLine("connected. face listening");
            };
            ws.ConnectAsync();
        }

        WebSocket ws = null;
        public void StartSearch(dynamic face) {
            this.FaceDetail.CurrentFace = face;
            this.FaceDetail.EntryTime = 0;
            this.FaceDetail.LastTime = 0;
            this.FaceDetail.Traces.Clear();
            TrackChanged.OnNext(true);
            this.FaceDetail.PossibleContacts.Clear();

            ConcurrentBag<SearchItem> allitem = new ConcurrentBag<SearchItem>();

            long comp_duration = long.Parse(ConfigurationManager.AppSettings["possible_companion_duration_seconds"]) * 1000;

            foreach (var value in Cameras.Values) {
                ((Camera)value).Face = null;
            }

            if (ws != null) ws.Close();
            ws = new WebSocket( string.Format("{0}/search", Host) );
            ws.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();
                var obj_item = jsonSerializer.Deserialize<SearchItem>(e.Data);
                var obj_info = jsonSerializer.Deserialize<SearchInfo>(e.Data);
                if (obj_item.image == null) {
                    if (obj_item.status == "stop") {
                        ws.CloseAsync();
                    }
                } else {
                    const double rate = 0.7;
                    allitem.Add(obj_item);
                    allitem = new ConcurrentBag<SearchItem>(allitem.OrderByDescending(x => x.createtime));

                    this.FaceDetail.Dispatcher.BeginInvoke(
                        new Action(() => {
                            this.FaceDetail.Traces.Clear();
                            this.FaceDetail.PossibleContacts.Clear();
                            SearchItem match = null;
                            List<SearchItem> notmatches = new List<SearchItem>();

                            foreach (SearchItem obj in allitem) {
                                if (obj.score < rate) {
                                    /// not match
                                    if (match == null || Math.Abs(match.createtime - obj.createtime) > comp_duration)
                                        notmatches.Add(obj);
                                    else
                                        this.FaceDetail.PossibleContacts.Add(obj);

                                } else {
                                    /// matches
                                    /// 1) get last traces, if camera match, add into it.
                                    /// 2) if not match, add new trace, then add into it.
                                    do {
                                        /// detect possible comps
                                        match = obj;
                                        foreach (var notmatch in notmatches) {
                                            if (Math.Abs(obj.createtime - notmatch.createtime) <= comp_duration)
                                                this.FaceDetail.PossibleContacts.Add(notmatch);
                                        }
                                        notmatches.Clear();

                                        // 1)
                                        if (this.FaceDetail.Traces.Count > 0) {
                                            var lasttrace = this.FaceDetail.Traces[this.FaceDetail.Traces.Count - 1];
                                            if (lasttrace.Camera.sourceid == obj.sourceid) {
                                                lasttrace.Faces.Add(obj);
                                                lasttrace.starttime = Math.Min(obj.createtime, lasttrace.starttime);
                                                lasttrace.endtime = Math.Max(obj.createtime, lasttrace.endtime);
                                                this.FaceDetail.EntryTime = Math.Min(this.FaceDetail.EntryTime, obj.createtime);
                                                this.FaceDetail.LastTime = Math.Max(this.FaceDetail.LastTime, obj.createtime);
                                                break;
                                            }
                                        }

                                        // 2)
                                        // find camera
                                        Camera camera = null;
                                        Cameras.TryGetValue(obj.sourceid, out camera);
                                        if (camera == null) break;
                                        /// create trace
                                        var traceitem = new TraceItem();
                                        traceitem.Camera = camera;
                                        traceitem.starttime = traceitem.endtime = obj.createtime;
                                        traceitem.Faces.Add(obj);
                                        this.FaceDetail.Traces.Add(traceitem);
                                        this.FaceDetail.EntryTime = Math.Min(
                                            this.FaceDetail.EntryTime == 0 ? long.MaxValue : this.FaceDetail.EntryTime,
                                                obj.createtime);
                                        this.FaceDetail.LastTime = Math.Max(this.FaceDetail.LastTime, obj.createtime);
                                        camera.Face = obj;

                                    } while (false);
                                }
                            }
                            TrackChanged.OnNext(true);
                        })
                    );

                        //this.FaceDetail.Dispatcher.BeginInvoke(
                        //        new Action(() => {
                        //            if (obj_item.score < rate) {
                        //                /// not match
                        //                    ///this.FaceDetail.PossibleContacts.Add(obj_item);
                        //                /// check for matches to add
                        //                if (matches.Count > 0) {
                        //                    SearchItem item = matches.Last();
                        //                    if (Math.Abs(item.createtime - obj_item.createtime) <= comp_duration) {
                        //                        if (item.sourceid == obj_item.sourceid)
                        //                            this.FaceDetail.PossibleContacts.Add(obj_item);
                        //                    } else {
                        //                        notmatches.Add(obj_item);
                        //                    }
                        //                } else {
                        //                    notmatches.Add(obj_item);
                        //                }

                        //            } else {
                        //                /// matches
                        //                /// 1) get last traces, if camera match, add into it.
                        //                /// 2) if not match, add new trace, then add into it.
                        //                matches.Add(obj_item);
                        //                /// check for notmatches to add
                        //                foreach (var obj in notmatches) {
                        //                    if (Math.Abs(obj.createtime - obj_item.createtime) <= comp_duration && obj.sourceid == obj_item.sourceid) {
                        //                        this.FaceDetail.PossibleContacts.Add(obj_item);
                        //                    }
                        //                }
                        //                notmatches.Clear();

                        //                do {
                        //                    // 1)
                        //                    if (this.FaceDetail.Traces.Count > 0) {
                        //                        var lasttrace = this.FaceDetail.Traces[this.FaceDetail.Traces.Count - 1];
                        //                        if (lasttrace.Camera.sourceid == obj_item.sourceid) {
                        //                            lasttrace.Faces.Add(obj_item);
                        //                            lasttrace.starttime = Math.Min(obj_item.createtime, lasttrace.starttime);
                        //                            lasttrace.endtime = Math.Max(obj_item.createtime, lasttrace.endtime);
                        //                            this.FaceDetail.EntryTime = Math.Min(this.FaceDetail.EntryTime, obj_item.createtime);
                        //                            this.FaceDetail.LastTime = Math.Max(this.FaceDetail.LastTime, obj_item.createtime);
                        //                            this.TrackChanged?.Invoke(true);
                        //                            break;
                        //                        }
                        //                    }

                        //                    // 2)
                        //                    // find camera
                        //                    Camera camera = null;
                        //                    Cameras.TryGetValue(obj_item.sourceid, out camera);
                        //                    if (camera == null) break;
                        //                    /// create trace
                        //                    var traceitem = new TraceItem();
                        //                    traceitem.Camera = camera;
                        //                    traceitem.starttime = traceitem.endtime = obj_item.createtime;
                        //                    traceitem.Faces.Add(obj_item);
                        //                    this.FaceDetail.Traces.Add(traceitem);
                        //                    this.FaceDetail.EntryTime = Math.Min(
                        //                        this.FaceDetail.EntryTime == 0 ? long.MaxValue : this.FaceDetail.EntryTime,
                        //                            obj_item.createtime);
                        //                    this.FaceDetail.LastTime = Math.Max(this.FaceDetail.LastTime, obj_item.createtime);
                        //                    camera.Face = obj_item;
                        //                    this.TrackChanged?.Invoke(true);

                        //                } while (false);
                        //            }
                        //        })
                        //    );
                }
                return;
            };
            ws.OnOpen += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();
                long duration = long.Parse(ConfigurationManager.AppSettings["search_duration_seconds"]) * 1000;
                var param = new SearchParam() {
                    //starttime = face.createtime - 1000 * 60 * 5,
                    //endtime = face.createtime + 1000 * 60 * 5,
                    name = face.name,
                    starttime = face.createtime - duration,
                    endtime = face.createtime + duration,
                    image = face.image,
                    score = 0,
                    searchid = "",
                };
                Console.WriteLine(jsonSerializer.Serialize(param));
                ws.SendAsync(jsonSerializer.Serialize(param), null);
            };
            ws.ConnectAsync();
        }
    }
}
