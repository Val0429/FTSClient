using Library.Windows;
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
using Tencent.DataSources;

namespace Tencent {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : EmbedContainerWindow {
        public MainWindow() {
            InitializeComponent();
        }

        private void FaceTracingHistory_FaceItemSelected(object sender, RoutedEventArgs e) {
            FaceItem faceitem = null;
            if (e.OriginalSource.GetType() == typeof(SearchItem)) {
                var searchitem = (SearchItem)e.OriginalSource;
                faceitem = new FaceItem() {
                    createtime = searchitem.createtime,
                    image = searchitem.image,
                    sourceid = searchitem.sourceid
                };
            } else {
                faceitem = (FaceItem)e.OriginalSource;
            }
            ((FaceListenerSource)this.FindResource("FaceListenerSource")).StartSearch(faceitem);
        }

    }
}
