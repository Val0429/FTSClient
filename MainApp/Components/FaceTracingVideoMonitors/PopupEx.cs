using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Tencent.Components.FaceTracingVideoMonitors {
    public class PopupEx : Popup {
        protected override void OnOpened(EventArgs e) {
            var friend = this.PlacementTarget;
            friend.QueryCursor += friend_QueryCursor;

            base.OnOpened(e);
        }

        protected override void OnClosed(EventArgs e) {
            var friend = this.PlacementTarget;
            friend.QueryCursor -= friend_QueryCursor;

            base.OnClosed(e);
        }

        private void friend_QueryCursor(object sender, System.Windows.Input.QueryCursorEventArgs e) {
            this.HorizontalOffset += +0.1;
            this.HorizontalOffset += -0.1;
        }
    }
}
