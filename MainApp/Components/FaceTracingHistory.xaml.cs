using Infralution.Localization.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
using TencentLibrary.Borders;
using Tencent.Helpers;
using Tencent.Components.FaceTracingHistorys;

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingHistory.xaml
    /// </summary>
    public partial class FaceTracingHistory : UserControl {
        private DependencyObject filterContent;
        private FilterGroups filterGroup;
        private FilterCameras filterCamera;
        private TextBox filterName;

        public FaceTracingHistory() {
            InitializeComponent();

            filterContent = this.FindResource("FilterContent") as DependencyObject;
            filterGroup = filterContent.FindVisualChildren<FilterGroups>().First();
            filterCamera = filterContent.FindVisualChildren<FilterCameras>().First();
            filterNameTime = filterContent.FindVisualChildren<NameAndTimeRange>().First();
            filterName = filterNameTime.getNameTextBox();

            applyFilterToView();
        }

        private void applyFilterToView() {
            CollectionView view = CollectionViewSource.GetDefaultView((this.FindResource("MainContent") as ListView).ItemsSource) as CollectionView;

            if (view.Filter == null) view.Filter = (object item) => {
                FaceItem face = (FaceItem)item;

                /// validate name
                if ((filterName != null && filterName.Text != "") &&
                    (face.name == null || (face.name != null) && face.name.IndexOf(filterName.Text) < 0)
                    ) return false;

                /// validate groups
                //return filterGroup.CheckGroupValid(face.groupname);
                if (filterGroup.CheckGroupValid(face.groupname) == false) return false;

                /// validate cameras
                return filterCamera.CheckCameraValid(face.sourceid);
            };
        }

        #region "Dependency Properties"

        public int IconType {
            get { return (int)GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IconType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register("IconType", typeof(int), typeof(FaceTracingHistory), new PropertyMetadata(0));

        public int FilterType {
            get { return (int)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterTypeProperty =
            DependencyProperty.Register("FilterType", typeof(int), typeof(FaceTracingHistory), new PropertyMetadata(0));

        public string FilterName {
            get { return (string)GetValue(FilterNameProperty); }
            set { SetValue(FilterNameProperty, value); }
        }
        // Using a DependencyProperty as the backing store for FilterName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterNameProperty =
            DependencyProperty.Register("FilterName", typeof(string), typeof(FaceTracingHistory), new PropertyMetadata(
                null
                ));

        public NameAndTimeRange filterNameTime {
            get { return (NameAndTimeRange)GetValue(filterNameTimeProperty); }
            set { SetValue(filterNameTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for filterNameTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty filterNameTimeProperty =
            DependencyProperty.Register("filterNameTime", typeof(NameAndTimeRange), typeof(FaceTracingHistory), new PropertyMetadata(
                null
                ));

        #endregion "Dependency Properties"

        #region "Routed Events"
        public static readonly RoutedEvent FaceItemSelectedEvent = EventManager.RegisterRoutedEvent("FaceItemSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FaceTracingHistory));
        public event RoutedEventHandler FaceItemSelected {
            add { AddHandler(FaceItemSelectedEvent, value); }
            remove { RemoveHandler(FaceItemSelectedEvent, value); }
        }
        #endregion "Routed Events"

        private void FaceTracingBorder_MouseDown(object sender, RoutedEventArgs e) {
            var vm = e.OriginalSource as FaceTracingBorder;
            RoutedEventArgs ea;
            if (vm.DataContext.GetType() == typeof(FaceItem)) {
                ea = new RoutedEventArgs(FaceTracingHistory.FaceItemSelectedEvent, vm.DataContext);
                base.RaiseEvent(ea);
            }
        }

        private void EntryUnitBorder_MouseDown(object sender, RoutedEventArgs e) {
            var vm = e.OriginalSource as EntryUnitBorder;
            RoutedEventArgs ea = new RoutedEventArgs(FaceTracingHistory.FaceItemSelectedEvent, vm.DataContext);
            base.RaiseEvent(ea);
        }

        private void LBIcon_MouseDown(object sender, MouseButtonEventArgs e) {
            if (IconType == 0) IconType = 1;
            else IconType = 0;
        }

        private void FilterIcon_MouseDown(object sender, MouseButtonEventArgs e) {
            if (FilterType == 0) FilterType = 1;
            else FilterType = 0;

            this.ContentArea.Content = FilterType == 0 ? this.FindResource("MainContent") : this.FindResource("FilterContent");
        }

        private void apply_button_Click(object sender, RoutedEventArgs e) {
            FilterType = 0;

            FaceListenerSource FaceListener = this.FindResource("FaceListenerSource") as FaceListenerSource;
            var MainContent = this.FindResource("MainContent") as ListView;
            var FilterContent = this.FindResource("FilterContent") as FrameworkElement;
            var txtFilterName = FilterContent.FindVisualChildren<TextBox>("txt_FilterName").First();
            FilterName = txtFilterName.Text;

            /// determine ItemsSource
            if (filterNameTime.rb_realtime.IsChecked == true) {
                //MainContent.ItemsSource = FaceListener.Faces;
                //applyFilterToView();
                FaceListener.InitialNewListen();

            } else {
                //MainContent.ItemsSource = FaceListener.TimeRangeFaces;
                //applyFilterToView();
                FaceListener.HistoryWithDuration(
                    DateTime.Parse((string)filterNameTime.calendar.Text),
                    long.Parse(filterNameTime.txt_duration.Tag.ToString()) * 60
                    );
            }

            this.ContentArea.Content = MainContent;
            (CollectionViewSource.GetDefaultView(MainContent.ItemsSource) as CollectionView).Refresh();
        }

        private void MainBorder_RTMaximumClicked(object sender, RoutedEventArgs e) {
            this.MainBorder.IsMaximum = !this.MainBorder.IsMaximum;
        }
    }
}
