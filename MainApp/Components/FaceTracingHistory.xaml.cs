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

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingHistory.xaml
    /// </summary>
    public partial class FaceTracingHistory : UserControl {
        public FaceTracingHistory() {
            InitializeComponent();

            SetValue(PanelProperty, new ObservableCollection<FaceTracingBorder>());

            CollectionView view = CollectionViewSource.GetDefaultView((this.FindResource("MainContent") as ListView).ItemsSource) as CollectionView;
            view.Filter = (object item) => {
                FaceItem face = (FaceItem)item;
                if (FilterName != null && FilterName.Length > 0) {
                    if (face.name != null && face.name.IndexOf(FilterName) >= 0) return true;
                    return false;
                }

                if (FilterGroupAll == true) return true;
                if (FilterGroupVIP == true && face.groupname == "VIP") return true;
                if (FilterGroupBlacklist == true && face.groupname == "Blacklist") return true;
                if (FilterGroupStranger == true && face.groupname == "No Match") return true;

                return false;
            };
        }

        #region "Dependency Properties"

        public ObservableCollection<FaceTracingBorder> Panel {
            get { return (ObservableCollection<FaceTracingBorder>)GetValue(PanelProperty); }
            set { SetValue(PanelProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Panel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanelProperty =
            DependencyProperty.Register("Panel", typeof(ObservableCollection<FaceTracingBorder>), typeof(FaceTracingHistory), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

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
                //null
                /// workaround, todo remove
                "Val"
                /// workaround, todo remove
                ));

        public bool FilterGroupAll {
            get { return (bool)GetValue(FilterGroupAllProperty); }
            set { SetValue(FilterGroupAllProperty, value); }
        }
        // Using a DependencyProperty as the backing store for FilterGroupAll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterGroupAllProperty =
            DependencyProperty.Register("FilterGroupAll", typeof(bool), typeof(FaceTracingHistory), new PropertyMetadata(true));

        public bool FilterGroupVIP {
            get { return (bool)GetValue(FilterGroupVIPProperty); }
            set { SetValue(FilterGroupVIPProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterGroupVIP.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterGroupVIPProperty =
            DependencyProperty.Register("FilterGroupVIP", typeof(bool), typeof(FaceTracingHistory), new PropertyMetadata(false));



        public bool FilterGroupBlacklist {
            get { return (bool)GetValue(FilterGroupBlacklistProperty); }
            set { SetValue(FilterGroupBlacklistProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterGroupBlacklist.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterGroupBlacklistProperty =
            DependencyProperty.Register("FilterGroupBlacklist", typeof(bool), typeof(FaceTracingHistory), new PropertyMetadata(false));



        public bool FilterGroupStranger {
            get { return (bool)GetValue(FilterGroupStrangerProperty); }
            set { SetValue(FilterGroupStrangerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterGroupStranger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterGroupStrangerProperty =
            DependencyProperty.Register("FilterGroupStranger", typeof(bool), typeof(FaceTracingHistory), new PropertyMetadata(false));



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

        private void button_Click(object sender, RoutedEventArgs e) {
            FilterType = 0;
            this.ContentArea.Content = this.FindResource("MainContent");
            (CollectionViewSource.GetDefaultView((this.FindResource("MainContent") as ListView).ItemsSource) as CollectionView).Refresh();
        }

        private void Filter_Group_All_Click(object sender, RoutedEventArgs e) {
            //CheckBox cb = (CheckBox)sender;

            //((this.FindResource("FilterContent") as FrameworkElement).FindName("Filter_Group_VIP") as CheckBox).IsChecked = cb.IsChecked;

            //(LogicalTreeHelper.FindLogicalNode(this, "Filter_Group_VIP") as CheckBox).IsChecked = cb.IsChecked;
            //(LogicalTreeHelper.FindLogicalNode(this, "Filter_Group_Blacklist") as CheckBox).IsChecked = cb.IsChecked;
            //(LogicalTreeHelper.FindLogicalNode(this, "Filter_Group_Stranger") as CheckBox).IsChecked = cb.IsChecked;
        }

        private void MainBorder_RTMaximumClicked(object sender, RoutedEventArgs e) {
            this.MainBorder.IsMaximum = !this.MainBorder.IsMaximum;
        }
    }
}
