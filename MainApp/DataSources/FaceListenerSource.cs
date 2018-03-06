using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tencent.DataSources {
    public class FaceItem {
        public string image { get; set; }
        public long createtime { get; set; }
    }

    public class FaceListenerSource {
        public FaceListenerSource() {
            Faces = new ObservableCollection<FaceItem>() {
                new FaceItem() { image = "123", createtime = 456 }
            };
        }

        public ObservableCollection<FaceItem> Faces { get; private set; }
    }
}
