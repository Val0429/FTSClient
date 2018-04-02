using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Tencent.DataSources {
    public class FaceItem {
        public string sourceid { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public double facewidth { get; set; }
        public double faceheight { get; set; }
        public double quality { get; set; }
        public long createtime { get; set; }
        public string groupname { get; set; }
    }

    public class SearchParam {
        public string searchid { get; set; }
        public string name { get; set; }
        public long starttime { get; set; }
        public long endtime { get; set; }
        public string image { get; set; }
        public double score { get; set; }
    }

    public class SearchInfo {
        public string searchid { get; set; }
        public string status { get; set; }
    }

    public class SearchItem {
        public string searchid { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string sourceid { get; set; }
        public string image { get; set; }
        public long createtime { get; set; }
        public double score { get; set; }
    }

    public class PeopleGroup {
        [XmlAttribute("name")]
        public string name { get; set; }
        [XmlAttribute("color")]
        public string color { get; set; }
        [XmlAttribute("glowcolor")]
        public string glowcolor { get; set; }
    }

    public class Camera : DependencyObject {
        [XmlAttribute("name")]
        public string name { get; set; }
        [XmlAttribute("sourceid")]
        public string sourceid { get; set; }
        [XmlAttribute("type")]
        public int type { get; set; }
        [XmlAttribute("X")]
        public double X { get; set; }
        [XmlAttribute("Y")]
        public double Y { get; set; }
        [XmlAttribute("Angle")]
        public double Angle { get; set; }

        public SearchItem Face {
            get { return (SearchItem)GetValue(FaceProperty); }
            set { SetValue(FaceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Face.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FaceProperty =
            DependencyProperty.Register("Face", typeof(SearchItem), typeof(Camera), new PropertyMetadata(null));
    }

    public class TraceItem : DependencyObject {
        public TraceItem() {
            Faces = new ObservableCollection<SearchItem>();
        }


        public Camera Camera { get; set; }

        public long starttime { get; set; }
        public long endtime { get; set; }
        public ObservableCollection<SearchItem> Faces { get; private set; }
    }

    public class FaceDetail : DependencyObject {
        public FaceDetail() {
            PossibleContacts = new ObservableCollection<SearchItem>();
            Traces = new ObservableCollection<TraceItem>();
        }
        #region "Dependency Properties"
        public FaceItem CurrentFace {
            get { return (FaceItem)GetValue(CurrentFaceProperty); }
            set { SetValue(CurrentFaceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for currentFace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentFaceProperty =
            DependencyProperty.Register("CurrentFace", typeof(FaceItem), typeof(FaceDetail), new PropertyMetadata(null));
        public delegate void CurrentFaceChanged(FaceItem face);
        public event CurrentFaceChanged OnCurrentFaceChanged;
        public void DoCurrentFaceChange(FaceItem face) {
            this.OnCurrentFaceChanged?.Invoke(face);
        }

        public long EntryTime {
            get { return (long)GetValue(EntryTimeProperty); }
            set { SetValue(EntryTimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for EntryTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EntryTimeProperty =
            DependencyProperty.Register("EntryTime", typeof(long), typeof(FaceDetail), new PropertyMetadata(null));

        public long LastTime {
            get { return (long)GetValue(LastTimeProperty); }
            set { SetValue(LastTimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for LastTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastTimeProperty =
            DependencyProperty.Register("LastTime", typeof(long), typeof(FaceDetail), new PropertyMetadata(null));

        #endregion "Dependency Properties"

        public ObservableCollection<TraceItem> Traces { get; private set; }

        public ObservableCollection<SearchItem> PossibleContacts { get; private set; }
    }
}
