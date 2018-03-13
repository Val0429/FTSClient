﻿using System;
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
            RoutedEventArgs ea = new RoutedEventArgs(FaceTracingHistory.FaceItemSelectedEvent, vm.Tag);
            base.RaiseEvent(ea);
        }

        private void MainBorder_LBIconClicked(object sender, RoutedEventArgs e) {
            if (IconType == 0) IconType = 1;
            else IconType = 0;
        }
    }
}
