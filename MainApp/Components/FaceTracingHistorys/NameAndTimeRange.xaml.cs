using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tencent.Helpers;
using Xceed.Wpf.Toolkit;

namespace Tencent.Components.FaceTracingHistorys {
    /// <summary>
    /// Interaction logic for NameAndTimeRange.xaml
    /// </summary>
    public partial class NameAndTimeRange : UserControl {
        public NameAndTimeRange() {
            InitializeComponent();

            this.calendar.Text = DateTime.Now.ToString("yyyy/MM/dd 08:00");
        }

        public TextBox getNameTextBox() {
            return this.txt_FilterName;
        }
    }
}
