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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TencentLibrary.Borders {
    [ContentProperty("Content")]
    public class SubBorder : ContentControl {
        static SubBorder() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SubBorder), new FrameworkPropertyMetadata(typeof(SubBorder)));
        }
    }
}
