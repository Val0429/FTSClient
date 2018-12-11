using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Library.Helpers {
    public class ImageAsyncHelper : DependencyObject {
        public static Uri GetSourceUri(DependencyObject obj) { return (Uri)obj.GetValue(SourceUriProperty); }
        public static void SetSourceUri(DependencyObject obj, Uri value) {
            obj.SetValue(SourceUriProperty, value);
        }
        public static readonly DependencyProperty SourceUriProperty = DependencyProperty.RegisterAttached("SourceUri", typeof(Uri), typeof(ImageAsyncHelper), new FrameworkPropertyMetadata(
            (obj, e) => {
                if (obj.GetType() == typeof(Image)) {
                    ((Image)obj).SetBinding(Image.SourceProperty,
                      new Binding("VerifiedUri") {
                          Source = new ImageAsyncHelper { GivenUri = (Uri)e.NewValue },
                          IsAsync = true,
                      });
                }

            }
        ));

        //public static readonly DependencyProperty SourceUriProperty = DependencyProperty.RegisterAttached("SourceUri", typeof(Uri), typeof(ImageAsyncHelper), new PropertyMetadata {
        //    PropertyChangedCallback = (obj, e) =>
        //    {
        //        if (obj.GetType() == typeof(Image)) {
        //            ((Image)obj).SetBinding(Image.SourceProperty,
        //              new Binding("VerifiedUri") {
        //                  Source = new ImageAsyncHelper { GivenUri = (Uri)e.NewValue },
        //                  IsAsync = true,
        //              });
        //        }

        //    }
        //});

        Uri GivenUri;
        public Uri VerifiedUri {
            get {
                try {
                    Dns.GetHostEntry(GivenUri.DnsSafeHost);
                    return GivenUri;
                }
                catch (Exception) {
                    return null;
                }

            }
        }
    }
}
