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
                    //for (var i=0; i<8; ++i) Faces.Add(face);
                    Faces.Add(face);
                    //Console.WriteLine("face count {0}", Faces.Count);
                    while (Faces.Count > maximumFaces) {
                        //Console.WriteLine("Try Remove");
                        Faces.RemoveAt(0);
                    }
                }));
            };
            wsl.ConnectAsync();

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
                Console.WriteLine("connected. face listening");
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
                    const double rate = 0.8;
                    allitem.Add(obj_item);
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
