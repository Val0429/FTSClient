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
using AxiCMSViewerLib;
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
using System.Threading;
using AxiCMSUtilityLib;

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingVideoMonitor.xaml
    /// </summary>
    public partial class FaceTracingVideoMonitor : UserControl {
        private IDisposable _subscription;

        private const double startpaddingseconds = 5;
        private const double endpaddingseconds = 10;

        private string _currentCross;
        private string _pbSessionId;
        private int _currentNvrId;
        private int _currentChannelId;

        public FaceTracingVideoMonitor() {
            InitializeComponent();

            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");

            this.Traces = new ObservableCollection<Track>();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            /// Reset Time
            source.FaceDetail.OnCurrentFaceChanged += (FaceItem face) => {
                this.TimeTrack.CurrentTime = 0;
            };

            /// Initial Video Control
            //AxNvrCtrl videoctrl = this.VideoCtrl;
            AxiCMSCtrl videoctrl = this.VideoCtrl;
            videoctrl.SetPlayMode(1);
            videoctrl.ServerIp = ConfigurationManager.AppSettings["nvr_ip"];
            videoctrl.ServerPort = int.Parse(ConfigurationManager.AppSettings["nvr_port"]);
            videoctrl.ServerUsername = ConfigurationManager.AppSettings["nvr_account"];
            videoctrl.ServerPassword = ConfigurationManager.AppSettings["nvr_password"];
            videoctrl.ServerSSL = 0;
            videoctrl.DisplayTitleBar(1);
            videoctrl.StretchToFit = 0;
            videoctrl.AutoReconnect = 1;
            videoctrl.Mute = 1;

            this.VideoUtility.ServerIp = ConfigurationManager.AppSettings["nvr_ip"];
            this.VideoUtility.ServerPort = int.Parse(ConfigurationManager.AppSettings["nvr_port"]);
            this.VideoUtility.ServerUsername = ConfigurationManager.AppSettings["nvr_account"];
            this.VideoUtility.ServerPassword = ConfigurationManager.AppSettings["nvr_password"];
            this.VideoUtility.ServerSSL = 0;

            ///// test code
            ////videoctrl.ServerIp = "172.16.10.90";
            ////videoctrl.ServerPort = 8080;
            ////videoctrl.ServerUsername = "Admin";
            ////videoctrl.ServerPassword = "Aa123456";
            //string cross = "1529494384,1529494484,2,1";
            //string uri2 = string.Format("/airvideo/playback?nvr=nvr2&channel=channel1&track=video,audio1&wait=0&stream=1&cross={0}", cross);
            //var startTime = (uint)(1529074020);  // seconds
            //var endTime = (uint)(1529074120);      // seconds
            //var format = "video,audio1";
            //var prefix = "";
            //ulong maxFileSize = 500 * 1024 * 1024;    // bytes
            //var encoding = 3;           // 0:raw, 1:original, 2:mjpeg, 3:mp4
            //var quality = 75;           // 1 ~ 100
            //var scale = 0;              // 0:original 1:1/2,  2:1/4,  3:1/8  4:1/16
            //var osdWatermark = 0;       // 0:disable 1:enable osd, 2:enable watermark, 3:enable osd and watermark
            //var osdText = "";
            //var font = "Arial";         // 
            //var fontSize = 12;
            //var fontColor = 16777215;   // #FFFFFF white
            //var watermarkText = "";
            //videoctrl.OnConnect += (object sender, AxiCMSViewerLib._IiCMSViewerEvents_OnConnectEvent e) => {
            //    var session = e.playback_sessionid;

            //    //this.VideoUtility.ExportFile2(startTime, endTime, 2, session, 1, format, "D:\temp\test.mp4", prefix, maxFileSize,
            //    //                                encoding, quality, scale, osdWatermark, osdText, font, fontSize, fontColor, watermarkText, cross);

            //    //this.VideoUtility.OnExportStatus += (object sender2, AxiCMSUtilityLib._IiCMSUtilityEvents_OnExportStatusEvent e2) => {
            //    //    Console.WriteLine("{0}, {1}", e2.status, e2.progress);
            //    //};
            //    //tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));

            //    videoctrl.Goto(startTime, 2);
            //    videoctrl.PlayEx(startTime, 1, "1x", 0, 0);

            //};
            //videoctrl.ServerUri = uri2;
            //videoctrl.Connect();

            ///
            /// within OnTimeCode & OnDragSetCurrentTime, calculate goto next track.
            ///
            ManualResetEvent mre_cancelcurrent = new ManualResetEvent(false);

            Action<long, bool> gotoTime = (long timestamp, bool force) => {
                Camera camera = null;
                foreach (var trace in this.Traces) {
                    /// before time
                    if (trace.starttime - 2000 /* workaround */ > timestamp) {
                        timestamp = (long)trace.starttime - 2000 /* workaround */;
                        camera = trace.camera;
                        break;
                    }

                    //File.AppendAllText(@"D:\log.txt", string.Format("final uri: {0}, start: {1}, end: {2}", uri, this.Slider.Minimum / 1000, this.Slider.Maximum / 1000));

                    /// matches time
                    if (trace.starttime - 2000 /* workaround */ <= timestamp && trace.endtime >= timestamp) {
                        camera = trace.camera;
                        if (!force) {
                            source.DoPlayingCameraChange(camera);
                            return;
                        }
                        break;
                    }
                }
                mre_cancelcurrent.Set();
                videoctrl.Goto((ulong)timestamp, 1);
                mre_cancelcurrent.Reset();

                var task = Task.Run(() => {
                    if (mre_cancelcurrent.WaitOne(1000)) {
                        return;
                    }
                    //System.Threading.Thread.Sleep(1000);
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        videoctrl.Goto((ulong)timestamp, 2);
                        videoctrl.PlayEx((ulong)timestamp, 1, "1x", 0, 0);

                        source.DoPlayingCameraChange(camera);
                    }));
                });
            };

            videoctrl.OnTimeCode += (object sender, _IiCMSViewerEvents_OnTimeCodeEvent e) => {
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
            List<_IiCMSViewerEvents_OnConnectEventHandler> delegates = new List<_IiCMSViewerEvents_OnConnectEventHandler>();
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
                            string uri = "/airvideo/playback?nvr=nvr{0}&channel=channel{1}&track=video,audio1&wait=0&stream=1";
                            long? starttime = null;
                            List<string> tmp = new List<string>();

                            Traces.Clear();
                            begintime = 0;
                            endtime = 0;

                            Func<TraceItem, Tuple<long, long>> calStEd = (TraceItem item) => {
                                return new Tuple<long, long>(
                                    item.starttime - (long)(startpaddingseconds * 1000),
                                    item.endtime + (long)(endpaddingseconds * 1000)
                                );
                            };

                            /// step one, prepare video
                            //foreach (var trace in source.FaceDetail.Traces) {

                            var stedresults = calculateTracks();
                            foreach (var data in stedresults) {
                                var st = data.Item1;
                                var et = data.Item2;
                                var nvrid = data.Item3;
                                var channelid = data.Item4;
                                if (first) {
                                    uri = string.Format(uri, nvrid, channelid);
                                    _currentNvrId = nvrid;
                                    _currentChannelId = channelid;
                                    starttime = st;
                                    begintime = st;
                                    first = false;
                                }
                                tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));
                                /// Add into local Traces
                                begintime = Math.Min(begintime, st);
                                endtime = Math.Max(endtime, (long)et);
                            }
                            if (starttime == null) return;

                            //for (var i=0; i<source.FaceDetail.Traces.Count; ++i) {
                            //    var trace = source.FaceDetail.Traces[i];
                            //    var sourceid = trace.Camera.sourceid;
                            //    Match match = Regex.Match(sourceid, @"(\d+)_(\d+)");
                            //    if (match.Groups.Count != 3) continue;
                            //    var nvrid = int.Parse(match.Groups[1].ToString());
                            //    var channelid = int.Parse(match.Groups[2].ToString());

                            //    /// for first, starttime - 5 seconds
                            //    var sted = calStEd(trace);
                            //    var st = sted.Item1;
                            //    var et = sted.Item2;
                            //    if (first) {
                            //        uri = string.Format(uri, nvrid, channelid);
                            //        _currentNvrId = nvrid;
                            //        _currentChannelId = channelid;
                            //        starttime = st;
                            //        begintime = st;
                            //        first = false;
                            //    }
                            //    /// don't overlap with next trace
                            //    if ((i+1) < source.FaceDetail.Traces.Count) {
                            //        et = Math.Min(et, calStEd(source.FaceDetail.Traces[i + 1]).Item1);
                            //    }
                            //    /// for every, add into uri
                            //    tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));
                            //    /// Add into local Traces
                            //    begintime = Math.Min(begintime, st);
                            //    endtime = Math.Max(endtime, (long)et);
                            //}
                            //if (starttime == null) return;

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
                            _currentCross = string.Join(";", tmp.ToArray());
                            uri = string.Format("{0}&cross={1}", uri, _currentCross);
                            //File.AppendAllText(@"C:\log.txt", string.Format("final uri: {0}, start: {1}, end: {2}", uri, this.Slider.Minimum / 1000, this.Slider.Maximum / 1000));

                            /// Clean Connect Event
                            foreach (var eh in delegates) {
                                videoctrl.OnConnect -= eh;
                            }
                            delegates.Clear();

                            videoctrl.Disconnect();
                            videoctrl.ServerUri = uri;
                            videoctrl.Connect();
                            _IiCMSViewerEvents_OnConnectEventHandler evt = (object s2, _IiCMSViewerEvents_OnConnectEvent e2) => {
                                _pbSessionId = e2.playback_sessionid;
                                Console.WriteLine("On Connect Called");
                                //videoctrl.Goto((ulong)starttime, 2);
                                if (this.TimeTrack.CurrentTime == 0) this.TimeTrack.CurrentTime = (long)starttime;
                                gotoTime(this.TimeTrack.CurrentTime, true);
                                Console.WriteLine("start? {0} max? {1} min? {2}", starttime, this.begintime, this.endtime);
                            };
                            videoctrl.OnConnect += evt;
                            delegates.Add(evt);

                            Console.WriteLine("uri {0}", uri);

                            //bool first = true;
                            //string uri = "/airvideo/playback?channel=channel";
                            //long? starttime = null;
                            //List<string> tmp = new List<string>();

                            //Traces.Clear();
                            //begintime = 0;
                            //endtime = 0;

                            ///// step one, prepare video
                            //foreach (var trace in source.FaceDetail.Traces) {
                            //    var cameraid = int.Parse(Regex.Match(trace.Camera.sourceid, @"\d+").Value);

                            //    var st = trace.starttime - (long)(startpaddingseconds * 1000);
                            //    /// for first, starttime - 5 seconds
                            //    if (first) {
                            //        uri += cameraid;
                            //        starttime = st;
                            //        begintime = st;
                            //        first = false;
                            //    }
                            //    /// for every, add into uri
                            //    tmp.Add(string.Format("{0},{1}", st / 1000, cameraid));
                            //    /// Check previous trace first
                            //    var et = trace.endtime + endpaddingseconds * 1000;
                            //    /// Add into local Traces
                            //    begintime = Math.Min(begintime, st);
                            //    endtime = Math.Max(endtime, (long)et);
                            //}
                            //if (starttime == null) return;

                            ///// step two, prepare track
                            //foreach (var trace in source.FaceDetail.Traces) {
                            //    foreach (var face in trace.Faces) {
                            //        var st = face.createtime - startpaddingseconds * 1000;
                            //        var et = face.createtime + endpaddingseconds * 1000;

                            //        do {
                            //            /// 1) check previous trace, if overlap, add to endtime.
                            //            var lasttrace = this.Traces.LastOrDefault();
                            //            if (lasttrace == null) goto Addnew;
                            //            /// if overlap
                            //            if (et > lasttrace.starttime && st < lasttrace.endtime) {
                            //                if (lasttrace.camera.sourceid != face.sourceid) {
                            //                    lasttrace.endtime = st;
                            //                    goto Addnew;
                            //                }
                            //                lasttrace.endtime = et;
                            //                break;
                            //            }

                            //            Addnew:
                            //            /// 2) if not, create new trace.
                            //            Traces.Add(
                            //                new Track() {
                            //                    camera = trace.Camera,
                            //                    starttime = st,
                            //                    endtime = et
                            //                }
                            //            );

                            //        } while (false);
                            //    }
                            //}

                            ///// endtime + 10 second
                            //uri = string.Format("{0}&mix={1}", uri, string.Join(";", tmp.ToArray()));
                            ////File.AppendAllText(@"C:\log.txt", string.Format("final uri: {0}, start: {1}, end: {2}", uri, this.Slider.Minimum / 1000, this.Slider.Maximum / 1000));

                            ///// Clean Connect Event
                            //foreach (var eh in delegates) {
                            //    videoctrl.OnConnect -= eh;
                            //}
                            //delegates.Clear();

                            //videoctrl.Disconnect();
                            //videoctrl.ServerUri = uri;
                            //videoctrl.Connect();
                            //_IiCMSViewerEvents_OnConnectEventHandler evt = (object s2, _IiCMSViewerEvents_OnConnectEvent e2) => {
                            //    Console.WriteLine("On Connect Called");
                            //    //videoctrl.Goto((ulong)starttime, 2);
                            //    if (this.TimeTrack.CurrentTime == 0) this.TimeTrack.CurrentTime = (long)starttime;
                            //    gotoTime(this.TimeTrack.CurrentTime, true);
                            //    Console.WriteLine("start? {0} max? {1} min? {2}", starttime, this.begintime, this.endtime);
                            //};
                            //videoctrl.OnConnect += evt;
                            //delegates.Add(evt);

                        })
                    , DispatcherPriority.ContextIdle);
                });
        }
        
        static public List<Tuple<long, long, int, int>> calculateTracks() {
            var result = new List<Tuple<long, long, int, int>> ();

            /// return: starttime, endtime, nvrid, channelid
            FaceListenerSource source = (FaceListenerSource)Application.Current.FindResource("FaceListenerSource");

            Func<TraceItem, Tuple<long, long>> calStEd = (TraceItem item) => {
                return new Tuple<long, long>(
                    item.starttime - (long)(startpaddingseconds * 1000),
                    item.endtime + (long)(endpaddingseconds * 1000)
                );
            };

            for (var i=0; i<source.FaceDetail.Traces.Count; ++i) {
                var trace = source.FaceDetail.Traces[i];
                var sourceid = trace.Camera.sourceid;
                Match match = Regex.Match(sourceid, @"(\d+)_(\d+)");
                if (match.Groups.Count != 3) continue;
                var nvrid = int.Parse(match.Groups[1].ToString());
                var channelid = int.Parse(match.Groups[2].ToString());

                var sted = calStEd(trace);
                var st = sted.Item1;
                var et = sted.Item2;
                
                /// don't overlap with next trace
                if ((i + 1) < source.FaceDetail.Traces.Count) {
                    et = Math.Min(et, calStEd(source.FaceDetail.Traces[i + 1]).Item1);
                }

                result.Add(new Tuple<long, long, int, int>(st, et, nvrid, channelid));
            }

            return result;
        }

        List<_IiCMSUtilityEvents_OnExportStatusEventHandler> delegates2 = new List<_IiCMSUtilityEvents_OnExportStatusEventHandler>();
        public void export() {
            /// Clean OnExportStatus Event
            foreach (var eh in delegates2) {
                this.VideoUtility.OnExportStatus -= eh;
            }
            delegates2.Clear();

            AxiCMSCtrl videoctrl = this.VideoCtrl;

            //videoctrl.SetPlayMode(1);
            //videoctrl.ServerIp = ConfigurationManager.AppSettings["nvr_ip"];
            //videoctrl.ServerPort = int.Parse(ConfigurationManager.AppSettings["nvr_port"]);
            //videoctrl.ServerUsername = ConfigurationManager.AppSettings["nvr_account"];
            //videoctrl.ServerPassword = ConfigurationManager.AppSettings["nvr_password"];
            //videoctrl.ServerSSL = 0;
            //videoctrl.DisplayTitleBar(1);
            //videoctrl.StretchToFit = 0;
            //videoctrl.AutoReconnect = 1;
            //videoctrl.Mute = 1;

            string cross = _currentCross;
            var startTime = Convert.ToUInt32(begintime/1000);  // seconds
            var endTime = Convert.ToUInt32(endtime/1000);      // seconds
            var format = "video,audio1";
            var prefix = "";
            ulong maxFileSize = 500 * 1024 * 1024;    // bytes
            var encoding = 3;           // 0:raw, 1:original, 2:mjpeg, 3:mp4
            var quality = 75;           // 1 ~ 100
            var scale = 0;              // 0:original 1:1/2,  2:1/4,  3:1/8  4:1/16
            var osdWatermark = 0;       // 0:disable 1:enable osd, 2:enable watermark, 3:enable osd and watermark
            var osdText = "";
            var font = "Arial";         // 
            var fontSize = 12;
            var fontColor = 16777215;   // #FFFFFF white
            var watermarkText = "";
            var path = "D:\\temp";

            this.VideoUtility.ExportFile2(startTime, endTime, _currentNvrId, _pbSessionId, _currentChannelId, format, path,
                prefix, maxFileSize, encoding, quality, scale, osdWatermark, osdText, font, fontSize, fontColor, watermarkText, cross);

            _IiCMSUtilityEvents_OnExportStatusEventHandler evt = (object s, _IiCMSUtilityEvents_OnExportStatusEvent e) => {
                Console.WriteLine("{0}, {1}", e.status, e.progress);
            };
            this.VideoUtility.OnExportStatus += evt;
            delegates2.Add(evt);

            //videoctrl.OnConnect += (object sender, AxiCMSViewerLib._IiCMSViewerEvents_OnConnectEvent e) => {
            //    var session = e.playback_sessionid;

            //    //this.VideoUtility.ExportFile2(startTime, endTime, 2, session, 1, format, "D:\temp\test.mp4", prefix, maxFileSize,
            //    //                                encoding, quality, scale, osdWatermark, osdText, font, fontSize, fontColor, watermarkText, cross);

            //    //this.VideoUtility.OnExportStatus += (object sender2, AxiCMSUtilityLib._IiCMSUtilityEvents_OnExportStatusEvent e2) => {
            //    //    Console.WriteLine("{0}, {1}", e2.status, e2.progress);
            //    //};
            //    //tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));

            //    videoctrl.Goto(startTime, 2);
            //    videoctrl.PlayEx(startTime, 1, "1x", 0, 0);

            //};
            //videoctrl.ServerUri = uri2;
            //videoctrl.Connect();
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

        private void TimeTrackUnit_ExportClicked(object sender, RoutedEventArgs e) {
            export();
        }
    }
}
