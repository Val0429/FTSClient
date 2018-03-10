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

namespace TencentLibrary.ScrollViewers {
    public class CustomListView : ListView {
        static CustomListView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomListView), new FrameworkPropertyMetadata(typeof(CustomListView)));
        }

        #region "Dependency Properties"
        public Thickness ScrollBarMargin {
            get { return (Thickness)GetValue(ScrollBarMarginProperty); }
            set { SetValue(ScrollBarMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollBarMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollBarMarginProperty =
            DependencyProperty.Register("ScrollBarMargin", typeof(Thickness), typeof(CustomListView), new PropertyMetadata(new Thickness(0)));


        public Thickness ScrollContentMargin {
            get { return (Thickness)GetValue(ScrollContentMarginProperty); }
            set { SetValue(ScrollContentMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollContentMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollContentMarginProperty =
            DependencyProperty.Register("ScrollContentMargin", typeof(Thickness), typeof(CustomListView), new PropertyMetadata(new Thickness(0)));
        #endregion "Dependency Properties"

    }
}
