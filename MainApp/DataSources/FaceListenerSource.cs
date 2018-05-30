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
    public class FaceListenerSource : DependencyObject {
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

        // After Video Player Camera Changed. Map should receive this to change icon.
        public delegate void PlayingCameraChanged(Camera camera);
        public event PlayingCameraChanged OnPlayingCameraChanged;
        public void DoPlayingCameraChange(Camera camera) {
            this.OnPlayingCameraChanged?.Invoke(camera);
            PlayingCamera = camera;
        }
        public Camera PlayingCamera {
            get { return (Camera)GetValue(PlayingCameraProperty); }
            set { SetValue(PlayingCameraProperty, value); }
        }
        public static readonly DependencyProperty PlayingCameraProperty =
            DependencyProperty.Register("PlayingCamera", typeof(Camera), typeof(FaceListenerSource), new PropertyMetadata(null));

        // After Video Player Playing Time Changed. Detail should receive this to change track.
        public delegate void PlayingTimeChanged(long timestamp);
        public event PlayingTimeChanged OnPlayingTimeChanged;
        public void DoPlayingTimeChange(long timestamp) {
            this.OnPlayingTimeChanged?.Invoke(timestamp);
        }

        // After Map Camera Selected. Video Player should receive this to goto next area.
        public delegate void MapCameraClicked(Camera camera);
        public event MapCameraClicked OnMapCameraClicked;
        public void DoMapCameraClicked(Camera camera) {
            this.OnMapCameraClicked?.Invoke(camera);
        }

        public void StartServer() {
            string ip = ConfigurationManager.AppSettings["ip"];
            string port = ConfigurationManager.AppSettings["port"];
            long maximumFaces = long.Parse(ConfigurationManager.AppSettings["maximum_keep_faces"]);

            Host = string.Format("ws://{0}:{1}", ip, port);
            /// Fetch Latest
            var wsl = new WebSocket(string.Format("{0}/latestImages", Host));
            wsl.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);

                ///// workaround, todo remove
                //var names = new List<string> { "VIP", "Blacklist", null, null, null, null, null, null, null };
                //var random = new Random();
                //int index = random.Next(names.Count);
                //if (face.name == "Val") face.groupname = "VIP";
                //else if (face.name == "") {
                //    face.name = null;
                //    face.groupname = "Stranger";
                //} else {
                //    face.groupname = names[index];
                //}
                ///// workaround, todo remove

                // Unrecognized
                if (face.name == null) face.groupname = "No Match";

                this.Dispatcher.BeginInvoke(new Action(() => {
                    //for (var i=0; i<300; ++i) Faces.Add(face);
                    Faces.Add(face);
                }));
            };
            wsl.ConnectAsync();

            ///// workaround, todo remove ///
            //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            //List<string> images = new List<string>() {
            //    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS5THpTGLWqXyKH-PYVdAquMG3kdHGrymmBkmDnnAuSFVTw-YQQ",
            //    "http://issue247.com/wp-content/uploads/2015/05/%E0%B8%AA%E0%B8%B4%E0%B9%88%E0%B8%87%E0%B8%97%E0%B8%B5%E0%B9%88%E0%B8%AA%E0%B8%B2%E0%B8%A7%E0%B9%86%E0%B8%84%E0%B8%A7%E0%B8%A3%E0%B8%97%E0%B8%B3%E0%B8%81%E0%B9%88%E0%B8%AD%E0%B8%99%E0%B8%99%E0%B8%AD%E0%B8%99%E0%B9%80%E0%B8%9E%E0%B8%B7%E0%B9%88%E0%B8%AD%E0%B9%83%E0%B8%AB%E0%B9%89%E0%B8%94%E0%B8%B9%E0%B9%83%E0%B8%AA%E0%B9%81%E0%B8%9A%E0%B9%8A%E0%B8%A7.jpg",
            //    "https://thumbs.dreamstime.com/z/jovem-mulher-que-mostra-o-produto-de-beleza-32942553.jpg",
            //    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSbK8CdyftAkdQVPcqp4StapckaxhizPGYDS1JcRfcMAMq-Yscx3A",
            //    "https://obs.line-scdn.net/0hSUmtCMrhDHpQDSOgk85zLWpbDxVjYR95NDtdeQxjUk51bh5-OGNGFHMKBUstb0skOWpGHHQIF0t6akN7aGJG/w644",
            //    "https://obs.line-scdn.net/0h0kAuWjeIb0R8DECev88QE0ZabCtPYHxHGDo-RyBiMXBZb31AFGIlKl8MYn1XbigaFWslIlgKdHVWayBFRGMl/w644",
            //    "https://obs.line-scdn.net/0hFhYWCYxtGUdkLjadp-FmEF54GihXQgpEABhIRDhAR3NBTQtDDEBfKUcrRXdAG14ZDUlTIUQtAnZOSVZGXEFf/w644",
            //    "https://farm9.staticflickr.com/8636/15785217080_bc766078cb_o.jpg",
            //    "http://d3t543lkaz1xy.cloudfront.net/photo/5a0e39c9f03c80349d3114c9_m",
            //    "https://obs.line-scdn.net/0h9qwZGv1kZl5KI0mdsyUZCXB1ZTF5T3VdLhU3XRZNOGpvQyRfc0Epa2ZzbWdgECEAI0QtOmshfW9gRyEIJUAp/w644",
            //    "http://www.central.co.th/e-shopping/wp-content/uploads/2017/05/%E0%B8%84%E0%B8%A3%E0%B8%B5%E0%B8%A1%E0%B8%9A%E0%B8%B3%E0%B8%A3%E0%B8%B8%E0%B8%87%E0%B8%9C%E0%B8%B4%E0%B8%A7-%E0%B8%A1%E0%B8%AD%E0%B8%A2%E0%B9%80%E0%B8%88%E0%B8%AD%E0%B8%A3%E0%B9%8C%E0%B9%84%E0%B8%A3%E0%B9%80%E0%B8%8B%E0%B8%AD%E0%B8%A3%E0%B9%8C.jpg",
            //};
            ////"http://www.xn--r3cd1ab3b.com/upload/sexy/sexyupPic2_1462587630.jpg",
            ////    "http://www.xn--r3cd1ab3b.com/upload/sexy/sexyupPic3_1462587630.jpg",
            ////    "http://www.xn--r3cd1ab3b.com/upload/sexy/sexyupPic4_1462587630.jpg",
            ////    "http://www.xn--r3cd1ab3b.com/upload/sexy/sexyupPic5_1462587630.jpg",
            //for (var i = 0; i < 10000; ++i) {
            //    FaceItem face = new FaceItem() {
            //        name = i.ToString(),
            //        createtime = i * 1000,
            //        groupname = "VIP",
            //        image = images[i % images.Count],
            //        quality = 1,
            //        sourceid = "Camera01"
            //    };
            //    Faces.Add(face);
            //}
            ///// workaround, todo remove ///

            /// Start Server
            var ws = new WebSocket(string.Format("{0}/listen", Host));
            ws.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);
                if (face.name == null) face.groupname = "No Match";
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
                Console.WriteLine("connected. face listening@{0}", string.Format("{0}/listen", Host));
            };
            ws.ConnectAsync();
        }

        WebSocket ws = null;
        public void StartSearch(dynamic face) {
            this.FaceDetail.CurrentFace = face;
            this.FaceDetail.DoCurrentFaceChange(face);
            this.FaceDetail.EntryTime = 0;
            this.FaceDetail.LastTime = 0;
            this.FaceDetail.Traces.Clear();
            TrackChanged.OnNext(true);
            this.FaceDetail.PossibleContacts.Clear();
            this.DoPlayingCameraChange(null);
            PlayingCamera = null;

            long duration = long.Parse(ConfigurationManager.AppSettings["search_duration_seconds"]) * 1000;
            var starttime = face.createtime - duration;
            var endtime = face.createtime + duration;

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
                    if (obj_item.status == "start") {
                        this.FaceDetail.Dispatcher.BeginInvoke(new Action(() => this.FaceDetail.Progress = 0));
                    }
                    if (obj_item.status == "stop") {
                        this.FaceDetail.Dispatcher.BeginInvoke(new Action(() => this.FaceDetail.Progress = 100));
                        ws.CloseAsync();
                    }
                } else {
                    const double rate = 0.8;
                    allitem.Add(obj_item);

                    double percent = (double)(obj_item.createtime - starttime) / (endtime - starttime) * 100;
                    this.FaceDetail.Dispatcher.BeginInvoke(
                        new Action(() => {
                            this.FaceDetail.Progress = Math.Max(Math.Min(100.0, percent), this.FaceDetail.Progress);
                        })
                    );

                    allitem = new ConcurrentBag<SearchItem>(allitem
                        .OrderByDescending(x => x.createtime)
                        .ThenBy(x => x.image)
                        );

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
                }
                return;
            };
            ws.OnOpen += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();
                var param = new SearchParam() {
                    //starttime = face.createtime - 1000 * 60 * 5,
                    //endtime = face.createtime + 1000 * 60 * 5,
                    name = face.name,
                    starttime = starttime,
                    endtime = endtime,
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
