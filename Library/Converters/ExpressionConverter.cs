using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Data;

namespace Library.Converters {
    public class ExpressionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string param = (string)parameter;
            double val = double.Parse(value.ToString());

            var engine = new Jurassic.ScriptEngine();
            var result = engine.Evaluate(string.Format(param, val));
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
