using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Library.Panels {
    public class AnnoyingPanel : Panel, IScrollInfo {
        private TranslateTransform _trans = new TranslateTransform();
        public AnnoyingPanel() {
            this.RenderTransform = _trans;
        }

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        private Size _extent = new Size(0, 0);
        public double ExtentHeight {
            get {
                return _extent.Height;
            }
        }

        public double ExtentWidth {
            get {
                return _extent.Width;
            }
        }

        private Point _offset;
        public double HorizontalOffset {
            get { return _offset.X; }
        }
        public double VerticalOffset {
            get { return _offset.Y; }
        }
        public void SetHorizontalOffset(double offset) {
            _offset.X = offset;
        }

        public void SetVerticalOffset(double offset) {
            if (offset < 0 || _viewport.Height >= _extent.Height) {
                offset = 0;
            } else {
                if (offset + _viewport.Height >= _extent.Height) {
                    offset = _extent.Height - _viewport.Height;
                }
            }
            _offset.Y = offset;
            ScrollOwner?.InvalidateScrollInfo();
            _trans.Y = -offset;
        }

        public ScrollViewer ScrollOwner { get; set; }

        private Size _viewport = new Size(0, 0);
        public double ViewportHeight {
            get {
                return _viewport.Height;
            }
        }

        public double ViewportWidth {
            get {
                return _viewport.Width;
            }
        }

        public void LineDown() {
            SetVerticalOffset(this.VerticalOffset + 1);
        }

        public void LineLeft() {
            return;
        }

        public void LineRight() {
            return;
        }

        public void LineUp() {
            SetVerticalOffset(this.VerticalOffset - 1);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            throw new NotImplementedException();
        }

        public void MouseWheelDown() {
            throw new NotImplementedException();
        }

        public void MouseWheelLeft() {
            throw new NotImplementedException();
        }

        public void MouseWheelRight() {
            throw new NotImplementedException();
        }

        public void MouseWheelUp() {
            throw new NotImplementedException();
        }

        public void PageDown() {
            throw new NotImplementedException();
        }

        public void PageLeft() {
            throw new NotImplementedException();
        }

        public void PageRight() {
            throw new NotImplementedException();
        }

        public void PageUp() {
            throw new NotImplementedException();
        }

        protected override Size MeasureOverride(Size availableSize) {
            Size childSize = new Size(
                availableSize.Width,
                availableSize.Height * 2 / this.InternalChildren.Count
                );

            Size extent = new Size(
                availableSize.Width,
                childSize.Height * this.InternalChildren.Count
                );

            if (extent != _extent) {
                _extent = extent;
                ScrollOwner?.InvalidateScrollInfo();
            }

            if (availableSize != _viewport) {
                _viewport = availableSize;
                ScrollOwner?.InvalidateScrollInfo();
            }

            foreach (UIElement child in this.InternalChildren) {
                child.Measure(childSize);
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            Size childSize = new Size(
                finalSize.Width,
                (finalSize.Height * 2) / this.InternalChildren.Count
                );

            Size extent = new Size(
                finalSize.Width,
                childSize.Height * this.InternalChildren.Count
                );

            if (extent != _extent) {
                _extent = extent;
                ScrollOwner?.InvalidateScrollInfo();
            }

            if (finalSize != _viewport) {
                _viewport = finalSize;
                ScrollOwner?.InvalidateScrollInfo();
            }

            for (int i = 0; i < this.InternalChildren.Count; ++i) {
                this.InternalChildren[i].Arrange(new Rect(0, childSize.Height * i, childSize.Width, childSize.Height));
            }

            return finalSize;
        }
    }
}
