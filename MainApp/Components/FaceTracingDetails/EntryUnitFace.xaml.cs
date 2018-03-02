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
    /// Interaction logic for EntryUnitFace.xaml
    /// </summary>
    public partial class EntryUnitFace : UserControl {
        public EntryUnitFace() {
            InitializeComponent();
        }

        #region "Dependency Properties"

        public ImageSource Image {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(EntryUnitFace), new PropertyMetadata(null));

        #endregion "Dependency Properties"

    }
}
