using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
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
        public string image { get; set; }
        public double facewidth { get; set; }
        public double faceheight { get; set; }
        public double quality { get; set; }
        public long createtime { get; set; }
    }

    public class SearchParam {
        public string searchid { get; set; }
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
        public string status { get; set; }
        public string sourceid { get; set; }
        public string image { get; set; }
        public long createtime { get; set; }
        public double score { get; set; }
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

            List<Camera> config = (List<Camera>)ConfigurationManager.GetSection("CameraInfo");
            foreach (var camera in config)
                Cameras[camera.sourceid] = camera;

            StartServer();
        }

        public ObservableCollection<FaceItem> Faces { get; private set; }

        public FaceDetail FaceDetail { get; private set; }

        public Dictionary<string, Camera> Cameras { get; private set; }

        public void StartServer() {
            string ip, port;
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime) {
                ip = ConfigurationManager.AppSettings["ip"];
                port = ConfigurationManager.AppSettings["port"];
            } else {
                //XmlDocument doc = new XmlDocument();
                //doc.Load("App.config");
                //ip = doc.SelectSingleNode("configuration/startup/appSettings/add[@key='ip']").Value;
                //port = doc.SelectSingleNode("configuration/startup/appSettings/add[@key='port']").Value;
                ip = "localhost";
                port = "7070";
            }

            Host = string.Format("ws://{0}:{1}", ip, port);
            /// Fetch Latest
            var wsl = new WebSocket(string.Format("{0}/latestImages", Host));
            wsl.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);
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

        public void StartSearch(dynamic face) {
            this.FaceDetail.CurrentFace = face;
            this.FaceDetail.EntryTime = 0;
            this.FaceDetail.Traces.Clear();
            this.FaceDetail.PossibleContacts.Clear();

            foreach (var value in Cameras.Values) {
                ((Camera)value).Face = null;
            }

            var ws = new WebSocket( string.Format("{0}/search", Host) );
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

                        this.FaceDetail.Dispatcher.BeginInvoke(
                                new Action(() => {
                                    if (obj_item.score < rate) {

                                        //Console.WriteLine("not match! {0}", obj_item);
                                        /// not match
                                        this.FaceDetail.PossibleContacts.Add(obj_item);
                                    } else {
                                        /// matches
                                        /// 1) get last traces, if camera match, add into it.
                                        /// 2) if not match, add new trace, then add into it.

                                        //Console.WriteLine("match! {0}", obj_item);
                                        do {
                                            // 1)
                                            if (this.FaceDetail.Traces.Count > 0) {
                                                var lasttrace = this.FaceDetail.Traces[this.FaceDetail.Traces.Count - 1];
                                                if (lasttrace.Camera.sourceid == obj_item.sourceid) {
                                                    lasttrace.Faces.Add(obj_item);
                                                    lasttrace.starttime = Math.Min(obj_item.createtime, lasttrace.starttime);
                                                    lasttrace.endtime = Math.Min(obj_item.createtime, lasttrace.endtime);
                                                    this.FaceDetail.EntryTime = Math.Min(this.FaceDetail.EntryTime, obj_item.createtime);
                                                    break;
                                                }
                                            }

                                            // 2)
                                            // find camera
                                            Camera camera = null;
                                            Cameras.TryGetValue(obj_item.sourceid, out camera);
                                            if (camera == null) break;
                                            /// create trace
                                            var traceitem = new TraceItem();
                                            traceitem.Camera = camera;
                                            traceitem.starttime = traceitem.endtime = obj_item.createtime;
                                            traceitem.Faces.Add(obj_item);
                                            this.FaceDetail.Traces.Add(traceitem);
                                            this.FaceDetail.EntryTime = Math.Min(
                                                this.FaceDetail.EntryTime == 0 ? long.MaxValue : this.FaceDetail.EntryTime,
                                                    obj_item.createtime);
                                            camera.Face = obj_item;

                                        } while (false);
                                    }
                                })
                            );
                }
                return;
            };
            ws.OnOpen += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();
                var param = new SearchParam() {
                    starttime = face.createtime - 1000 * 60 * 5,
                    endtime = face.createtime + 1000 * 60 * 5,
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
