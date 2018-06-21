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

namespace Tencent.Components.FaceTracingVideoMonitors {

    [TemplatePart(Name = "ExportButton", Type = typeof(Button))]

    public class TimeTrackUnit : Control {
        static TimeTrackUnit() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeTrackUnit), new FrameworkPropertyMetadata(typeof(TimeTrackUnit)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            Button button = base.GetTemplateChild("ExportButton") as Button;
            button.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                RoutedEventArgs ea = new RoutedEventArgs(TimeTrackUnit.ExportClickedEvent);
                base.RaiseEvent(ea);
            }));
        }

        #region "Routed Events"
        public static readonly RoutedEvent ExportClickedEvent = EventManager.RegisterRoutedEvent("ExportClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimeTrackUnit));
        public event RoutedEventHandler ExportClicked {
            add { AddHandler(ExportClickedEvent, value); }
            remove { RemoveHandler(ExportClickedEvent, value); }
        }
        #endregion "Routed Events"

    }
}
