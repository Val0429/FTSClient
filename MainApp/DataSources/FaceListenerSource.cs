using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Threading;
using WebSocketSharp;

namespace Tencent.DataSources {
    public class FaceItem {
        public string sourceid { get; set; }
        public string image { get; set; }
        public double facewidth { get; set; }
        public double faceheight { get; set; }
        public long createtime { get; set; }
    }

    public class FaceListenerSource : DispatcherObject {
        public FaceListenerSource() {
            Faces = new ObservableCollection<FaceItem>();

            var host = string.Format("ws://{0}:{1}", ConfigurationManager.AppSettings["ip"], ConfigurationManager.AppSettings["port"]);
            var ws = new WebSocket( string.Format("{0}/listen", host) );
            ws.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();

                var face = jsonSerializer.Deserialize<FaceItem>(e.Data);
                this.Dispatcher.BeginInvoke(new Action(() => {
                    Faces.Add(face);
                }));
            };
            ws.Connect();
            Console.WriteLine("ws connected");
        }

        public ObservableCollection<FaceItem> Faces { get; private set; }
    }
}
