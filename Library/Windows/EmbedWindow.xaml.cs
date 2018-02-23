using Library.Converters;
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

namespace Library.Windows {
    /// <summary>
    /// Interaction logic for EmbedWindow.xaml
    /// </summary>
    public partial class EmbedWindow : UserControl {
        public EmbedWindow() {
            InitializeComponent();

            this.Loaded += (object sender, RoutedEventArgs e) => {
                if (DesignerProperties.GetIsInDesignMode(this)) return;

                var win = new Window();
                win.Content = this.Content;
                {
                    Binding binding = new Binding("LeftRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenWidthConverter()
                    };
                    win.SetBinding(Window.LeftProperty, binding);
                }
                {
                    Binding binding = new Binding("TopRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenHeightConverter()
                    };
                    win.SetBinding(Window.TopProperty, binding);
                }
                {
                    Binding binding = new Binding("WidthRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenWidthConverter()
                    };
                    win.SetBinding(WidthProperty, binding);
                }
                {
                    Binding binding = new Binding("HeightRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenHeightConverter()
                    };
                    win.SetBinding(HeightProperty, binding);
                }
                win.Show();
            };
        }

        #region "Dependency Property"
        public double LeftRatio {
            get { return (double)GetValue(LeftRatioProperty); }
            set { SetValue(LeftRatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftRatioProperty =
            DependencyProperty.Register("LeftRatio", typeof(double), typeof(EmbedWindow), new PropertyMetadata(0.25));

        public double TopRatio {
            get { return (double)GetValue(TopRatioProperty); }
            set { SetValue(TopRatioProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TopRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopRatioProperty =
            DependencyProperty.Register("TopRatio", typeof(double), typeof(EmbedWindow), new PropertyMetadata(0.25));

        public double WidthRatio {
            get { return (double)GetValue(WidthRatioProperty); }
            set { SetValue(WidthRatioProperty, value); }
        }
        // Using a DependencyProperty as the backing store for WidthRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthRatioProperty =
            DependencyProperty.Register("WidthRatio", typeof(double), typeof(EmbedWindow), new PropertyMetadata(0.5));

        public double HeightRatio {
            get { return (double)GetValue(HeightRatioProperty); }
            set { SetValue(HeightRatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeightRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightRatioProperty =
            DependencyProperty.Register("HeightRatio", typeof(double), typeof(EmbedWindow), new PropertyMetadata(0.5));

        #endregion "Dependency Property"
    }
}
