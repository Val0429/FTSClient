using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Library.Extensions {
    static class ChildHelper {
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }

    public class ScrollViewerExtension {
        public static readonly DependencyProperty AlwaysScrollToEndProperty = DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(ScrollViewerExtension), new PropertyMetadata(false, AlwaysScrollToEndChanged));
        private static bool _autoScroll;

        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Dictionary<int, DispatcherOperation> stored = new Dictionary<int, DispatcherOperation>();

            do {
                bool alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
                ///// try ScrollViewer
                //{
                //    ScrollViewer scroll = sender as ScrollViewer;
                //    if (scroll != null) {
                //        if (alwaysScrollToEnd) {
                //            scroll.ScrollToEnd();
                //            scroll.ScrollChanged += ScrollChanged;
                //        } else { scroll.ScrollChanged -= ScrollChanged; }
                //        return;
                //    }
                //}
                /// try ListView
                {
                    ListView scroll = sender as ListView;
                    bool atBottom = true;
                    if (scroll != null) {
                        RoutedEventHandler scrollChanged = (object s2, RoutedEventArgs e2) => {
                            ScrollViewer sv = scroll.GetChildOfType<ScrollViewer>();
                            if (sv.VerticalOffset == sv.ScrollableHeight) atBottom = true;
                            else atBottom = false;
                        };

                        NotifyCollectionChangedEventHandler scrollCollectionChanged = (object s2, NotifyCollectionChangedEventArgs e2) => {
                            if (e2.NewItems == null || scroll.Items.Count == 0) return;

                            if (atBottom == true) {
                                var hash = scroll.GetHashCode();
                                DispatcherOperation op;
                                stored.TryGetValue(hash, out op);
                                if (op != null && op.Status != DispatcherOperationStatus.Completed) return;

                                var dispatcher = scroll.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => {
                                    scroll.ScrollIntoView(scroll.Items[scroll.Items.Count - 1]);
                                }));
                                stored[hash] = dispatcher;
                            }
                        };

                        if (alwaysScrollToEnd) {
                            ((INotifyCollectionChanged)scroll.Items).CollectionChanged += scrollCollectionChanged;
                            scroll.AddHandler(ScrollBar.ScrollEvent, scrollChanged);

                        } else {
                            ((INotifyCollectionChanged)scroll.Items).CollectionChanged -= scrollCollectionChanged;
                            scroll.RemoveHandler(ScrollBar.ScrollEvent, scrollChanged);
                        }
                        return;
                    }
                }

            } while (false);

            throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer or ListView instances.");
        }

        public static bool GetAlwaysScrollToEnd(DependencyObject scroll) {
            if (scroll == null) { throw new ArgumentNullException("scroll"); }
            return (bool)scroll.GetValue(AlwaysScrollToEndProperty);
        }

        public static void SetAlwaysScrollToEnd(DependencyObject scroll, bool alwaysScrollToEnd) {
            if (scroll == null) { throw new ArgumentNullException("scroll"); }
            scroll.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e) {
            ScrollViewer scroll = sender as ScrollViewer;
            if (scroll == null) { throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances."); }

            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0) { _autoScroll = scroll.VerticalOffset == scroll.ScrollableHeight; }

            // Content scroll event : autoscroll eventually
            if (_autoScroll && e.ExtentHeightChange != 0) { scroll.ScrollToVerticalOffset(scroll.ExtentHeight); }
        }

    }

}
