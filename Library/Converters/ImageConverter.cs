using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Library.Converters {
    public class ImageConverter : IValueConverter {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            var bi = new BitmapImage(new Uri(value.ToString())) {
                CacheOption = BitmapCacheOption.OnLoad
            };
            return bi;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
