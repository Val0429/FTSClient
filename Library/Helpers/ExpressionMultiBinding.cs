using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Library.Helpers {
    public class ExpressionMultiBindingConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            string format = values[0].ToString();
            var template = string.Format(format, values
                .Where((v, i) => i != 0)
                .Select((v, i) => v == null ? "null" : "\""+v.ToString()+"\"")
                .ToArray()
                );

            var engine = new Jurassic.ScriptEngine();
            var result = engine.Evaluate(template);

            return TypeDescriptor.GetConverter(targetType).ConvertFrom(result.ToString());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionMultiBinding : System.Windows.Data.MultiBinding {
        public ExpressionMultiBinding(BindingBase format, BindingBase b1, BindingBase b2, BindingBase b3, BindingBase b4, BindingBase b5, BindingBase b6, BindingBase b7, BindingBase b8) {
            Bindings.Add(format);
            if (b1 != null) Bindings.Add(b1);
            if (b2 != null) Bindings.Add(b2);
            if (b3 != null) Bindings.Add(b3);
            if (b4 != null) Bindings.Add(b4);
            if (b5 != null) Bindings.Add(b5);
            if (b6 != null) Bindings.Add(b6);
            if (b7 != null) Bindings.Add(b7);
            if (b8 != null) Bindings.Add(b8);

            Converter = new ExpressionMultiBindingConverter();
        }
        public ExpressionMultiBinding(BindingBase format, BindingBase b1, BindingBase b2, BindingBase b3, BindingBase b4, BindingBase b5, BindingBase b6, BindingBase b7) : this(format, b1, b2, b3, b4, b5, b6, b7, null) {}
        public ExpressionMultiBinding(BindingBase format, BindingBase b1, BindingBase b2, BindingBase b3, BindingBase b4, BindingBase b5, BindingBase b6) : this(format, b1, b2, b3, b4, b5, b6, null) {}
        public ExpressionMultiBinding(BindingBase format, BindingBase b1, BindingBase b2, BindingBase b3, BindingBase b4, BindingBase b5) : this(format, b1, b2, b3, b4, b5, null) {}
        public ExpressionMultiBinding(BindingBase format, BindingBase b1, BindingBase b2, BindingBase b3, BindingBase b4) : this(format, b1, b2, b3, b4, null) {}
        public ExpressionMultiBinding(BindingBase format, BindingBase b1, BindingBase b2, BindingBase b3) : this(format, b1, b2, b3, null) {}
        public ExpressionMultiBinding(BindingBase format, BindingBase b1, BindingBase b2) : this(format, b1, b2, null) {}
        public ExpressionMultiBinding(BindingBase format, BindingBase b1) : this(format, b1, null) {}
    }
}
