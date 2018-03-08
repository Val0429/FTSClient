using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Library.Converters {
    public class ImageConverter : MarkupExtension, IValueConverter {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            var bi = new BitmapImage(new Uri(value.ToString())) {
                CacheOption = BitmapCacheOption.OnLoad
            };
            return bi;

            //string uri = value.ToString();
            //var task = Task.Run(() => {
            //    var bi = new BitmapImage(new Uri(uri)) {
            //        CacheOption = BitmapCacheOption.OnLoad
            //    };
            //    return bi;
            //});
            //return new TaskCompletionNotifier<BitmapImage>(task);
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    //public sealed class TaskCompletionNotifier<TResult> : INotifyPropertyChanged {
    //    public TaskCompletionNotifier(Task<TResult> task) {
    //        Task = task;
    //        if (task.IsCompleted) return;
    //        task.ContinueWith(t => {
    //            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
    //                Final = task.Result;
    //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Result"));
    //            }));
    //        });
    //    }

    //    TResult Final { get; set; }

    //    // Gets the task being watched. This property never changes and is never <c>null</c>.
    //    public Task<TResult> Task { get; private set; }

    //    // Gets the result of the task. Returns the default value of TResult if the task has not completed successfully.
    //    public TResult Result { get { return Final == null ? default(TResult) : Final; } }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //}
}
