﻿using System;
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
using System.Net.Http;
using System.Reflection;

namespace Tencent.DataSources {
    public class FaceListenerSource : DependencyObject {
        private string WsHost { get; set; }
        public string HttpHost { get; set; }

        public FaceListenerSource() {
            Faces = new ObservableCollection<FaceItem>();
            FaceDetail = new FaceDetail();
            Floors = new Dictionary<int, Floor>();
            Cameras = new Dictionary<string, Camera>();
            PeopleGroups = new Dictionary<string, PeopleGroup>();

            //if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
        }

        public void InitConfig(FTSServerSource FTSServer) {
            /// init floors
            foreach (var floor in FTSServer.floors) {
                Floors[floor.floor] = new Floor {
                    name = floor.name,
                    number = floor.floor,
                    image = floor.image
                };
            }
            /// init cameras
            foreach (var camera in FTSServer.cameras) {
                Cameras[camera.sourceId] = new Camera {
                    name = camera.name,
                    floor = camera.floor.floor,
                    Angle = camera.angle,
                    sourceid = camera.sourceId,
                    type = 0,
                    X = camera.x,
                    Y = camera.y,
                };
            }
            /// init groups
            foreach (var group in FTSServer.config.fts.groupInfo) {
                PeopleGroups[group.name] = new PeopleGroup {
                    name = group.name,
                    color = group.color,
                    glowcolor = group.glowcolor
                };
            }
        }

        public Subject<bool> TrackChanged = new Subject<bool>();

        public ObservableCollection<FaceItem> Faces { get; private set; }

        public FaceDetail FaceDetail { get; private set; }

        public Dictionary<int, Floor> Floors { get; private set; }

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

        public string sessionId { get; set; }
        public void StartServer() {
            InitialNewListen();
        }

        WebSocket mainWs = null;
        private EventHandler<MessageEventArgs> messageCallback = null;
        private EventHandler<CloseEventArgs> closeCallback = null;
        private EventHandler<ErrorEventArgs> errorCallback = null;
        private EventHandler openCallback = null;
        public void InitialNewListen(long? startms = null, long? endms = null, bool alive = true) {
            FTSServerSource FTSServer = Application.Current.FindResource("FTSServerSource") as FTSServerSource;

            /// bridge back to FaceListener
            this.sessionId = FTSServer.sessionId;
            string ip = FTSServer.ip;
            string port = FTSServer.port;
            WsHost = string.Format("ws://{0}:{1}", ip, port);
            HttpHost = string.Format("http://{0}:{1}", ip, port);

            /// generalize new face
            Action<bool> doReconnect = null;
            if (messageCallback == null) {
                messageCallback = new EventHandler<MessageEventArgs>((sender, e) => {
                    var jsonSerializer = new JavaScriptSerializer();
                    var face = jsonSerializer.Deserialize<FaceItem>(e.Data);
                    face.sourceid = face.channel;
                    face.name = face.person_info?.fullname;
                    face.image = string.Format("{0}/snapshot?sessionId={1}&image={2}", HttpHost, sessionId, face.snapshot);
                    face.createtime = face.timestamp;
                    if (face.groups != null && face.groups?.Length > 0) face.groupname = face.groups[0].name;

                    if (face.type == FaceType.UnRecognized) face.groupname = "No Match";
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        /// duplicate face logic
                        var uid = face.valFaceId;
                        long pprevid = long.MaxValue;
                        if (uid != 0)
                            for (var i = Faces.Count - 1; i >= 0; --i) {
                                FaceItem prevFace = Faces[i];
                                var previd = prevFace.valFaceId;
                                /// should larger
                                if (pprevid <= previd) break;
                                /// no match
                                if (uid > previd) break;
                                if (uid == previd) {
                                    /// replace
                                    Faces[i] = face;
                                    Console.WriteLine("{0}, {1}", uid, previd);
                                    return;
                                }
                                pprevid = previd;
                            }
                        Faces.Add(face);
                    }));
                });
            }

            if (closeCallback == null) {
                closeCallback = new EventHandler<CloseEventArgs>((sender, e) => {
                    if (e.Code == 1000) return;
                    Console.WriteLine("Close reason {0} {1}", e.Code, e.Reason);
                    Console.WriteLine("Do reconnect");
                    doReconnect(false);
                });
            }

            if (errorCallback == null) {
                errorCallback = new EventHandler<ErrorEventArgs>((sender, e) => {
                    Console.WriteLine("Error reason {0}", e.Message);
                    Console.WriteLine("Do reconnect");
                    doReconnect(false);
                });
            }

            if (openCallback == null) {
                openCallback = new EventHandler((sender, e) => {
                    Console.WriteLine("connected. face listening@{0}", string.Format("{0}/lastImages", WsHost));
                });
            }

            var disposeWs = new Action(() => {
                if (mainWs != null) {
                    mainWs.OnMessage -= messageCallback;
                    mainWs.OnClose -= closeCallback;
                    mainWs.OnError -= errorCallback;
                    mainWs.OnOpen -= openCallback;
                    mainWs.Close();
                }
            });

            doReconnect = new Action<bool>((bool first) => {
                disposeWs();
                /// merge start & end
                string strSted = "";
                if (startms != null && endms != null) strSted = string.Format("&start={0}&end={1}", startms, endms);
                string strAlive = "";
                if (alive) strAlive = "&alive=true";
                if (first)
                    mainWs = new WebSocket(string.Format("{0}/lastImages?sessionId={1}{2}&pureListen=false{3}", WsHost, sessionId, strAlive, strSted));
                else
                    mainWs = new WebSocket(string.Format("{0}/lastImages?sessionId={1}{2}&pureListen=true{3}", WsHost, sessionId, strAlive, strSted));
                mainWs.OnMessage += messageCallback;
                mainWs.OnClose += closeCallback;
                mainWs.OnError += errorCallback;
                mainWs.OnOpen += openCallback;
                mainWs.ConnectAsync();
            });

            /// stop old ws
            disposeWs();
            Faces.Clear();
            doReconnect(true);
        }


        WebSocket ws = null;
        public void StartSearch(dynamic face) {
            FTSServerSource FTSServer = Application.Current.FindResource("FTSServerSource") as FTSServerSource;
            long searchDurationSeconds = FTSServer.config.fts.searchDurationSeconds;
            long possibleCompanionDurationSeconds = FTSServer.config.fts.possibleCompanionDurationSeconds;

            this.FaceDetail.CurrentFace = face;
            this.FaceDetail.DoCurrentFaceChange(face);
            this.FaceDetail.EntryTime = 0;
            this.FaceDetail.LastTime = 0;
            this.FaceDetail.Traces.Clear();
            TrackChanged.OnNext(true);
            this.FaceDetail.PossibleContacts.Clear();
            this.DoPlayingCameraChange(null);
            PlayingCamera = null;

            long duration = searchDurationSeconds * 1000;
            var starttime = face.createtime - duration;
            var endtime = face.createtime + duration;

            long comp_duration = possibleCompanionDurationSeconds * 1000;

            foreach (var value in Cameras.Values) {
                ((Camera)value).Face = null;
            }

            var closeHandler = new EventHandler<CloseEventArgs>((sender, e) => {
                this.FaceDetail.Dispatcher.BeginInvoke(
                    new Action(() => this.FaceDetail.Progress = 100)
                );
            });

            if (ws != null) { ws.OnClose -= closeHandler; ws.Close(); }
            var jsonSerializerx = new JavaScriptSerializer();
            this.FaceDetail.Progress = 0;

            ws = new WebSocket( string.Format("{0}/search?sessionId={1}&starttime={2}&endtime={3}&face={4}",
                WsHost, this.sessionId, starttime, endtime, jsonSerializerx.Serialize(face)) );

            Console.Write(string.Format("{0}/search?sessionId={1}&starttime={2}&endtime={3}&face={4}",
                WsHost, this.sessionId, starttime, endtime, jsonSerializerx.Serialize(face)));

            SearchItem lastMatch = null;
            List<SearchItem> notmatches = new List<SearchItem>();
            /// ordered algorithm ///////////////////////////////////
            ws.OnClose += closeHandler;
            ws.OnMessage += (sender, e) => {
                var jsonSerializer = new JavaScriptSerializer();
                var obj_item = jsonSerializer.Deserialize<SearchItem>(e.Data);
                obj_item.sourceid = obj_item.channel;
                obj_item.name = obj_item.person_info?.fullname;
                obj_item.image = string.Format("{0}/snapshot?sessionId={1}&image={2}", HttpHost, sessionId, obj_item.snapshot);
                obj_item.createtime = obj_item.timestamp;
                if (obj_item.groups != null && obj_item.groups?.Length > 0) obj_item.groupname = obj_item.groups[0].name;

                const double rate = 0.6;
                /// calculate progress
                double percent = (double)(obj_item.createtime - starttime) / (endtime - starttime) * 100;
                this.FaceDetail.Dispatcher.BeginInvoke(
                        new Action(() => {
                            this.FaceDetail.Progress = Math.Max(Math.Min(100.0, percent), this.FaceDetail.Progress);
                        })
                    );

                /// calculate match
                this.FaceDetail.Dispatcher.BeginInvoke(
                        new Action(() => {
                            if (
                                (face.type == FaceType.Recognized && obj_item.name != face.name) ||
                                (face.type == FaceType.UnRecognized && obj_item.type == FaceType.Recognized) ||
                                (face.type == FaceType.UnRecognized && obj_item.score < rate)
                                ) {
                                /// not match
                                //this.FaceDetail.PossibleContacts.Add(obj_item);

                                //if (lastMatch != null && (lastMatch.createtime - obj_item.createtime) > comp_duration) {
                                //    this.FaceDetail.PossibleContacts.Add(obj_item);
                                //}

                                /// not match
                                if (lastMatch == null || Math.Abs(lastMatch.createtime - obj_item.createtime) > comp_duration)
                                    notmatches.Add(obj_item);
                                else if (lastMatch.sourceid == obj_item.sourceid)
                                    this.FaceDetail.PossibleContacts.Add(obj_item);
                                /// else ignore

                            } else {
                                /// matches
                                /// detect possible comps
                                foreach (var notmatch in notmatches) {
                                    if (Math.Abs(obj_item.createtime - notmatch.createtime) <= comp_duration)
                                        if (notmatch.sourceid == obj_item.sourceid)
                                            this.FaceDetail.PossibleContacts.Add(notmatch);
                                }
                                notmatches.Clear();

                                /// 1) get last traces, if camera match, add into it.
                                /// 2) if not match, add new trace, then add into it.
                                do {
                                    lastMatch = obj_item;
                                    // 1)
                                    if (this.FaceDetail.Traces.Count > 0) {
                                        var lasttrace = this.FaceDetail.Traces[this.FaceDetail.Traces.Count - 1];
                                        if (lasttrace.Camera.sourceid == obj_item.sourceid) {
                                            /// remove duplicate face
                                            var lastface = lasttrace.Faces[lasttrace.Faces.Count - 1];
                                            if (lastface.name != null && lastface.name == obj_item.name &&
                                                (obj_item.timestamp - lastface.timestamp) <= 3000) {
                                                //MessageBox.Show(string.Format("RemoveAt {0} {1}", lastface.timestamp, obj_item.timestamp));
                                                lasttrace.Faces.RemoveAt(lasttrace.Faces.Count - 1);
                                            }
                                            lasttrace.Faces.Add(obj_item);
                                            lasttrace.starttime = Math.Min(obj_item.createtime, lasttrace.starttime);
                                            lasttrace.endtime = Math.Max(obj_item.createtime, lasttrace.endtime);
                                            this.FaceDetail.EntryTime = Math.Min(this.FaceDetail.EntryTime, obj_item.createtime);
                                            this.FaceDetail.LastTime = Math.Max(this.FaceDetail.LastTime, obj_item.createtime);
                                            break;
                                        }
                                    }

                                    // 2)
                                    // find camera
                                    Camera camera = null;
                                    if (obj_item.sourceid == null) break;
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
                                    this.FaceDetail.LastTime = Math.Max(this.FaceDetail.LastTime, obj_item.createtime);
                                    camera.Face = obj_item;

                                } while (false);
                            }
                            TrackChanged.OnNext(true);
                        })
                    );
            /////////////////////////////////////////////////////////////

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

        public void HistoryWithDuration(DateTime start, long durationSeconds) {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            long startms = (long)start.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            startms -= (long)offset.TotalMilliseconds;
            long endms = startms + durationSeconds * 1000;

            InitialNewListen(startms, endms, false);
        }
    }
}
