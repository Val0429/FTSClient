using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Tencent.Panels {
    public class TimeTrackPanel : Panel {

        #region "Dependency Properties"

        public long TimeStart {
            get { return (long)GetValue(TimeStartProperty); }
            set { SetValue(TimeStartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeStartProperty =
            DependencyProperty.Register("TimeStart", typeof(long), typeof(TimeTrackPanel),
                new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public long TimeEnd {
            get { return (long)GetValue(TimeEndProperty); }
            set { SetValue(TimeEndProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TimeEnd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeEndProperty =
            DependencyProperty.Register("TimeEnd", typeof(long), typeof(TimeTrackPanel),
                new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static long GetTrackTimeStart(DependencyObject obj) {
            return (long)obj.GetValue(TrackTimeStartProperty);
        }
        public static void SetTrackTimeStart(DependencyObject obj, long value) {
            obj.SetValue(TrackTimeStartProperty, value);
        }
        public static readonly DependencyProperty TrackTimeStartProperty =
            DependencyProperty.RegisterAttached("TrackTimeStart", typeof(long), typeof(TimeTrackPanel),
                new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange
                    ));

        public static long GetTrackTimeEnd(DependencyObject obj) {
            return (long)obj.GetValue(TrackTimeEndProperty);
        }
        public static void SetTrackTimeEnd(DependencyObject obj, long value) {
            obj.SetValue(TrackTimeEndProperty, value);
        }
        public static readonly DependencyProperty TrackTimeEndProperty =
            DependencyProperty.RegisterAttached("TrackTimeEnd", typeof(long), typeof(TimeTrackPanel),
                new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange
                    ));

        #endregion "Dependency Properties"

        protected override Size MeasureOverride(Size availableSize) {
            var ts = TimeStart;
            var te = TimeEnd;

            foreach (UIElement element in base.InternalChildren) {
                //double width = 0;
                //long tts = (long)element.GetValue(TrackTimeStartProperty);
                //long tte = (long)element.GetValue(TrackTimeEndProperty);
                //if (tts < te && tte > ts) {
                //    /// overlaps, therefore on track
                //    width = availableSize.Width * (tte - tts) / (te - ts);
                //}
                //Size size = new Size(
                //    width,
                //    availableSize.Height
                //    );
                //element.Measure(size);
                element.Measure(availableSize);
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            var ts = TimeStart;
            var te = TimeEnd;

            foreach (UIElement element in base.InternalChildren) {
                double width = 0;
                long tts = (long)element.GetValue(TrackTimeStartProperty);
                long tte = (long)element.GetValue(TrackTimeEndProperty);
                if (tts < te && tte > ts) {
                    /// overlaps, therefore on track
                    width = finalSize.Width * (tte - tts) / (te - ts);
                }

                element.Arrange(
                    new Rect(finalSize.Width * (tts-ts) / (te-ts), 0, width, finalSize.Height)
                    );
            }
            return finalSize;
        }
    }
}
