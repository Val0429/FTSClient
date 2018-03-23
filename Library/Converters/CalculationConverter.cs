﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Data;

namespace Library.Converters {
    public class CalculationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string param = (string)parameter;
            double val = double.Parse(value.ToString());
            using (DataTable dt = new DataTable()) {
                return dt.Compute(string.Format(param, val), null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
