using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tencent.Components.FaceTracingVideoMonitors {
    public class NonClipGrid : Grid {
        protected override Geometry GetLayoutClip(Size layoutSlotSize) {
            return null;
        }
    }
}
