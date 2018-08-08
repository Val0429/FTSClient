using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Tencent.Components.FaceTracingDetails;
using Tencent.DataSources;

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingDetail.xaml
    /// </summary>
    public partial class FaceTracingDetail : UserControl {
        public FaceTracingDetail() {
            InitializeComponent();

            SetValue(LeftPanelProperty, new ObservableCollection<EntryUnit>());
            SetValue(RightPanelProperty, new ObservableCollection<EntryUnitFace>());

            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");
            UIElement fr_camera_template = (UIElement)this.FindResource("FRCameraTemplate");
            UIElement nm_camera_template = (UIElement)this.FindResource("NormalCameraTemplate");

            // hook playing camera event
            source.OnPlayingTimeChanged += (long timestamp) => {
                var caltracks = FaceTracingVideoMonitor.calculateTracks();

                for (var i=0; i<this.EntryList.Items.Count; ++i) {
                    ListViewItem item = this.EntryList.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    var cp = FindVisualChild<ContentPresenter>(item);
                    var tpl = cp.ContentTemplate as DataTemplate;
                    EntryUnit result = (EntryUnit)tpl.FindName("EntryUnit", cp);
                    TraceItem obj = (TraceItem)result.Tag;
                    var caltrack = caltracks[i];
                    if (caltrack.Item1 <= timestamp && caltrack.Item2 >= timestamp)
                        result.Icon = (UIElement)this.FindResource("FRCameraTemplate");
                    else
                        result.Icon = (UIElement)this.FindResource("NormalCameraTemplate");
                }
            };
        }

        #region "Dependency Properties"

        public string EntryTime {
            get { return (string)GetValue(EntryTimeProperty); }
            set { SetValue(EntryTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EntryTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EntryTimeProperty =
            DependencyProperty.Register("EntryTime", typeof(string), typeof(FaceTracingDetail), new PropertyMetadata(null));

        public ObservableCollection<EntryUnit> LeftPanel {
            get { return (ObservableCollection<EntryUnit>)GetValue(LeftPanelProperty); }
            set { SetValue(LeftPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftPanelProperty =
            DependencyProperty.Register("LeftPanel", typeof(ObservableCollection<EntryUnit>), typeof(FaceTracingDetail), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.AffectsRender
                ));

        public ObservableCollection<EntryUnitFace> RightPanel {
            get { return (ObservableCollection<EntryUnitFace>)GetValue(RightPanelProperty); }
            set { SetValue(RightPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightPanelProperty =
            DependencyProperty.Register("RightPanel", typeof(ObservableCollection<EntryUnitFace>), typeof(FaceTracingDetail), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.AffectsRender
                ));

        #endregion "Dependency Properties"

        #region "Routed Events"
        public static readonly RoutedEvent FaceItemSelectedEvent = EventManager.RegisterRoutedEvent("FaceItemSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FaceTracingDetail));
        public event RoutedEventHandler FaceItemSelected {
            add { AddHandler(FaceItemSelectedEvent, value); }
            remove { RemoveHandler(FaceItemSelectedEvent, value); }
        }
        #endregion "Routed Events"

        private void EntryUnitFace_MouseDown(object sender, MouseButtonEventArgs e) {
            var vm = sender as FrameworkElement;
            RoutedEventArgs ea = new RoutedEventArgs(FaceTracingDetail.FaceItemSelectedEvent, vm.DataContext);
            base.RaiseEvent(ea);
        }

        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T) {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }

        private void EntryUnit_MouseDown(object sender, MouseButtonEventArgs e) {
            EntryUnit unit = (EntryUnit)sender;
            TraceItem item = (TraceItem)unit.Tag;
            FaceListenerSource source = (FaceListenerSource)this.FindResource("FaceListenerSource");
            source.DoMapCameraClicked(item.Camera);
        }

        private void MainBorder_RTMaximumClicked(object sender, RoutedEventArgs e) {
            this.MainBorder.IsMaximum = !this.MainBorder.IsMaximum;
        }

    }
}
