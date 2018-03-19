using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Tencent.DataSources;

namespace Tencent.Converters {
    public class GroupnameToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return DependencyProperty.UnsetValue;
            var peoplegroups = (Application.Current.Resources["FaceListenerSource"] as FaceListenerSource).PeopleGroups;
            string param = (string)parameter;
            PeopleGroup pg = null;
            peoplegroups.TryGetValue((string)value, out pg);
            return pg != null ? ColorConverter.ConvertFromString(
                param == "glow" ? pg.glowcolor : pg.color
                ) : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
