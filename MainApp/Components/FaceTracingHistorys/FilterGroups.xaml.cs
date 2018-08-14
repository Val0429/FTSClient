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
        private Dictionary<string, bool> permission;

        public FilterGroups() {
            InitializeComponent();

            reloadPermission();
        }

        private void reloadPermission() { permission = getPermission(); }
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
            if (permission["__all__"] == true) return true;
            bool result;
            if (name == null || permission.TryGetValue(name, out result) == false)
                return false;
            return result;
        }

        public string[] Groups() {
            if (permission["__all__"] == true) return null;
            List<string> result = new List<string>();
            foreach (var str in permission.Keys) {
                if (str == "__all__") continue;
                if (permission[str] == true) result.Add(str);
            }
            return result.ToArray();
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            reloadPermission();
        }
    }
}
