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

namespace Tencent.Components.FaceTracingDetails {
    /// <summary>
    /// Interaction logic for EntryUnitBorder.xaml
    /// </summary>
    public partial class EntryUnitBorder : UserControl {
        public EntryUnitBorder() {
            InitializeComponent();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.MouseDown += (object sender, MouseButtonEventArgs e) => {
                RoutedEventArgs ea = new RoutedEventArgs(EntryUnitBorder.OnClickEvent);
                base.RaiseEvent(ea);
            };
        }

        #region "Dependency Properties"

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(EntryUnitBorder), new PropertyMetadata(null));

        public ImageSource Image {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(EntryUnitBorder), new PropertyMetadata(null));

        public Color Color {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(EntryUnitBorder), new PropertyMetadata(ColorConverter.ConvertFromString("#4D746C")));

        public Color GlowColor {
            get { return (Color)GetValue(GlowColorProperty); }
            set { SetValue(GlowColorProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register("GlowColor", typeof(Color), typeof(EntryUnitBorder), new PropertyMetadata(ColorConverter.ConvertFromString("#4D746C")));

        #endregion "Dependency Properties"

        #region "Routed Events"
        public static readonly RoutedEvent OnClickEvent = EventManager.RegisterRoutedEvent("OnClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EntryUnitBorder));
        public event RoutedEventHandler OnClick {
            add { AddHandler(OnClickEvent, value); }
            remove { RemoveHandler(OnClickEvent, value); }
        }
        #endregion "Routed Events"
    }
}
