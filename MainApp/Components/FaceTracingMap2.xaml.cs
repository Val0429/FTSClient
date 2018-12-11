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
using FloorPlanMap;
using System.ComponentModel;
using System.Threading;
using Tencent.DataSources;
using FloorPlanMap.Components.Footprints;

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingMap2.xaml
    /// </summary>
    public partial class FaceTracingMap2 : UserControl {
        Dictionary<string, SideContentCameraDevice> Cameras = new Dictionary<string, SideContentCameraDevice>();

        public FaceTracingMap2() {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            InitMap();
        }

        Dictionary<int, FloorPlanMapUnit> Maps = new Dictionary<int, FloorPlanMapUnit>();
        public void InitMap() {
            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");
            Style fr_camera_template = (Style)this.FindResource("FRCameraModel");
            Style nm_camera_template = (Style)this.FindResource("NormalCameraModel");

            int initFloor = int.MaxValue;
            foreach (var value in source.Cameras.Values) {
                var floor = value.floor;
                this.FloorSwitch.AddFloor(floor);
                initFloor = Math.Min(initFloor, floor);
                FloorPlanMapUnit map = null;
                Maps.TryGetValue(floor, out map);
                /// initial map
                if (map == null) {
                    map = new FloorPlanMapUnit();

                    /// get http image
                    var image = source.Floors[floor].image;
                    map.MapSource = image;
                    //map.MapSource = string.Format("Resources/floor{0}.png", floor);

                    map.Margin = new Thickness(10, 0, 10, 0);
                    map.ClipToBounds = true;
                    Maps[floor] = map;
                }

                /// initial camera
                var camera = new SideContentCameraDevice() { sourceid = value.sourceid, X = value.X, Y = value.Y, Angle = value.Angle, Size = 0.4, Style = nm_camera_template };
                camera.MouseDown += (object sender, MouseButtonEventArgs e) => {
                    Camera oricamera = null;
                    source.Cameras.TryGetValue(((SideContentCameraDevice)sender).sourceid, out oricamera);
                    if (oricamera == null) return;
                    source.DoMapCameraClicked(oricamera);
                };
                var binding = new Binding();
                binding.Source = value;
                binding.Path = new PropertyPath("Face");
                camera.SetBinding(SideContentCameraDevice.FaceProperty, binding);
                Cameras[value.sourceid] = camera;
                map.Objects.Add(camera);
            }
            if (initFloor == int.MaxValue) {
                MessageBox.Show("You have to config <Floor> on server first.");
                Environment.Exit(0);
            }

            /// load init floor
            SwitchMap(initFloor);

            /// listen to camera change
            source.OnPlayingCameraChanged += (Camera playingcamera) => {
                Dispatcher.BeginInvoke(new Action(() => {
                    if (playingcamera != null) {
                        /// switch maps
                        SwitchMap(playingcamera.floor);
                    }

                    /// Change icon
                    foreach (var camera in Cameras.Values) {
                        camera.Style = nm_camera_template;
                        if (playingcamera == null || (playingcamera.sourceid != camera.sourceid)) {
                            camera.Style = nm_camera_template;
                        } else if (playingcamera.sourceid == camera.sourceid) {
                            camera.Style = fr_camera_template;
                        }
                    }

                    ///// Change traces icon
                    //foreach (var trace in source.FaceDetail.Traces) {
                    //    trace.
                    //    if (lastTrace != null) {
                    //        NormalFootprint footprint = new NormalFootprint() {
                    //            X = lastTrace.Camera.X,
                    //            Y = lastTrace.Camera.Y,
                    //            TargetX = trace.Camera.X,
                    //            TargetY = trace.Camera.Y,
                    //            Size = 3,
                    //            StartOpacity = ((double)(lastTrace.endtime + lastTrace.starttime) / 2 - starttime) / duration * (maxOpacity - minOpacity) + minOpacity,
                    //            TargetOpacity = ((double)(trace.endtime + trace.starttime) / 2 - starttime) / duration * (maxOpacity - minOpacity) + minOpacity,
                    //            Color = (Color)ColorConverter.ConvertFromString("CornflowerBlue"),
                    //        };
                    //        //Console.WriteLine("Start Opa: {0}, End Opa: {1}", ((lastTrace.endtime + lastTrace.starttime) / 2 - starttime), ((trace.endtime + trace.starttime) / 2 - starttime));
                    //        //Console.WriteLine("Opacity: {0} {1}", footprint.StartOpacity, footprint.TargetOpacity);
                    //        /// Opacity
                    //        //footprint.TargetOpacity

                    //        if (lastTrace.Camera.floor == trace.Camera.floor) {
                    //            Maps[lastTrace.Camera.floor].Objects.Add(footprint);
                    //        }
                    //        //mainmap.Objects.Add(footprint);
                    //        footprints.Add(footprint);
                    //    }
                    //    lastTrace = trace;
                    //}


                }));
            };


            /// Footprint!
            List<NormalFootprint> footprints = new List<NormalFootprint>();
            source.FaceDetail.Traces.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {
                TraceItem lastTrace = null;

                /// Remove previous footprints
                foreach (var footprint in footprints) {
                    foreach (var map in Maps) {
                        if (map.Value.Objects.Contains(footprint)) {
                            map.Value.Objects.Remove(footprint);
                            break;
                        }
                    }
                }
                footprints.Clear();
                if (source.FaceDetail.Traces.Count == 0) return;

                long starttime = (source.FaceDetail.Traces[0].endtime + source.FaceDetail.Traces[0].starttime) / 2;
                long endtime = (source.FaceDetail.Traces[source.FaceDetail.Traces.Count - 1].endtime + source.FaceDetail.Traces[source.FaceDetail.Traces.Count - 1].starttime) / 2;
                long duration = Math.Max(endtime - starttime, 1);
                const double minOpacity = 0.1;
                const double maxOpacity = 0.8;

                foreach (var trace in source.FaceDetail.Traces) {
                    if (lastTrace != null) {
                        NormalFootprint footprint = new NormalFootprint() {
                            X = lastTrace.Camera.X,
                            Y = lastTrace.Camera.Y,
                            TargetX = trace.Camera.X,
                            TargetY = trace.Camera.Y,
                            Size = 3,
                            StartOpacity = ((double)(lastTrace.endtime + lastTrace.starttime) / 2 - starttime) / duration * (maxOpacity - minOpacity) + minOpacity,
                            TargetOpacity = ((double)(trace.endtime + trace.starttime) / 2 - starttime) / duration * (maxOpacity - minOpacity) + minOpacity,
                            Color = (Color)ColorConverter.ConvertFromString("CornflowerBlue"),
                        };
                        //Console.WriteLine("Start Opa: {0}, End Opa: {1}", ((lastTrace.endtime + lastTrace.starttime) / 2 - starttime), ((trace.endtime + trace.starttime) / 2 - starttime));
                        //Console.WriteLine("Opacity: {0} {1}", footprint.StartOpacity, footprint.TargetOpacity);
                        /// Opacity
                        //footprint.TargetOpacity

                        if (lastTrace.Camera.floor == trace.Camera.floor) {
                            Maps[lastTrace.Camera.floor].Objects.Add(footprint);
                        }
                        //mainmap.Objects.Add(footprint);
                        footprints.Add(footprint);
                    }
                    lastTrace = trace;
                }
            };


        }

        private void SwitchMap(int floor) {
            var target = Maps[floor];
            if (!this.Main.Children.Contains(target)) {
                if (this.Main.Children.Count > 0) this.Main.Children.RemoveAt(0);
                this.Main.Children.Add(target);
            }
            /// floor label
            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");
            this.FloorLabel.Content = source.Floors.ContainsKey(floor) ?
                    source.Floors[floor].name :
                    string.Format("{0}F", floor);
        }

        private void FloorSwitch_FloorChanged(object sender, RoutedEventArgs e) {
            //MessageBox.Show(e.Source.ToString());
            SwitchMap((int)e.OriginalSource);
        }
    }
}
