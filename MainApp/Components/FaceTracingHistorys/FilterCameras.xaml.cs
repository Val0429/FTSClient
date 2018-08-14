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
using Tencent.DataSources;

namespace Tencent.Components.FaceTracingHistorys {
    /// <summary>
    /// Interaction logic for FilterCameras.xaml
    /// </summary>
    public partial class FilterCameras : UserControl {
        private Dictionary<string, bool> permission;

        public FilterCameras() {
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
                dict[(cb.Tag as Camera).sourceid] = (bool)cb.IsChecked;
            }
            if (selected == 0 || selected == checkboxes.Count()) dict["__all__"] = true;
            else dict["__all__"] = false;
            return dict;
        }

        public string[] Cameras() {
            if (permission["__all__"] == true) return null;
            List<string> result = new List<string>();
            foreach (var str in permission.Keys) {
                if (str == "__all__") continue;
                if (permission[str] == true) result.Add(str);
            }
            return result.ToArray();
        }

        public bool CheckCameraValid(string sourceid) {
            if (permission["__all__"] == true) return true;
            bool result;
            if (sourceid == null || permission.TryGetValue(sourceid, out result) == false)
                return false;
            return result;
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
