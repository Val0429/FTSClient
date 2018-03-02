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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tencent.Components.FaceTracingDetails {
    /// <summary>
    /// Interaction logic for EntryUnitxaml.xaml
    /// </summary>
    [ContentProperty("CustomContent")]
    public partial class EntryUnit : UserControl {
        public EntryUnit() {
            InitializeComponent();

            SetValue(CustomContentProperty, new ObservableCollection<UIElement>());
        }

        #region "Dependency Properties"

        public ObservableCollection<UIElement> CustomContent {
            get { return (ObservableCollection<UIElement>)GetValue(CustomContentProperty); }
            set { SetValue(CustomContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register("CustomContent", typeof(ObservableCollection<UIElement>), typeof(EntryUnit), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.AffectsRender
                ));

        #endregion "Denepdency Properties"
    }
}
