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
using Tencent.DataSources;

namespace Tencent.Components.FaceTracingDetails {
    /// <summary>
    /// Interaction logic for EntryUnitxaml.xaml
    /// </summary>
    [ContentProperty("CustomContent")]
    public partial class EntryUnit : UserControl {
        public EntryUnit() {
            InitializeComponent();

            SetValue(IconProperty, this.FindResource("NormalCameraTemplate"));
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
                new ObservableCollection<UIElement>(), FrameworkPropertyMetadataOptions.AffectsRender
                ));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(EntryUnit), new PropertyMetadata(null));



        public UIElement Icon {
            get { return (UIElement)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(UIElement), typeof(EntryUnit), new PropertyMetadata(null));


        #endregion "Denepdency Properties"
    }
}
