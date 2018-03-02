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

        #endregion "Dependency Properties"
    }
}
