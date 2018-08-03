using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Tencent.Helpers {
    public static class DependencyObjectHelper {
        //public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent, string name = null) where T : DependencyObject {
        //    int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < childrenCount; i++) {
        //        var child = VisualTreeHelper.GetChild(parent, i);

        //        var childType = child as T;
        //        if ( childType != null && (name == null || (childType as FrameworkElement).Name == name) ) {
        //            yield return (T)child;
        //        }

        //        foreach (var other in FindVisualChildren<T>(child, name)) {
        //            yield return other;
        //        }
        //    }
        //}

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent, string name = null) where T : DependencyObject {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);

                var childType = child as T;
                if (childType != null && (name == null || (childType as FrameworkElement).Name == name)) {
                    yield return (T)child;
                }

                foreach (var other in child.FindVisualChildren<T>(name)) {
                    yield return other;
                }
            }
        }

    }
}
