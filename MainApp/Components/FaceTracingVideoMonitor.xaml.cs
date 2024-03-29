﻿using System;
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
using System.Xml;
using System.Net.Http;

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
            FTSServerSource FTSServer = Application.Current.FindResource("FTSServerSource") as FTSServerSource;
            string cmsIp = FTSServer.config.cms.ip;
            int cmsPort = FTSServer.config.cms.port;
            string cmsAccount = FTSServer.config.cms.account;
            string cmsPassword = FTSServer.config.cms.password;

            this.Traces = new ObservableCollection<Track>();
            this.Crosses = new ObservableCollection<Track>();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            /// Reset Time
            source.FaceDetail.OnCurrentFaceChanged += (FaceItem face) => {
                this.TimeTrack.CurrentTime = 0;
            };

            /// Initial Video Control
            //AxNvrCtrl videoctrl = this.VideoCtrl;
            AxiCMSCtrl videoctrl = this.VideoCtrl;
            videoctrl.SetPlayMode(1);
            videoctrl.ServerIp = cmsIp;
            videoctrl.ServerPort = cmsPort;
            videoctrl.ServerUsername = cmsAccount;
            videoctrl.ServerPassword = cmsPassword;
            videoctrl.ServerSSL = 0;
            videoctrl.DisplayTitleBar(1);
            videoctrl.StretchToFit = 0;
            videoctrl.AutoReconnect = 1;
            videoctrl.Mute = 1;

            this.VideoUtility.ServerIp = cmsIp;
            this.VideoUtility.ServerPort = cmsPort;
            this.VideoUtility.ServerUsername = cmsAccount;
            this.VideoUtility.ServerPassword = cmsPassword;
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

                foreach (var trace in this.Crosses) {
                    /// before time
                    if (trace.starttime > timestamp) {
                        timestamp = (long)trace.starttime;
                        camera = trace.camera;
                        break;
                    }

                    /// match time
                    if (trace.starttime <= timestamp && trace.endtime >= timestamp) {
                        camera = trace.camera;
                        if (!force) {
                            source.DoPlayingCameraChange(camera);
                            return;
                        }
                        break;
                    }
                }

                //foreach (var trace in this.Traces) {
                //    /// before time
                //    if (trace.starttime - 2000 /* workaround */ > timestamp) {
                //        timestamp = (long)trace.starttime - 2000 /* workaround */;
                //        camera = trace.camera;
                //        break;
                //    }

                //    //File.AppendAllText(@"D:\log.txt", string.Format("final uri: {0}, start: {1}, end: {2}", uri, this.Slider.Minimum / 1000, this.Slider.Maximum / 1000));

                //    /// matches time
                //    if (trace.starttime - 2000 /* workaround */ <= timestamp && trace.endtime >= timestamp) {
                //        camera = trace.camera;
                //        if (!force) {
                //            source.DoPlayingCameraChange(camera);
                //            return;
                //        }
                //        break;
                //    }
                //}
                mre_cancelcurrent.Set();
                //File.AppendAllText(@"C:\log.txt", string.Format("GotoTime(1): {0}\n", timestamp));
                videoctrl.Goto((ulong)timestamp, 1);
                mre_cancelcurrent.Reset();

                var task = Task.Run(() => {
                    if (mre_cancelcurrent.WaitOne(1000)) {
                        return;
                    }
                    //System.Threading.Thread.Sleep(1000);
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        //File.AppendAllText(@"C:\log.txt", string.Format("GotoTime(2): {0}\n", timestamp));
                        videoctrl.Goto((ulong)timestamp, 2);
                        videoctrl.PlayEx((ulong)timestamp, 1, "1x", 0, 0);

                        source.DoPlayingCameraChange(camera);
                    }));
                });
            };

            videoctrl.OnTimeCode += (object sender, _IiCMSViewerEvents_OnTimeCodeEvent e) => {
                var time = long.Parse(e.t);
                //File.AppendAllText(@"C:\log.txt", string.Format("OnTimeCode: {0}", time));
                if (!this.TimeTrack.IsDragging) {
                    this.TimeTrack.CurrentTime = time;
                }
                /// if end then stop
                //if (time > this.TimeTrack.EndTime) {
                if (time > this.Crosses[this.Crosses.Count - 1].endtime) {
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
                for (; i < this.Crosses.Count; ++i) {
                    var trace = this.Crosses[i];
                    if (trace.camera.sourceid != camera.sourceid) continue;
                    /// matches time
                    if (trace.starttime <= this.TimeTrack.CurrentTime && trace.endtime >= this.TimeTrack.CurrentTime) {
                        i++;
                        break;
                    }
                }

                for (var t = i; t < i + this.Crosses.Count; ++t) {
                    var trace = this.Crosses[t % this.Crosses.Count];
                    if (trace.camera.sourceid != camera.sourceid) continue;
                    gotoTime((long)trace.starttime, true);
                    break;
                }

                //var i = 0;
                //for (; i < this.Traces.Count; ++i) {
                //    var trace = this.Traces[i];
                //    if (trace.camera.sourceid != camera.sourceid) continue;
                //    /// matches time
                //    if (trace.starttime <= this.TimeTrack.CurrentTime && trace.endtime >= this.TimeTrack.CurrentTime) {
                //        i++;
                //        break;
                //    }
                //}

                //for (var t = i; t < i + this.Traces.Count; ++t) {
                //    var trace = this.Traces[t % this.Traces.Count];
                //    if (trace.camera.sourceid != camera.sourceid) continue;
                //    gotoTime((long)trace.starttime, true);
                //    break;
                //}
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

                            /// Show start download
                            this.DownloadLabel.Visibility = Visibility.Visible;

                            Traces.Clear();
                            begintime = 0;
                            endtime = 0;

                            Func<TraceItem, Tuple<long, long>> calStEd = (TraceItem item) => {
                                return new Tuple<long, long>(
                                    item.starttime - (long)(startpaddingseconds * 1000),
                                    item.endtime + (long)(endpaddingseconds * 1000)
                                );
                            };

                            ///// step one, prepare video
                            //for (var i = 0; i < this.Traces.Count; ++i) {
                            //    var trace = this.Traces[i];
                            //    var sourceid = trace.camera.sourceid;
                            //    Match match = Regex.Match(sourceid, @"(\d+)_(\d+)");
                            //    if (match.Groups.Count != 3) continue;
                            //    var nvrid = int.Parse(match.Groups[1].ToString());
                            //    var channelid = int.Parse(match.Groups[2].ToString());

                            //    var st = long.Parse(trace.starttime.ToString());
                            //    var et = long.Parse(trace.endtime.ToString());
                            //    if (first) {
                            //        uri = string.Format(uri, nvrid, channelid);
                            //        _currentNvrId = nvrid;
                            //        _currentChannelId = channelid;
                            //        starttime = begintime = st;
                            //        first = false;
                            //    }
                            //    /// don't overlap with next trace
                            //    if ((i + 1) < this.Traces.Count) {
                            //        et = Math.Min(et, long.Parse(this.Traces[i + 1].starttime.ToString()));
                            //    }
                            //    /// for every, add into uri
                            //    tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));
                            //    /// Add into local Traces
                            //    begintime = Math.Min(begintime, st);
                            //    endtime = Math.Max(endtime, et);
                            //}
                            ////foreach (var trace in source.FaceDetail.Traces) {
                            ////for (var i = 0; i < source.FaceDetail.Traces.Count; ++i) {
                            ////    var trace = source.FaceDetail.Traces[i];
                            ////    var sourceid = trace.Camera.sourceid;
                            ////    Console.WriteLine("sourceid: {0}", sourceid);
                            ////    Match match = Regex.Match(sourceid, @"(\d+)_(\d+)");
                            ////    if (match.Groups.Count != 3) continue;
                            ////    var nvrid = int.Parse(match.Groups[1].ToString());
                            ////    var channelid = int.Parse(match.Groups[2].ToString());

                            ////    /// for first, starttime - 5 seconds
                            ////    var sted = calStEd(trace);
                            ////    var st = sted.Item1;
                            ////    var et = sted.Item2;
                            ////    if (first) {
                            ////        uri = string.Format(uri, nvrid, channelid);
                            ////        _currentNvrId = nvrid;
                            ////        _currentChannelId = channelid;
                            ////        starttime = st;
                            ////        begintime = st;
                            ////        first = false;
                            ////    }
                            ////    /// don't overlap with next trace
                            ////    if ((i + 1) < source.FaceDetail.Traces.Count) {
                            ////        et = Math.Min(et, calStEd(source.FaceDetail.Traces[i + 1]).Item1);
                            ////    }
                            ////    /// for every, add into uri
                            ////    tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));
                            ////    /// Add into local Traces
                            ////    begintime = Math.Min(begintime, st);
                            ////    endtime = Math.Max(endtime, (long)et);
                            ////}
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

                            /// step one, prepare video
                            for (var i = 0; i < this.Traces.Count; ++i) {
                                var trace = this.Traces[i];
                                var sourceid = trace.camera.sourceid;
                                Match match = Regex.Match(sourceid, @"(\d+)_(\d+)");
                                if (match.Groups.Count != 3) continue;
                                var nvrid = int.Parse(match.Groups[1].ToString());
                                var channelid = int.Parse(match.Groups[2].ToString());

                                var st = long.Parse(trace.starttime.ToString());
                                var et = long.Parse(trace.endtime.ToString());
                                if (first) {
                                    uri = string.Format(uri, nvrid, channelid);
                                    _currentNvrId = nvrid;
                                    _currentChannelId = channelid;
                                    starttime = begintime = st;
                                    first = false;
                                }
                                /// don't overlap with next trace
                                if ((i + 1) < this.Traces.Count) {
                                    et = Math.Min(et, long.Parse(this.Traces[i + 1].starttime.ToString()));
                                }
                                /// for every, add into uri
                                tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));
                                /// Add into local Traces
                                begintime = Math.Min(begintime, st);
                                endtime = Math.Max(endtime, et);
                            }
                            //foreach (var trace in source.FaceDetail.Traces) {
                            //for (var i = 0; i < source.FaceDetail.Traces.Count; ++i) {
                            //    var trace = source.FaceDetail.Traces[i];
                            //    var sourceid = trace.Camera.sourceid;
                            //    Console.WriteLine("sourceid: {0}", sourceid);
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
                            //    if ((i + 1) < source.FaceDetail.Traces.Count) {
                            //        et = Math.Min(et, calStEd(source.FaceDetail.Traces[i + 1]).Item1);
                            //    }
                            //    /// for every, add into uri
                            //    tmp.Add(string.Format("{0},{1},{2},{3}", st / 1000, et / 1000, nvrid, channelid));
                            //    /// Add into local Traces
                            //    begintime = Math.Min(begintime, st);
                            //    endtime = Math.Max(endtime, (long)et);
                            //}
                            if (starttime == null) return;

                            /// endtime + 10 second
                            _currentCross = string.Join(";", tmp.ToArray());
                            uri = string.Format("{0}&cross={1}", uri, _currentCross);
                            //MessageBox.Show(_currentCross);
                            //File.AppendAllText(@"C:\log.txt", _currentCross);
                            //MessageBox.Show(string.Format("debug playback uri: {0}", uri));

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
                                /* workaround: wait for playbackdone, to start play */
                                //gotoTime(this.TimeTrack.CurrentTime, true);
                                //videoctrl.Goto((ulong)starttime, 1);
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

            /// Wait playback download complete
            this.VideoUtility.OnServerEventReceive += (object sender, _IiCMSUtilityEvents_OnServerEventReceiveEvent e) => {
                string message = e.msg;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(message);
                var rootNode = doc.FirstChild;

                if (rootNode == null) return;
                if (rootNode.Name == "Event") {
                    var node = rootNode.SelectSingleNode("Type");
                    if (node.InnerText != "PlaybackDownloadDone") return;
                    node = rootNode.SelectSingleNode("SessionID");
                    if (node.InnerText == _pbSessionId) {
                        /// Show download complete
                        this.DownloadLabel.Visibility = Visibility.Hidden;
                        /// get Cross
                        node = rootNode.SelectSingleNode("Cross");
                        var cross = node.InnerText;
                        //File.AppendAllText(@"C:\log.txt", cross);
                        this.Crosses.Clear();
                        foreach (var unit in cross.Split(';')) {
                            var data = unit.Split(',');
                            this.Crosses.Add(
                                    new Track() {
                                        starttime = double.Parse(data[0]) * 1000,
                                        endtime = double.Parse(data[1]) * 1000,
                                        camera = getCameraByNvrChannelId(int.Parse(data[2]), int.Parse(data[3]))
                                    }
                                );
                        }
                        /// goto
                        gotoTime(this.TimeTrack.BeginTime, true);
                    }
                }
            };
            this.VideoUtility.StartEventReceive();

        }

        private Camera getCameraByNvrChannelId(int nvrid, int channelid) {
            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");

            foreach (var camera in source.Cameras) {
                Match match = Regex.Match(camera.Value.sourceid, @"(\d+)_(\d+)");
                if (match.Groups.Count != 3) continue;
                var pnvrid = int.Parse(match.Groups[1].ToString());
                var pchannelid = int.Parse(match.Groups[2].ToString());
                if (nvrid == pnvrid && channelid == pchannelid)
                    return camera.Value;
            }

            return null;
        }

        static public List<Tuple<long, long, int, int>> calculateTracks() {
            var result = new List<Tuple<long, long, int, int>>();

            /// return: starttime, endtime, nvrid, channelid
            FaceListenerSource source = (FaceListenerSource)Application.Current.FindResource("FaceListenerSource");

            Func<TraceItem, Tuple<long, long>> calStEd = (TraceItem item) => {
                return new Tuple<long, long>(
                    item.starttime - (long)(startpaddingseconds * 1000),
                    item.endtime + (long)(endpaddingseconds * 1000)
                );
            };

            for (var i = 0; i < source.FaceDetail.Traces.Count; ++i) {
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
            FTSServerSource FTSServer = Application.Current.FindResource("FTSServerSource") as FTSServerSource;
            string cmsIp = FTSServer.config.cms.ip;
            int cmsPort = FTSServer.config.cms.port;
            string cmsAccount = FTSServer.config.cms.account;
            string cmsPassword = FTSServer.config.cms.password;

            /// Clean OnExportStatus Event
            foreach (var eh in delegates2) {
                this.VideoUtility.OnExportStatus -= eh;
            }
            delegates2.Clear();

            AxiCMSCtrl videoctrl = this.VideoCtrl;

            string cross = _currentCross;
            var startTime = Convert.ToUInt32(begintime / 1000);  // seconds
            var endTime = Convert.ToUInt32(endtime / 1000);      // seconds
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
            var path = "C:\\FTSExport";

            this.VideoUtility.ExportFile2(startTime, endTime, _currentNvrId, _pbSessionId, _currentChannelId, format, path,
                prefix, maxFileSize, encoding, quality, scale, osdWatermark, osdText, font, fontSize, fontColor, watermarkText, cross);

            _IiCMSUtilityEvents_OnExportStatusEventHandler evt = async (object s, _IiCMSUtilityEvents_OnExportStatusEvent e) => {
                /// ExportFinished: 4
                /// UserAborted: 5
                /// ExportFailed: 6
                Console.WriteLine("{0}, {1}", e.status, e.progress);
                if (e.status == 6) {
                    MessageBox.Show("导出失败");
                } else if (e.status == 4) {
                    MessageBox.Show("导出成功");
                    /// send evis
                    var starttime = this.Traces[0].starttime;
                    FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");

                    var HttpHost = string.Format("http://{0}:{1}/cgi-bin/snapshot?nvr=nvr{2}&channel=channel{3}&timestamp={4}&width=640&height=360", cmsIp, cmsPort, this._currentNvrId, this._currentChannelId, starttime);

                    /// take snapshot
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "c2FuemhpbmlhbzpqaWppamlhbw==");
                    var result = await client.GetAsync(HttpHost);
                    var bytes = await result.Content.ReadAsByteArrayAsync();
                    var snapshot_b64str = Convert.ToBase64String(bytes);

                    /// get mp4
                    bytes = File.ReadAllBytes("C:\\FTSExport.mp4");
                    var mp4_b64str = Convert.ToBase64String(bytes);
                    //Console.WriteLine(mp4_b64str.Length);

                    /// send evis
                    var clientevis = new HttpClient();
                    var byteContent = new StringContent(string.Format("{{ \"time\": {0}, \"cameraName\": \"{1}\", \"mp4\": \"{2}\", \"snapshot\": \"{3}\" }}",
                        starttime,
                        this.Traces[0].camera.name,
                        mp4_b64str,
                        snapshot_b64str
                        ));
                    byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    result = await clientevis.PostAsync(string.Format("{0}/report", source.HttpHost), byteContent);
                    var resultStr = await result.Content.ReadAsStringAsync();

                    ///// do login
                    //var client = new HttpClient();
                    //var byteContent = new StringContent(string.Format("{{ \"username\": \"{0}\", \"password\": \"{1}\" }}", account, password));
                    //byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    //var result = await client.PostAsync(string.Format("{0}/users/login", HttpHost), byteContent);
                    //var resultStr = await result.Content.ReadAsStringAsync();
                    //var jsonSerializerx = new JavaScriptSerializer();
                    //var user = jsonSerializerx.Deserialize<OutputLogin>(resultStr);
                    //var sessionId = user.sessionId;
                    //this.sessionId = sessionId;

                }
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
        public ObservableCollection<Track> Crosses { get; private set; }

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
