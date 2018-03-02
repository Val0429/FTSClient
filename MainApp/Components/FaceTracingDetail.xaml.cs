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

namespace Tencent.Components {
    /// <summary>
    /// Interaction logic for FaceTracingDetail.xaml
    /// </summary>
    public partial class FaceTracingDetail : UserControl {
        public FaceTracingDetail() {
            InitializeComponent();
        }

        #region "Dependency Properties"

        public string EntryTime {
            get { return (string)GetValue(EntryTimeProperty); }
            set { SetValue(EntryTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EntryTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EntryTimeProperty =
            DependencyProperty.Register("EntryTime", typeof(string), typeof(FaceTracingDetail), new PropertyMetadata(null));

        #endregion "Dependency Properties"
    }
}
