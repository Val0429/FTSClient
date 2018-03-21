using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

            Traces = new ObservableCollection<Track>();
        }

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


        public ObservableCollection<Track> Traces {
            get { return (ObservableCollection<Track>)GetValue(TracesProperty); }
            set { SetValue(TracesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Traces.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TracesProperty =
            DependencyProperty.Register("Traces", typeof(ObservableCollection<Track>), typeof(TimeTrack), new PropertyMetadata(null));



        #endregion "Dependency Properties"
    }
}
