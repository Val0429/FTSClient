using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AxNvrViewerLib;
using Tencent.DataSources;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Controls.Primitives;
using System.IO;

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingVideoMonitor.xaml
    /// </summary>
    public partial class FaceTracingVideoMonitor : UserControl {
        bool dragging = false;

        public FaceTracingVideoMonitor() {
            InitializeComponent();

            /// Initial Video Control
            AxNvrCtrl videoctrl = this.VideoCtrl;
            videoctrl.SetPlayMode(1);
            videoctrl.ServerIp = ConfigurationManager.AppSettings["nvr_ip"];
            videoctrl.ServerPort = int.Parse(ConfigurationManager.AppSettings["nvr_port"]);
            videoctrl.ServerUsername = ConfigurationManager.AppSettings["nvr_account"];
            videoctrl.ServerPassword = ConfigurationManager.AppSettings["nvr_password"];
            videoctrl.ServerSSL = 0;
            videoctrl.DisplayTitleBar(1);
            videoctrl.StretchToFit = 1;
            videoctrl.AutoReconnect = 1;
            videoctrl.Mute = 1;
            videoctrl.OnTimeCode += (object sender, _INvrViewerEvents_OnTimeCodeEvent e) => {
                if (!dragging) this.Slider.Value = double.Parse(e.t);
                //Console.WriteLine("Slider {0} {1} {2}", this.Slider.Minimum, double.Parse(e.t) / 1000, this.Slider.Maximum);
            };

            /// Initial Slider
            this.Slider.ValueChanged += (object sender, RoutedPropertyChangedEventArgs<double> e) => {
                if (!dragging && e.NewValue == this.Slider.Maximum) {
                    videoctrl.Goto((ulong)this.Slider.Maximum, 1);
                }
            };
            this.Slider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler((object o, DragStartedEventArgs e) => {
                Console.WriteLine("Set Dragging True");
                dragging = true;
            }));
            this.Slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((object o, DragCompletedEventArgs e) => dragging = false));
            this.Slider.PreviewMouseUp += (object sender, MouseButtonEventArgs e) => {
                videoctrl.Goto((ulong)this.Slider.Value, 1);
                var task = Task.Run(() => {
                    System.Threading.Thread.Sleep(1000);
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        videoctrl.Goto((ulong)this.Slider.Value, 2);
                    }));
                });
            };

            /// Initial Resource
            List<_INvrViewerEvents_OnConnectEventHandler> delegates = new List<_INvrViewerEvents_OnConnectEventHandler>();
            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");
            source.FaceDetail.Traces.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {

                bool first = true;
                string uri = "/airvideo/playback?channel=channel";
                long? starttime = null;
                List<string> tmp = new List<string>();
                foreach (var trace in source.FaceDetail.Traces) {
                    var cameraid = int.Parse(Regex.Match(trace.Camera.sourceid, @"\d+").Value);
                    //switch (cameraid) {
                    //    case 1: cameraid = 6; break;
                    //    case 2: cameraid = 7; break;
                    //    case 3: cameraid = 8; break;
                    //    case 5: cameraid = 10; break;
                    //    default: cameraid = 9; break;
                    //}

                    if (first) {
                        uri += cameraid;
                        starttime = trace.starttime - 3000;
                        this.Slider.Minimum = (double)starttime;
                        first = false;
                    }
                    tmp.Add(string.Format("{0},{1}", (trace.starttime - 3000) / 1000, cameraid));
                }
                if (starttime == null) return;
                //this.Slider.Maximum = (double)source.FaceDetail.Traces[source.FaceDetail.Traces.Count - 1].endtime;
                this.Slider.Maximum = (double)source.FaceDetail.Traces[source.FaceDetail.Traces.Count - 1].endtime + 30*1000;
                uri = string.Format("{0}&mix={1}", uri, string.Join(";", tmp.ToArray()));
                Console.WriteLine("final uri: {0}, start: {1}, end: {2}", uri, this.Slider.Minimum/1000, this.Slider.Maximum/1000);
                File.AppendAllText(@"C:\log.txt", string.Format("final uri: {0}, start: {1}, end: {2}", uri, this.Slider.Minimum / 1000, this.Slider.Maximum / 1000));

                /// Clean Connect Event
                foreach (var eh in delegates) {
                    videoctrl.OnConnect -= eh;
                }
                delegates.Clear();

                videoctrl.Disconnect();
                videoctrl.ServerUri = uri;
                videoctrl.Connect();
                _INvrViewerEvents_OnConnectEventHandler evt = (object s2, _INvrViewerEvents_OnConnectEvent e2) => {
                    Console.WriteLine("On Connect Called");
                    videoctrl.Goto((ulong)starttime, 2);
                    this.Slider.Value = (double)starttime;
                };
                videoctrl.OnConnect += evt;
                delegates.Add(evt);
            };
        }
    }
}
