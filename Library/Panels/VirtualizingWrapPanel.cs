using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;

namespace Library.Panels {
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo {

        Size _extent = new Size(0, 0);
        Size _viewport = new Size(0, 0);
        Point _offset = new Point(0, 0);
        Size _unitSize = Size.Empty;

        ItemsControl _itemsControl = null;
        private TranslateTransform _trans = new TranslateTransform();

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
            _itemsControl = ItemsControl.GetItemsOwner(this);
            this.RenderTransform = _trans;
        }

        #region "IScrollInfo"

        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight { get { return _extent.Height; } }
        public double ExtentWidth { get { return _extent.Width; } }

        public double ViewportHeight { get { return _viewport.Height; } }

        public double ViewportWidth { get { return _viewport.Width; } }

        public double HorizontalOffset { get { return _offset.X; } }
        public double VerticalOffset { get { return _offset.Y; } }

        public ScrollViewer ScrollOwner { get; set; }

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

            InvalidateMeasure();
        }

        public void LineDown() {
            //Console.WriteLine("Set offset: {0}", this.VerticalOffset + _unitSize.Height);
            this.SetVerticalOffset(this.VerticalOffset + _unitSize.Height);
        }
        public void LineUp() {
            //Console.WriteLine("Set offset: {0}", this.VerticalOffset - _unitSize.Height);
            this.SetVerticalOffset(this.VerticalOffset - _unitSize.Height);
        }

        public void LineLeft() {}
        public void LineRight() {}

        public void MouseWheelDown() { this.LineDown(); }
        public void MouseWheelUp() { this.LineUp(); }

        public void MouseWheelLeft() { this.LineLeft(); }
        public void MouseWheelRight() { this.LineRight(); }

        public void PageDown() { this.LineDown(); }
        public void PageUp() { this.LineUp(); }

        public void PageLeft() { this.LineLeft(); }
        public void PageRight() { this.LineRight(); }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            return rectangle;
        }

        #endregion "IScrollInfo"

        #region "Helper Private Function"
        private int _mracLastItemsCount = 0;
        private Size _mracAvailableSize = Size.Empty;
        private Tuple<long, long> _mracRowsAndCols = new Tuple<long, long>(0, 1);
        /// Measure how many rows & cols available in current space
        private Tuple<long, long> MeasureRowsAndCols(Size availableSize = default(Size)) {
            int totalItemsCount = _itemsControl.Items.Count;
            if (availableSize == default(Size)) availableSize = _mracAvailableSize;
            if (availableSize == _mracAvailableSize && totalItemsCount == _mracLastItemsCount) return _mracRowsAndCols;
            _mracLastItemsCount = totalItemsCount;
            _mracAvailableSize = availableSize;

            long rows = 0, columns = 1;
            do {
                if (totalItemsCount == 0) break;
                columns = (long)Math.Max(1, Math.Floor(availableSize.Width / _unitSize.Width));
                rows = (long)Math.Ceiling((double)totalItemsCount / columns);

            } while (false);

            _mracRowsAndCols = new Tuple<long, long>(rows, columns);
            return _mracRowsAndCols;
        }

        /// get Rect from index number
        private Rect GetRectFromItemIndex(int index) {
            var RowsAndCols = MeasureRowsAndCols();
            var rows = RowsAndCols.Item1;
            var cols = RowsAndCols.Item2;
            Point pt = new Point(0, 0);
            pt.X = index % cols * _unitSize.Width;
            pt.Y = Math.Floor((double)index / cols) * _unitSize.Height;

            return new Rect(
                pt,
                _unitSize
                );
        }

        private Size _ceAvailableSize = Size.Empty;
        private int _ceTotalItemsCount = 0;
        /// Resize
        private void CalculateExtents(Size availableSize) {
            int totalItemsCount = _itemsControl.Items.Count;
            if (_ceAvailableSize == availableSize && _ceTotalItemsCount == totalItemsCount) return;
            _ceAvailableSize = availableSize; _ceTotalItemsCount = totalItemsCount;

            var RowsAndCols = MeasureRowsAndCols(availableSize);

            var extent = new Size(
                availableSize.Width,
                _unitSize.Height * RowsAndCols.Item1
                );

            var viewport = availableSize;

            /// re-position after resize
            SetVerticalOffset(VerticalOffset);

            _extent = extent;
            _viewport = viewport;
            ScrollOwner?.InvalidateScrollInfo();
        }

        private int _cuiStartIndex = 0;
        private int _cuiEndIndex = 0;
        private int ccount = 0;
        /// Remove item instance out of range
        private void CleanUpItems(int startIndex, int endIndex) {
            if (_cuiStartIndex == startIndex && _cuiEndIndex == endIndex) return;
            _cuiStartIndex = startIndex; _cuiEndIndex = endIndex;
            UIElementCollection children = this.InternalChildren;
            if (children.Count == 0) return;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            int? sfinal = null;
            GeneratorPosition? spos = null;
            int? efinal = null;
            for (int i = children.Count - 1; i >= 0; --i) {
                GeneratorPosition pos = new GeneratorPosition(i, 0);
                var itemIndex = generator.IndexFromGeneratorPosition(pos);
                if (itemIndex < startIndex || itemIndex > endIndex) {
                    if (itemIndex == -1) {
                        continue;
                    }
                    generator.Remove(pos, 1);
                    RemoveInternalChildRange(i, 1);
                    //if (efinal == null) sfinal = efinal = i;
                    //else sfinal = i;
                    //spos = pos;
                } else {
                    if (efinal != null) break;
                }
            }
            //if (spos != null && efinal != null) {
            //    var range = (int)efinal - (int)sfinal + 1;
            //    ccount += range;
            //    generator.Remove((GeneratorPosition)spos, range);
            //    RemoveInternalChildRange((int)sfinal, range);
            //}
        }

        protected override void BringIndexIntoView(int index) {
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;
            var genContainer = generator.GetItemContainerGeneratorForPanel(this);
            FrameworkElement element = (FrameworkElement)genContainer.ContainerFromIndex(index);
            if (element != null) element.BringIntoView();
            else {
                var rect = GetRectFromItemIndex(index);
                SetVerticalOffset(rect.Y);
            }
        }
        #endregion "Helper Private Function"

        #region "Measure & Arrange"
        private int count = 0;
        protected override Size MeasureOverride(Size availableSize) {
            /// todo, extent height
            if (availableSize.Height == double.PositiveInfinity) availableSize.Height = 0;

            /// Initial Variables
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = base.ItemContainerGenerator;
            ItemsControl itemsControl = _itemsControl;
            int totalItemsCount = itemsControl.Items.Count;
            if (totalItemsCount == 0) return availableSize;

            /// Auto Detect Size!
            if (_unitSize == Size.Empty) {
                using (generator.StartAt(generator.GeneratorPositionFromIndex(0), GeneratorDirection.Forward, true)) {
                    bool newlyRealized;
                    UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                    base.AddInternalChild(child);
                    generator.PrepareItemContainer(child);
                    child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    _unitSize = child.DesiredSize;
                    generator.Remove(generator.GeneratorPositionFromIndex(0), 1);
                    this.RemoveInternalChildRange(0, 1);
                }
            }

            /// After Auto Size Guarantee, Calculate extent / viewport
            CalculateExtents(availableSize);

            /// Calculate start / end position
            var row = Math.Floor(VerticalOffset / _unitSize.Height);
            var RowsAndCols = MeasureRowsAndCols(availableSize);
            //var startIndex = (int)(RowsAndCols.Item2 * row + RowsAndCols.Item2);
            //var endIndex = (int)(Math.Ceiling(ViewportHeight / _unitSize.Height) * RowsAndCols.Item2 + startIndex - 1 - RowsAndCols.Item2 * 2);
            //var startIndex = (int)(RowsAndCols.Item2 * row);
            //var endIndex = (int)(Math.Ceiling(ViewportHeight / _unitSize.Height) * RowsAndCols.Item2 + startIndex - 1);

            var startIndex = (int)(RowsAndCols.Item2 * row - RowsAndCols.Item2);
            var endIndex = (int)(Math.Ceiling(ViewportHeight / _unitSize.Height) * RowsAndCols.Item2 + startIndex - 1 + RowsAndCols.Item2);
            if (startIndex < 0) startIndex = 0;

            /// Clean Up Items
            CleanUpItems(startIndex, endIndex);
            var startPos = generator.GeneratorPositionFromIndex(startIndex);

            /// Draw
            using (generator.StartAt( startPos, GeneratorDirection.Forward, true )) {
                for (var i = startIndex; i <= endIndex; ++i) {
                    bool newlyRealized;
                    UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                    count++;

                    /// Insert if new
                    if (newlyRealized) {
                        int childIndex = ((startPos.Offset == 0) ? startPos.Index : startPos.Index + 1) + i - startIndex;
                        if (childIndex >= children.Count) base.AddInternalChild(child);
                        else base.InsertInternalChild(childIndex, child);
                        generator.PrepareItemContainer(child);
                    }
                    /// If null, out of bound
                    if (child != null) child.Measure(_unitSize);
                }
            }

            ///// Clean Up Items
            //this.Dispatcher.BeginInvoke(
            //    new Action(() => CleanUpItems(startIndex, endIndex))
            //    );

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            IItemContainerGenerator generator = base.ItemContainerGenerator;
            for (int i = 0; i < this.InternalChildren.Count; ++i) {
                var itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                this.InternalChildren[i].Arrange(GetRectFromItemIndex(itemIndex));
            }

            return finalSize;
        }
        #endregion "Measure & Arrange"
    }
}
