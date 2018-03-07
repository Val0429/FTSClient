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

    public class FaceDetail : DependencyObject {
        #region "Dependency Properties"
        public FaceItem CurrentFace {
            get { return (FaceItem)GetValue(CurrentFaceProperty); }
            set { SetValue(CurrentFaceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for currentFace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentFaceProperty =
            DependencyProperty.Register("CurrentFace", typeof(FaceItem), typeof(FaceDetail), new PropertyMetadata(null));
        #endregion "Dependency Properties"
    }

    public class FaceListenerSource : DispatcherObject {
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

            var host = string.Format("ws://{0}:{1}", ip, port);
            /// Fetch Latest
            var wsl = new WebSocket( string.Format("{0}/latestImages", host) );
            wsl.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);
                this.Dispatcher.BeginInvoke(new Action(() => {
                    Faces.Add(face);
                }));
            };
            wsl.ConnectAsync();

            /// Start Server
            var ws = new WebSocket( string.Format("{0}/listen", host) );
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
        }
    }
}
