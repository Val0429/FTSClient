using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Library.Converters {
    [ValueConversion(typeof(Color), typeof(Color))]
    public class OpacityColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            /// value: Color color
            /// parameter: double opacity
            Color color = (Color)value;
            double opacity = parameter == null ? 1 : double.Parse((string)parameter);

            byte ob = (byte)(opacity * 255);
            return Color.FromArgb(ob, color.R, color.G, color.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
