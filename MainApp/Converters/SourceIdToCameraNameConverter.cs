using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Tencent.DataSources;

namespace Tencent.Converters {
    public class SourceIdToCameraNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var cameras = (Application.Current.Resources["FaceListenerSource"] as FaceListenerSource).Cameras;
            Camera camera = null;
            cameras.TryGetValue((string)value, out camera);
            return camera != null ? camera.name : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
