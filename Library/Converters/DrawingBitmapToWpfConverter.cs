using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Security;
using System.Windows.Media;

namespace Library.Converters {
    public class DrawingBitmapToWpfConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var source = (System.Drawing.Bitmap)value;
            System.Windows.Media.Imaging.BitmapSource target = null;
            var handle = new SafeHBitmapHandle(source.GetHbitmap());
            try {
                using (handle) {
                    target = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        handle.DangerousGetHandle(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }

            } catch (Win32Exception) {
                target = null;
            }
            return target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    internal static class NativeMethods {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }

    public class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid {
        [SecurityCritical]
        public SafeHBitmapHandle(IntPtr preexistingHandle)
            : base(true) {
            SetHandle(preexistingHandle);
        }

        protected override bool ReleaseHandle() {
            return NativeMethods.DeleteObject(handle);
        }
    }

}
