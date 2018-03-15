using FloorPlanMap;
using FloorPlanMap.Components.Footprints;
using FloorPlanMap.Components.Objects.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Tencent.DataSources;

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingMap.xaml
    /// </summary>
    public partial class FaceTracingMap : UserControl {
        public FaceTracingMap() {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            FloorPlanMapUnit mainmap = this.MainMap;
            Style fr_camera_template = (Style)this.FindResource("FRCameraModel");
            Style nm_camera_template = (Style)this.FindResource("NormalCameraModel");
            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");
            foreach (var value in source.Cameras.Values) {
                var camera = new SideContentCameraDevice() { X = value.X, Y = value.Y, Angle = value.Angle, Size = 0.4, Style = value.type == 0 ? fr_camera_template : nm_camera_template };
                var binding = new Binding();
                binding.Source = value;
                binding.Path = new PropertyPath("Face");
                camera.SetBinding(SideContentCameraDevice.FaceProperty, binding);
                mainmap.Objects.Add(
                    camera
                    );
            }

            List<NormalFootprint> footprints = new List<NormalFootprint>();
            source.FaceDetail.Traces.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {
                TraceItem lastTrace = null;

                foreach (var footprint in footprints) {
                    mainmap.Objects.Remove(footprint);
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

                        mainmap.Objects.Add(footprint);
                        footprints.Add(footprint);
                    }
                    lastTrace = trace;
                }
            };
        }
    }

    public class SideContentCameraDevice : CameraDevice {

        public SearchItem Face {
            get { return (SearchItem)GetValue(FaceProperty); }
            set { SetValue(FaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Face.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FaceProperty =
            DependencyProperty.Register("Face", typeof(SearchItem), typeof(SideContentCameraDevice), new PropertyMetadata(null));

    }
}
