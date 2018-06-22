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

namespace Tencent.Components.FaceTracingMaps {
    /// <summary>
    /// Interaction logic for FloorSwitch.xaml
    /// </summary>
    public partial class FloorSwitch : UserControl {
        public ObservableCollection<int> Floors { get; private set; }

        public FloorSwitch() {
            InitializeComponent();

            Floors = new ObservableCollection<int>();
        }

        public void AddFloor(int floor) {
            if (!Floors.Contains(floor))
                Floors.Add(floor);
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Button button = sender as Button;
            RoutedEventArgs ea = new RoutedEventArgs(FloorSwitch.FloorChangedEvent, button.Tag);
            base.RaiseEvent(ea);
        }

        #region "Routed Events"
        public static readonly RoutedEvent FloorChangedEvent = EventManager.RegisterRoutedEvent("FloorChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FloorSwitch));
        public event RoutedEventHandler FloorChanged {
            add { AddHandler(FloorChangedEvent, value); }
            remove { RemoveHandler(FloorChangedEvent, value); }
        }
        #endregion "Routed Events"

    }
}
