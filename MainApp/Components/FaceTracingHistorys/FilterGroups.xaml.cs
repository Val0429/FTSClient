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
using Tencent.Helpers;

namespace Tencent.Components.FaceTracingHistorys {
    /// <summary>
    /// Interaction logic for FilterGroups.xaml
    /// </summary>
    public partial class FilterGroups : UserControl {
        public FilterGroups() {
            InitializeComponent();
        }

        private Dictionary<string, bool> getPermission() {
            var checkboxes = this.FindVisualChildren<CheckBox>();
            var dict = new Dictionary<string, bool>();
            var selected = 0;
            foreach (var cb in checkboxes) {
                if (cb.IsChecked == true) selected++;
                dict[cb.Content as string] = (bool)cb.IsChecked;
            }
            if (selected == 0 || selected == checkboxes.Count()) dict["__all__"] = true;
            else dict["__all__"] = false;
            return dict;
        }

        public bool CheckGroupValid(string name) {
            var permission = getPermission();
            try {
                return permission[name];
            } catch (Exception) {
                return permission["__all__"] ? true : false;
            }
        }

        private void btn_FilterSelectAll_Click(object sender, RoutedEventArgs e) {
            var checkboxes = this.FindVisualChildren<CheckBox>();
            var selected = 0;
            foreach (var cb in checkboxes) {
                if (cb.IsChecked == true) selected++;
            }
            foreach (var cb in checkboxes) {
                cb.IsChecked = selected == 0 ? true : false;
            }
        }

    }
}
