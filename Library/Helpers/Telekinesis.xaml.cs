using Library.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

namespace Library.Helpers {
    public partial class Telekinesis : Canvas {
        internal class EmbedValue {
            public EmbedWindow window { get; set; }
            public Visual parent { get; set; }
            public Visual element { get; set; }
            public TaskCompletionSource<bool> completion { get; set; }
        }

        public TimeSpan preTeleportDuration { get; set; } = TimeSpan.FromMilliseconds(300);
        public TimeSpan postRecallDuration { get; set; } = TimeSpan.FromMilliseconds(500);

        public Telekinesis() {
            InitializeComponent();
        }

        private Dictionary<string, EmbedValue> CollectedParent = new Dictionary<string, EmbedValue>();

        private object GetInternalField(object instance, string name) {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            PropertyInfo field = instance.GetType().GetProperty(name, bindFlags);
            return field.GetValue(instance);
        }

        private string GetAddress(object element) {
            return element.GetHashCode().ToString();
            /// Old method to get address
            //GCHandle handle = GCHandle.Alloc(element, GCHandleType.Normal);
            //try {
            //    IntPtr pointer = GCHandle.ToIntPtr(handle);
            //    return "0x" + pointer.ToString("X");
            //}
            //finally {
            //    handle.Free();
            //}
        }

        public TaskCompletionSource<bool> Teleport(UIElement element, EmbedWindow window = null) {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();

            /// Put into EmbedWindow
            if (window == null) {
                window = new EmbedWindow() {
                    LeftRatio = 0.25,
                    TopRatio = 0.25,
                    WidthRatio = 0.5,
                    HeightRatio = 0.5,
                };
            }
            window.Content = element;

            /// Collect
            var embedValue = new EmbedValue() {
                element = element,
                parent = (Visual)GetInternalField(element, "VisualParent"),
                window = window,
                completion = task
            };
            CollectedParent.Add(GetAddress(element), embedValue);

            /// Disable Element Until Animation Finished
            element.IsHitTestVisible = false;

            /// Prepare storyboard
            var sb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation() {
                To = 0,
                Duration = preTeleportDuration,
                FillBehavior = FillBehavior.Stop
            };
            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
            sb.Children.Add(da);

            sb.Completed += (object s3, EventArgs e3) => {
                sb.Children.Remove(da);
                element.Opacity = 1;

                /// Enable Element
                element.IsHitTestVisible = true;

                /// Clean Parent
                if (embedValue.parent.GetType() == typeof(ContentPresenter)) {
                    ((ContentPresenter)embedValue.parent).Content = null;
                }

                /// Hook Event
                window.Unloaded += (object sender, RoutedEventArgs e) => {
                    this.Recall(element);
                };

                /// Show
                this.Children.Add(window);
            };
            sb.Begin();

            return task;
        }

        public void Recall(UIElement element) {
            var address = GetAddress(element);
            EmbedValue embedValue = null;
            CollectedParent.TryGetValue(address, out embedValue);

            /// Remove EmbedWindow
            if (embedValue != null) {
                CollectedParent.Remove(address);
                this.Children.Remove(embedValue.window);

                /// Disable Element Until Animation Finished
                element.IsHitTestVisible = false;
                element.Opacity = 0;

                /// Put Content back, if ContentPresenter
                if (embedValue.parent.GetType() == typeof(ContentPresenter)) {
                    ((ContentPresenter)embedValue.parent).Content = embedValue.element;
                }

                /// Prepare storyboard
                var sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation() {
                    To = 1,
                    Duration = preTeleportDuration,
                    FillBehavior = FillBehavior.Stop
                };
                Storyboard.SetTarget(da, element);
                Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
                sb.Children.Add(da);

                sb.Completed += (object s3, EventArgs e3) => {
                    sb.Children.Remove(da);
                    element.Opacity = 1;

                    /// Enable Element
                    element.IsHitTestVisible = true;

                    embedValue.completion.TrySetResult(true);
                };
                sb.Begin();
            }
        }
    }
}