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
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Threading;

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingVideoMonitor.xaml
    /// </summary>
    public partial class FaceTracingVideoMonitor : UserControl {
        private IDisposable _subscription;

        public FaceTracingVideoMonitor() {
            InitializeComponent();

            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");

            this.Traces = new ObservableCollection<Track>();

            const double startpaddingseconds = 5;
            const double endpaddingseconds = 10;

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

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

            ///
            /// within OnTimeCode & OnDragSetCurrentTime, calculate goto next track.
            ///
            Action<long, bool> gotoTime = (long timestamp, bool force) => {
                Camera camera = null;
                foreach (var trace in this.Traces) {
                    /// before time
                    if (trace.starttime > timestamp) {
                        timestamp = (long)trace.starttime;
                        camera = trace.camera;
                        break;
                    }
                    /// matches time
                    if (trace.starttime <= timestamp && trace.endtime >= timestamp) {
                        camera = trace.camera;
                        if (!force) {
                            source.DoPlayingCameraChange(camera);
                            return;
                        }
                        break;
                    }
                }
                videoctrl.Goto((ulong)timestamp, 1);
                var task = Task.Run(() => {
                    System.Threading.Thread.Sleep(1000);
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        videoctrl.Goto((ulong)timestamp, 2);

                        source.DoPlayingCameraChange(camera);
                    }));
                });
            };

            videoctrl.OnTimeCode += (object sender, _INvrViewerEvents_OnTimeCodeEvent e) => {
                var time = long.Parse(e.t);
                if (!this.TimeTrack.IsDragging) {
                    this.TimeTrack.CurrentTime = time;
                }
                /// if end then stop
                if (time >= this.TimeTrack.EndTime) {
                    videoctrl.Goto((ulong)time, 1);
                    return;
                }

                /// fire event
                source.DoPlayingTimeChange(time);

                /// goto TimeTrack
                gotoTime(time, false);
            };

            /// Initial TimeTrack
            this.TimeTrack.OnDragSetCurrentTime += new FaceTracingVideoMonitors.TimeTrack.DragSetCurrentTime(
                new Action<long>((o) => gotoTime(o, true))
                );

            /// Hook Map Click event
            source.OnMapCameraClicked += (Camera camera) => {
                var i = 0;
                for (; i < this.Traces.Count; ++i) {
                    var trace = this.Traces[i];
                    if (trace.camera.sourceid != camera.sourceid) continue;
                    /// matches time
                    if (trace.starttime <= this.TimeTrack.CurrentTime && trace.endtime >= this.TimeTrack.CurrentTime) {
                        i++;
                        break;
                    }
                }

                for (var t = i; t < i+this.Traces.Count; ++t) {
                    var trace = this.Traces[t % this.Traces.Count];
                    if (trace.camera.sourceid != camera.sourceid) continue;
                    gotoTime((long)trace.starttime, true);
                    break;
                }
            };

            /// Initial Resource
            List<_INvrViewerEvents_OnConnectEventHandler> delegates = new List<_INvrViewerEvents_OnConnectEventHandler>();

            /// initial video throttle
            _subscription = Observable.CombineLatest(source.TrackChanged)
                .Select((o) => {
                    return Observable.Timer(TimeSpan.FromMilliseconds(600))
                    .Select(t => o);
                })
                .Switch()
                .Subscribe((o) => {
                    this.Dispatcher.BeginInvoke(
                        new Action(() => {

                            bool first = true;
                            string uri = "/airvideo/playback?channel=channel";
                            long? starttime = null;
                            List<string> tmp = new List<string>();

                            Traces.Clear();
                            begintime = 0;
                            endtime = 0;

                            /// step one, prepare video
                            foreach (var trace in source.FaceDetail.Traces) {
                                var cameraid = int.Parse(Regex.Match(trace.Camera.sourceid, @"\d+").Value);

                                ///// workaround, todo remove
                                //switch (cameraid) { case 1: cameraid = 6; break; case 2: cameraid = 7; break; case 3: cameraid = 8; break; case 5: cameraid = 10; break; default: cameraid = 9; break; }
                                ///// workaround, todo remove
                                var st = trace.starttime - (long)(startpaddingseconds * 1000);
                                /// for first, starttime - 5 seconds
                                if (first) {
                                    uri += cameraid;
                                    starttime = st;
                                    begintime = st;
                                    first = false;
                                }
                                /// for every, add into uri
                                tmp.Add(string.Format("{0},{1}", st / 1000, cameraid));
                                /// Check previous trace first
                                var et = trace.endtime + endpaddingseconds * 1000;
                                //var lt = Traces.LastOrDefault();
                                //if (lt != null && lt.endtime > st) lt.endtime = st;
                                /// Add into local Traces
                                begintime = Math.Min(begintime, st);
                                endtime = Math.Max(endtime, (long)et);
                                //Traces.Add(
                                //    new Track() { camera = trace.Camera, starttime = st, endtime = et }
                                //    );
                                //Console.WriteLine("Begin: {0}, End: {1}, st: {2}, ed: {3}", begintime, endtime, st, et);
                            }
                            if (starttime == null) return;

                            /// step two, prepare track
                            foreach (var trace in source.FaceDetail.Traces) {
                                foreach (var face in trace.Faces) {
                                    var st = face.createtime - startpaddingseconds * 1000;
                                    var et = face.createtime + endpaddingseconds * 1000;

                                    do {
                                        /// 1) check previous trace, if overlap, add to endtime.
                                        var lasttrace = this.Traces.LastOrDefault();
                                        if (lasttrace == null) goto Addnew;
                                        /// if overlap
                                        if (et > lasttrace.starttime && st < lasttrace.endtime) {
                                            if (lasttrace.camera.sourceid != face.sourceid) {
                                                lasttrace.endtime = st;
                                                goto Addnew;
                                            }
                                            lasttrace.endtime = et;
                                            break;
                                        }

                                        Addnew:
                                        /// 2) if not, create new trace.
                                        Traces.Add(
                                            new Track() {
                                                camera = trace.Camera,
                                                starttime = st,
                                                endtime = et
                                            }
                                        );

                                    } while (false);
                                }
                            }

                            /// endtime + 10 second
                            uri = string.Format("{0}&mix={1}", uri, string.Join(";", tmp.ToArray()));
                            //File.AppendAllText(@"C:\log.txt", string.Format("final uri: {0}, start: {1}, end: {2}", uri, this.Slider.Minimum / 1000, this.Slider.Maximum / 1000));

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
                                //videoctrl.Goto((ulong)starttime, 2);
                                gotoTime((long)starttime, true);
                                this.TimeTrack.CurrentTime = (long)starttime;
                                Console.WriteLine("start? {0} max? {1} min? {2}", starttime, this.begintime, this.endtime);
                            };
                            videoctrl.OnConnect += evt;
                            delegates.Add(evt);

                        })
                    , DispatcherPriority.ContextIdle);
                });
        }

        public class Track : DependencyObject {
            public Camera camera { get; set; }

            public double starttime {
                get { return (double)GetValue(starttimeProperty); }
                set { SetValue(starttimeProperty, value); }
            }

            // Using a DependencyProperty as the backing store for starttime.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty starttimeProperty =
                DependencyProperty.Register("starttime", typeof(double), typeof(Track), new PropertyMetadata(0.0));

            public double endtime {
                get { return (double)GetValue(endtimeProperty); }
                set { SetValue(endtimeProperty, value); }
            }

            // Using a DependencyProperty as the backing store for endtime.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty endtimeProperty =
                DependencyProperty.Register("endtime", typeof(double), typeof(Track), new PropertyMetadata(0.0));

        }

        #region "Dependency Properties"
        public ObservableCollection<Track> Traces { get; private set; }

        public long begintime {
            get { return (long)GetValue(begintimeProperty); }
            set { SetValue(begintimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for begintime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty begintimeProperty =
            DependencyProperty.Register("begintime", typeof(long), typeof(FaceTracingVideoMonitor), new PropertyMetadata(0L));

        public long endtime {
            get { return (long)GetValue(endtimeProperty); }
            set { SetValue(endtimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for endtime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty endtimeProperty =
            DependencyProperty.Register("endtime", typeof(long), typeof(FaceTracingVideoMonitor), new PropertyMetadata(0L));

        #endregion "Dependency Properties"
    }
}
