using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Library.Converters {
    public class MinutesToReadableConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            long o = System.Convert.ToInt64(value);

            long days = o / (60 * 24);
            long hours = (o - days * 60 * 24) / 60;
            long minutes = o % 60;

            List<string> final = new List<string>();
            if (days > 0) final.Add(days + "d");
            if (hours > 0) final.Add(hours + "h");
            if (minutes > 0) final.Add(minutes + "m");

            return string.Join(" ", final.ToArray());

            //Color c = (Color)value;
            //var l = 0.2126 * c.ScR + 0.7152 * c.ScG + 0.0722 * c.ScB;
            //return l < 0.5 ? ColorConverter.ConvertFromString("White") : ColorConverter.ConvertFromString("Black");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
