using Library.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tencent.DataSources;
using TencentLibrary.Borders;

namespace Tencent {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : EmbedContainerWindow {
        public MainWindow() {
            InitializeComponent();

            //var maxThreshold = TimeSpan.FromMilliseconds(750);
            //var previous = DateTime.Now;

            //string name = null;
            //List<string> lists = new List<string>();
            //Application.Current.MainWindow
            //   .Dispatcher.Hooks.DispatcherInactive += (s, eventArgs) => {
            //       var current = DateTime.Now;
            //       var delta = current - previous;

            //       previous = current;

            //       if (delta > maxThreshold) {
            //           Debug.WriteLine("{0}", string.Join("\r\n", lists.ToArray()));
            //           Debug.WriteLine("UI Freeze = {0} ms, last: {1}", delta.TotalMilliseconds, name);
            //           lists.Clear();
            //           //worker.DoWork += (object se, DoWorkEventArgs ex) => File.AppendAllText(fname, string.Format("UI Freeze = {0} ms, last: {1}", delta.TotalMilliseconds, name));
            //       }
            //   };

            //Application.Current.MainWindow
            //    .Dispatcher.Hooks.OperationPosted += (object sender, System.Windows.Threading.DispatcherHookEventArgs e) => {
            //        var privateName = e.Operation.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic);

            //        name = (string)privateName.GetValue(e.Operation);
            //        lists.Add(name);

            //        //worker.DoWork += (object s, DoWorkEventArgs ex) => {
            //        //    File.AppendAllText(fname, name);
            //        //    return;
            //        //};
            //        //Console.WriteLine("Method Dispatched: {0}", privateName.GetValue(e.Operation));
            //    };
        }

        private void FaceTracingHistory_FaceItemSelected(object sender, RoutedEventArgs e) {
            FaceItem faceitem = null;
            if (e.OriginalSource.GetType() == typeof(SearchItem)) {
                var searchitem = (SearchItem)e.OriginalSource;
                faceitem = new FaceItem() {
                    channel = searchitem.channel,
                    createtime = searchitem.createtime,
                    groups = searchitem.groups,
                    groupname = searchitem.groupname,
                    image = searchitem.image,
                    name = searchitem.name,
                    person_info = searchitem.person_info,
                    snapshot = searchitem.snapshot,
                    sourceid = searchitem.sourceid,
                    timestamp = searchitem.timestamp,
                    type = searchitem.type
                };
            } else {
                faceitem = (FaceItem)e.OriginalSource;
            }
            ((FaceListenerSource)this.FindResource("FaceListenerSource")).StartSearch(faceitem);
        }

        private void FaceTracingHistory_RTMaximumClicked(object sender, RoutedEventArgs e) {
            UIElement panel = (UIElement)this.FindResource("Panel1");

            if (((MainBorder)e.OriginalSource).IsMaximum == false) {
                var task = this.Telekinesis.Teleport(panel);
                task.Task.GetAwaiter().OnCompleted(new Action(() => {
                    ((MainBorder)e.OriginalSource).IsMaximum = true;
                }));

            } else {
                this.Telekinesis.Recall(panel);
            }
        }

        private void FaceTracingDetail_RTMaximumClicked(object sender, RoutedEventArgs e) {
            UIElement panel = (UIElement)this.FindResource("Panel2");

            if (((MainBorder)e.OriginalSource).IsMaximum == false) {
                var task = this.Telekinesis.Teleport(panel);
                task.Task.GetAwaiter().OnCompleted(new Action(() => {
                    ((MainBorder)e.OriginalSource).IsMaximum = true;
                }));

            } else {
                this.Telekinesis.Recall(panel);
            }

        }

    }
}
