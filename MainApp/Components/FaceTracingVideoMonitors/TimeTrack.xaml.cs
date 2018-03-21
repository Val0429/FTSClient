using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Tencent.Components.FaceTracingVideoMonitor;

namespace Tencent.Components.FaceTracingVideoMonitors {
    /// <summary>
    /// Interaction logic for TimeTrack.xaml
    /// </summary>
    public partial class TimeTrack : UserControl {
        public TimeTrack() {
            InitializeComponent();

            Traces = new ObservableCollection<FaceTracingVideoMonitor.Track>();

            /// initial slider
            this.Slider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler((object o, DragStartedEventArgs e) => IsDragging = true));
            this.Slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((object o, DragCompletedEventArgs e) => IsDragging = false));
            this.Slider.PreviewMouseUp += (object sender, MouseButtonEventArgs e) => {
                this.OnDragSetCurrentTime?.Invoke(this.CurrentTime);
            };
        }

        public delegate void DragSetCurrentTime(long timestamp);
        public event DragSetCurrentTime OnDragSetCurrentTime;

        #region "Dependency Properties"

        public long BeginTime {
            get { return (long)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BeginTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(long), typeof(TimeTrack), new PropertyMetadata(0L));



        public long EndTime {
            get { return (long)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(long), typeof(TimeTrack), new PropertyMetadata(0L));

        //public ObservableCollection<Track> Traces { get; private set; }


        public ObservableCollection<FaceTracingVideoMonitor.Track> Traces {
            get { return (ObservableCollection<FaceTracingVideoMonitor.Track>)GetValue(TracesProperty); }
            set { SetValue(TracesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Traces.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TracesProperty =
            DependencyProperty.Register("Traces", typeof(ObservableCollection<FaceTracingVideoMonitor.Track>), typeof(TimeTrack), new PropertyMetadata(null));



        public long CurrentTime {
            get { return (long)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(long), typeof(TimeTrack), new PropertyMetadata(0L));



        public bool IsDragging {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDragging.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof(bool), typeof(TimeTrack), new PropertyMetadata(false));



        #endregion "Dependency Properties"
    }
}
