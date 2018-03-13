using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace TencentLibrary.Borders {
    public class FaceTracingBorder : Control {
        static FaceTracingBorder() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FaceTracingBorder), new FrameworkPropertyMetadata(typeof(FaceTracingBorder)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.MouseDown += (object sender, MouseButtonEventArgs e) => {
                RoutedEventArgs ea = new RoutedEventArgs(FaceTracingBorder.OnClickEvent);
                base.RaiseEvent(ea);
            };
        }

        #region "Dependency Properties"
        public ImageSource Image {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(FaceTracingBorder), new PropertyMetadata(null));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FaceTracingBorder), new PropertyMetadata(null));


        public HorizontalAlignment TitleAlignment {
            get { return (HorizontalAlignment)GetValue(TitleAlignmentProperty); }
            set { SetValue(TitleAlignmentProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TitleAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleAlignmentProperty =
            DependencyProperty.Register("TitleAlignment", typeof(HorizontalAlignment), typeof(FaceTracingBorder), new PropertyMetadata(HorizontalAlignment.Right));

        #endregion "Dependency Properties"

        #region "Routed Events"
        public static readonly RoutedEvent OnClickEvent = EventManager.RegisterRoutedEvent("OnClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FaceTracingBorder));
        public event RoutedEventHandler OnClick {
            add { AddHandler(OnClickEvent, value); }
            remove { RemoveHandler(OnClickEvent, value); }
        }
        #endregion "Routed Events"
    }
}
