﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                    if (scroll != null) {
                        NotifyCollectionChangedEventHandler scrollCollectionChanged = (object s2, NotifyCollectionChangedEventArgs e2) => {
                            if (e2.NewItems == null || scroll.Items.Count == 0) return;
                            scroll.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => {
                                scroll.ScrollIntoView(scroll.Items[scroll.Items.Count - 1]);
                            }));
                        };

                        if (alwaysScrollToEnd) {
                            ((INotifyCollectionChanged)scroll.Items).CollectionChanged += scrollCollectionChanged;
                        } else
                            ((INotifyCollectionChanged)scroll.Items).CollectionChanged -= scrollCollectionChanged;
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
