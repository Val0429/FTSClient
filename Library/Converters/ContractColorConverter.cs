using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Library.Converters {
    public class ContractColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Color c = (Color)value;
            var l = 0.2126 * c.ScR + 0.7152 * c.ScG + 0.0722 * c.ScB;
            return l < 0.5 ? ColorConverter.ConvertFromString("White") : ColorConverter.ConvertFromString("Black");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
