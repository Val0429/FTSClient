using Library.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Library.Windows {
    public class EmbedContainerWindow : Window {
        public EmbedContainerWindow() {
            this.Loaded += (object sender, RoutedEventArgs e) => {
                {
                    Binding binding = new Binding("LeftRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenWidthConverter()
                    };
                    this.SetBinding(Window.LeftProperty, binding);
                }
                {
                    Binding binding = new Binding("TopRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenHeightConverter()
                    };
                    this.SetBinding(Window.TopProperty, binding);
                }
                {
                    Binding binding = new Binding("WidthRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenWidthConverter()
                    };
                    this.SetBinding(Window.WidthProperty, binding);
                }
                {
                    Binding binding = new Binding("HeightRatio") {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Converter = new RatioToActualScreenHeightConverter()
                    };
                    this.SetBinding(Window.HeightProperty, binding);
                }
            };
        }

        #region "Dependency Property"
        public double LeftRatio {
            get { return (double)GetValue(LeftRatioProperty); }
            set { SetValue(LeftRatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftRatioProperty =
            DependencyProperty.Register("LeftRatio", typeof(double), typeof(EmbedContainerWindow), new PropertyMetadata(0.25));

        public double TopRatio {
            get { return (double)GetValue(TopRatioProperty); }
            set { SetValue(TopRatioProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TopRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopRatioProperty =
            DependencyProperty.Register("TopRatio", typeof(double), typeof(EmbedContainerWindow), new PropertyMetadata(0.25));

        public double WidthRatio {
            get { return (double)GetValue(WidthRatioProperty); }
            set { SetValue(WidthRatioProperty, value); }
        }
        // Using a DependencyProperty as the backing store for WidthRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthRatioProperty =
            DependencyProperty.Register("WidthRatio", typeof(double), typeof(EmbedContainerWindow), new PropertyMetadata(0.5));

        public double HeightRatio {
            get { return (double)GetValue(HeightRatioProperty); }
            set { SetValue(HeightRatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeightRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightRatioProperty =
            DependencyProperty.Register("HeightRatio", typeof(double), typeof(EmbedContainerWindow), new PropertyMetadata(0.5));

        #endregion "Dependency Property"
    }
}
