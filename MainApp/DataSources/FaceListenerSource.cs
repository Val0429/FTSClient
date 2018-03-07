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

    public class FaceDetail : DependencyObject {
        public FaceDetail() {
            PossibleContacts = new ObservableCollection<SearchItem>();
        }
        #region "Dependency Properties"
        public FaceItem CurrentFace {
            get { return (FaceItem)GetValue(CurrentFaceProperty); }
            set { SetValue(CurrentFaceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for currentFace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentFaceProperty =
            DependencyProperty.Register("CurrentFace", typeof(FaceItem), typeof(FaceDetail), new PropertyMetadata(null));
        #endregion "Dependency Properties"

        public ObservableCollection<SearchItem> PossibleContacts { get; private set; }
    }

    public class FaceListenerSource : DispatcherObject {
        private string Host { get; set; }

        public FaceListenerSource() {
            Faces = new ObservableCollection<FaceItem>();
            FaceDetail = new FaceDetail();

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
            var wsl = new WebSocket( string.Format("{0}/latestImages", Host) );
            wsl.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);
                this.Dispatcher.BeginInvoke(new Action(() => {
                    Faces.Add(face);
                }));
            };
            wsl.ConnectAsync();

            /// Start Server
            var ws = new WebSocket( string.Format("{0}/listen", Host) );
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

        public ObservableCollection<FaceItem> Faces { get; private set; }

        public FaceDetail FaceDetail { get; private set; }

        public void StartSearch(FaceItem face) {
            this.FaceDetail.CurrentFace = face;
            this.FaceDetail.PossibleContacts.Clear();

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
                    this.FaceDetail.Dispatcher.BeginInvoke(
                            new Action(() => {
                                this.FaceDetail.PossibleContacts.Add(obj_item);
                            })
                        );
                }
                return;
            };
            ws.OnOpen += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();
                var param = new SearchParam() {
                    starttime = face.createtime - 1000000,
                    endtime = face.createtime + 1000000,
                    image = face.image,
                    score = 0,
                    searchid = "",
                };
                ws.SendAsync(jsonSerializer.Serialize(param), null);
            };
            ws.ConnectAsync();
        }
    }
}
