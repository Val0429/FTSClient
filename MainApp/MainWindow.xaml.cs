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
                    createtime = searchitem.createtime,
                    name = searchitem.name,
                    image = searchitem.image,
                    sourceid = searchitem.sourceid
                };
            } else {
                faceitem = (FaceItem)e.OriginalSource;
            }
            ((FaceListenerSource)this.FindResource("FaceListenerSource")).StartSearch(faceitem);
        }

        private EmbedWindow HistoryWindow = null;
        private void FaceTracingHistory_RTMaximumClicked(object sender, RoutedEventArgs e) {
            if (((MainBorder)e.OriginalSource).IsMaximum == false) {
                UIElement panel1 = (UIElement)this.FindResource("Panel1");
                ContentPresenter holder = this.Panel1Holder;
                HistoryWindow = new EmbedWindow() {
                    LeftRatio = 0,
                    TopRatio = 0,
                    WidthRatio = 0.5,
                    HeightRatio = 0.5,
                    Content = panel1
                };

                HistoryWindow.Unloaded += (object s2, RoutedEventArgs e2) => {
                    panel1.Opacity = 0;
                    holder.Content = panel1;
                    ((MainBorder)e.OriginalSource).IsMaximum = true;

                    var sb2 = new Storyboard();
                    DoubleAnimation da2 = new DoubleAnimation() {
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                        FillBehavior = FillBehavior.Stop
                    };
                    Storyboard.SetTarget(da2, panel1);
                    Storyboard.SetTargetProperty(da2, new PropertyPath("Opacity"));
                    sb2.Children.Add(da2);

                    sb2.Completed += (object s3, EventArgs e3) => {
                        sb2.Children.Remove(da2);
                        panel1.Opacity = 1;
                    };
                    sb2.Begin();
                };

                /// Fade Out Animation ///
                var sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation() {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    FillBehavior = FillBehavior.Stop,
                };
                Storyboard.SetTarget(da, panel1);
                Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
                sb.Children.Add(da);

                sb.Completed += (object s2, EventArgs e2) => {
                    holder.Content = null;
                    sb.Children.Remove(da);
                    panel1.Opacity = 1;
                    this.MainGrid.Children.Add(HistoryWindow);
                };
                sb.Begin();
                /// Fade Out Animation ///

            } else {
                this.MainGrid.Children.Remove(HistoryWindow);
                HistoryWindow = null;
            }
        }

        private EmbedWindow DetailWindow = null;
        private void FaceTracingDetail_RTMaximumClicked(object sender, RoutedEventArgs e) {
            if (((MainBorder)e.OriginalSource).IsMaximum == false) {
                UIElement panel1 = (UIElement)this.FindResource("Panel2");
                ContentPresenter holder = this.Panel2Holder;

                DetailWindow = new EmbedWindow() {
                    LeftRatio = 0.5,
                    TopRatio = 0,
                    WidthRatio = 0.5,
                    HeightRatio = 0.5,
                    Content = panel1
                };

                DetailWindow.Unloaded += (object s2, RoutedEventArgs e2) => {
                    panel1.Opacity = 0;
                    holder.Content = panel1;
                    ((MainBorder)e.OriginalSource).IsMaximum = true;

                    var sb2 = new Storyboard();
                    DoubleAnimation da2 = new DoubleAnimation() {
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                        FillBehavior = FillBehavior.Stop
                    };
                    Storyboard.SetTarget(da2, panel1);
                    Storyboard.SetTargetProperty(da2, new PropertyPath("Opacity"));
                    sb2.Children.Add(da2);

                    sb2.Completed += (object s3, EventArgs e3) => {
                        sb2.Children.Remove(da2);
                        panel1.Opacity = 1;
                    };
                    sb2.Begin();
                };

                /// Fade Out Animation ///
                var sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation() {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    FillBehavior = FillBehavior.Stop,
                };
                Storyboard.SetTarget(da, panel1);
                Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
                sb.Children.Add(da);

                sb.Completed += (object s2, EventArgs e2) => {
                    holder.Content = null;
                    sb.Children.Remove(da);
                    panel1.Opacity = 1;
                    this.MainGrid.Children.Add(DetailWindow);
                };
                sb.Begin();
                /// Fade Out Animation ///

            } else {
                this.MainGrid.Children.Remove(DetailWindow);
                DetailWindow = null;
            }
        }

    }
}
