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
            Console.WriteLine("Set offset: {0}", this.VerticalOffset + _unitSize.Height);
            this.SetVerticalOffset(this.VerticalOffset + _unitSize.Height);
        }
        public void LineUp() {
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
            /// todo
            return rectangle;
        }

        #endregion "IScrollInfo"

        #region "Helper Private Function"
        private int _mracLastItemsCount = 0;
        private Size _mracAvailableSize = Size.Empty;
        private Tuple<long, long> _mracRowsAndCols = new Tuple<long, long>(0, 1);
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
        }

        private void CleanUpItems(int startIndex, int endIndex) {
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            for (int i = children.Count -1; i >=0; --i) {
                GeneratorPosition pos = new GeneratorPosition(i, 0);
                var itemIndex = generator.IndexFromGeneratorPosition(pos);
                if (itemIndex < startIndex || itemIndex > endIndex) {
                    generator.Remove(pos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }
        #endregion "Helper Private Function"

        #region "Measure & Arrange"
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
            var startIndex = (int)(RowsAndCols.Item2 * row);
            var endIndex = (int)(Math.Ceiling(ViewportHeight / _unitSize.Height) * RowsAndCols.Item2 + startIndex - 1);

            /// Draw
            using (generator.StartAt( generator.GeneratorPositionFromIndex(startIndex), GeneratorDirection.Forward, true )) {
                for (var i = startIndex; i <= endIndex; ++i) {
                    bool newlyRealized;
                    UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                    if (newlyRealized) {
                        base.AddInternalChild(child);
                        generator.PrepareItemContainer(child);
                    }
                    /// If null, out of bound
                    if (child != null) child.Measure(_unitSize);
                }
            }

            /// Clean Up Items
            CleanUpItems(startIndex, endIndex);
            
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

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows;
//using System.Windows.Media;
//using System.Diagnostics;
//using System.ComponentModel;
//using System.Windows.Input;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;

//namespace Library.Panels {
//    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo {

//        #region Fields

//        UIElementCollection _children;
//        ItemsControl _itemsControl;
//        IItemContainerGenerator _generator;
//        private Point _offset = new Point(0, 0);
//        private Size _extent = new Size(0, 0);
//        private Size _viewport = new Size(0, 0);
//        private int firstIndex = 0;
//        private Size childSize;
//        private Size _pixelMeasuredViewport = new Size(0, 0);
//        Dictionary<UIElement, Rect> _realizedChildLayout = new Dictionary<UIElement, Rect>();
//        WrapPanelAbstraction _abstractPanel;


//        #endregion

//        #region Properties

//        private Size ChildSlotSize {
//            get {
//                return new Size(this.ItemWidth, this.ItemHeight);
//            }
//        }

//        #endregion

//        #region Dependency Properties

//        [TypeConverter(typeof(LengthConverter))]
//        public double ItemHeight {
//            get {
//                return (double)base.GetValue(ItemHeightProperty);
//            }
//            set {
//                base.SetValue(ItemHeightProperty, value);
//            }
//        }

//        [TypeConverter(typeof(LengthConverter))]
//        public double ItemWidth {
//            get {
//                return (double)base.GetValue(ItemWidthProperty);
//            }
//            set {
//                base.SetValue(ItemWidthProperty, value);
//            }
//        }

//        public Orientation Orientation {
//            get { return (Orientation)this.GetValue(OrientationProperty); }
//            set { this.SetValue(OrientationProperty, value); }
//        }

//        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.PositiveInfinity));
//        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.PositiveInfinity));
//        public static readonly DependencyProperty OrientationProperty = StackPanel.OrientationProperty.AddOwner(typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal));

//        #endregion

//        #region Methods

//        private void SetFirstRowViewItemIndex(int index) {
//            this.SetVerticalOffset((index) / Math.Floor((this._viewport.Width) / this.childSize.Width));
//            this.SetHorizontalOffset((index) / Math.Floor((this._viewport.Height) / this.childSize.Height));
//        }

//        public void Resizing(object sender, EventArgs e) {
//            if (this._viewport.Width != 0) {
//                int firstIndexCache = this.firstIndex;
//                this._abstractPanel = null;
//                this.MeasureOverride(this._viewport);
//                this.SetFirstRowViewItemIndex(this.firstIndex);
//                this.firstIndex = firstIndexCache;
//            }
//        }

//        public int GetFirstVisibleSection() {
//            int section;
//            var maxSection = 0;
//            if (this._abstractPanel != null) {
//                maxSection = this._abstractPanel.Max(x => x.Section);
//            }
//            if (this.Orientation == Orientation.Horizontal) {
//                section = (int)this._offset.Y;
//            } else {
//                section = (int)this._offset.X;
//            }
//            if (section > maxSection)
//                section = maxSection;
//            return section;
//        }

//        public int GetFirstVisibleIndex() {
//            int section = this.GetFirstVisibleSection();

//            if (this._abstractPanel != null) {
//                var item = this._abstractPanel.Where(x => x.Section == section).FirstOrDefault();
//                if (item != null)
//                    return item._index;
//            }
//            return 0;
//        }

//        public void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated) {
//            for (int i = this._children.Count - 1; i >= 0; i--) {
//                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
//                int itemIndex = this._generator.IndexFromGeneratorPosition(childGeneratorPos);
//                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated) {
//                    this._generator.Remove(childGeneratorPos, 1);
//                    this.RemoveInternalChildRange(i, 1);
//                }
//            }
//        }

//        private void ComputeExtentAndViewport(Size pixelMeasuredViewportSize, int visibleSections) {
//            if (this.Orientation == Orientation.Horizontal) {
//                this._viewport.Height = visibleSections;
//                this._viewport.Width = pixelMeasuredViewportSize.Width;
//            } else {
//                this._viewport.Width = visibleSections;
//                this._viewport.Height = pixelMeasuredViewportSize.Height;
//            }

//            if (this.Orientation == Orientation.Horizontal) {
//                this._extent.Height = this._abstractPanel.SectionCount + this.ViewportHeight - 1;

//            } else {
//                this._extent.Width = this._abstractPanel.SectionCount + this.ViewportWidth - 1;
//            }
//            this._owner?.InvalidateScrollInfo();
//        }

//        private void ResetScrollInfo() {
//            this._offset.X = 0;
//            this._offset.Y = 0;
//        }

//        private int GetNextSectionClosestIndex(int itemIndex) {
//            var abstractItem = this._abstractPanel[itemIndex];
//            if (abstractItem.Section < this._abstractPanel.SectionCount - 1) {
//                var ret = this._abstractPanel.
//                    Where(x => x.Section == abstractItem.Section + 1).
//                    OrderBy(x => Math.Abs(x.SectionIndex - abstractItem.SectionIndex)).
//                    First();
//                return ret._index;
//            } else
//                return itemIndex;
//        }

//        private int GetLastSectionClosestIndex(int itemIndex) {
//            var abstractItem = this._abstractPanel[itemIndex];
//            if (abstractItem.Section > 0) {
//                var ret = this._abstractPanel.
//                    Where(x => x.Section == abstractItem.Section - 1).
//                    OrderBy(x => Math.Abs(x.SectionIndex - abstractItem.SectionIndex)).
//                    First();
//                return ret._index;
//            } else
//                return itemIndex;
//        }

//        private void NavigateDown() {
//            var gen = this._generator.GetItemContainerGeneratorForPanel(this);
//            UIElement selected = (UIElement)Keyboard.FocusedElement;
//            int itemIndex = gen.IndexFromContainer(selected);
//            int depth = 0;
//            while (itemIndex == -1) {
//                selected = (UIElement)VisualTreeHelper.GetParent(selected);
//                itemIndex = gen.IndexFromContainer(selected);
//                depth++;
//            }
//            DependencyObject next = null;
//            if (this.Orientation == Orientation.Horizontal) {
//                int nextIndex = this.GetNextSectionClosestIndex(itemIndex);
//                next = gen.ContainerFromIndex(nextIndex);
//                while (next == null) {
//                    this.SetVerticalOffset(this.VerticalOffset + 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(nextIndex);
//                }
//            } else {
//                if (itemIndex == this._abstractPanel._itemCount - 1)
//                    return;
//                next = gen.ContainerFromIndex(itemIndex + 1);
//                while (next == null) {
//                    this.SetHorizontalOffset(this.HorizontalOffset + 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(itemIndex + 1);
//                }
//            }
//            while (depth != 0) {
//                next = VisualTreeHelper.GetChild(next, 0);
//                depth--;
//            }
//            (next as UIElement).Focus();
//        }

//        private void NavigateLeft() {
//            var gen = this._generator.GetItemContainerGeneratorForPanel(this);

//            UIElement selected = (UIElement)Keyboard.FocusedElement;
//            int itemIndex = gen.IndexFromContainer(selected);
//            int depth = 0;
//            while (itemIndex == -1) {
//                selected = (UIElement)VisualTreeHelper.GetParent(selected);
//                itemIndex = gen.IndexFromContainer(selected);
//                depth++;
//            }
//            DependencyObject next = null;
//            if (this.Orientation == Orientation.Vertical) {
//                int nextIndex = this.GetLastSectionClosestIndex(itemIndex);
//                next = gen.ContainerFromIndex(nextIndex);
//                while (next == null) {
//                    this.SetHorizontalOffset(this.HorizontalOffset - 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(nextIndex);
//                }
//            } else {
//                if (itemIndex == 0)
//                    return;
//                next = gen.ContainerFromIndex(itemIndex - 1);
//                while (next == null) {
//                    this.SetVerticalOffset(this.VerticalOffset - 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(itemIndex - 1);
//                }
//            }
//            while (depth != 0) {
//                next = VisualTreeHelper.GetChild(next, 0);
//                depth--;
//            }
//            (next as UIElement).Focus();
//        }

//        private void NavigateRight() {
//            var gen = this._generator.GetItemContainerGeneratorForPanel(this);
//            UIElement selected = (UIElement)Keyboard.FocusedElement;
//            int itemIndex = gen.IndexFromContainer(selected);
//            int depth = 0;
//            while (itemIndex == -1) {
//                selected = (UIElement)VisualTreeHelper.GetParent(selected);
//                itemIndex = gen.IndexFromContainer(selected);
//                depth++;
//            }
//            DependencyObject next = null;
//            if (this.Orientation == Orientation.Vertical) {
//                int nextIndex = this.GetNextSectionClosestIndex(itemIndex);
//                next = gen.ContainerFromIndex(nextIndex);
//                while (next == null) {
//                    this.SetHorizontalOffset(this.HorizontalOffset + 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(nextIndex);
//                }
//            } else {
//                if (itemIndex == this._abstractPanel._itemCount - 1)
//                    return;
//                next = gen.ContainerFromIndex(itemIndex + 1);
//                while (next == null) {
//                    this.SetVerticalOffset(this.VerticalOffset + 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(itemIndex + 1);
//                }
//            }
//            while (depth != 0) {
//                next = VisualTreeHelper.GetChild(next, 0);
//                depth--;
//            }
//            (next as UIElement).Focus();
//        }

//        private void NavigateUp() {
//            var gen = this._generator.GetItemContainerGeneratorForPanel(this);
//            UIElement selected = (UIElement)Keyboard.FocusedElement;
//            int itemIndex = gen.IndexFromContainer(selected);
//            int depth = 0;
//            while (itemIndex == -1) {
//                selected = (UIElement)VisualTreeHelper.GetParent(selected);
//                itemIndex = gen.IndexFromContainer(selected);
//                depth++;
//            }
//            DependencyObject next = null;
//            if (this.Orientation == Orientation.Horizontal) {
//                int nextIndex = this.GetLastSectionClosestIndex(itemIndex);
//                next = gen.ContainerFromIndex(nextIndex);
//                while (next == null) {
//                    this.SetVerticalOffset(this.VerticalOffset - 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(nextIndex);
//                }
//            } else {
//                if (itemIndex == 0)
//                    return;
//                next = gen.ContainerFromIndex(itemIndex - 1);
//                while (next == null) {
//                    this.SetHorizontalOffset(this.HorizontalOffset - 1);
//                    this.UpdateLayout();
//                    next = gen.ContainerFromIndex(itemIndex - 1);
//                }
//            }
//            while (depth != 0) {
//                next = VisualTreeHelper.GetChild(next, 0);
//                depth--;
//            }
//            (next as UIElement).Focus();
//        }


//        #endregion

//        #region Override
//        protected override void OnKeyDown(KeyEventArgs e) {
//            switch (e.Key) {
//                case Key.Down:
//                    this.NavigateDown();
//                    e.Handled = true;
//                    break;
//                case Key.Left:
//                    this.NavigateLeft();
//                    e.Handled = true;
//                    break;
//                case Key.Right:
//                    this.NavigateRight();
//                    e.Handled = true;
//                    break;
//                case Key.Up:
//                    this.NavigateUp();
//                    e.Handled = true;
//                    break;
//                default:
//                    base.OnKeyDown(e);
//                    break;
//            }
//        }


//        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args) {
//            base.OnItemsChanged(sender, args);
//            this._abstractPanel = null;
//            this.ResetScrollInfo();
//        }

//        protected override void OnInitialized(EventArgs e) {
//            base.OnInitialized(e);
//            this._itemsControl = ItemsControl.GetItemsOwner(this);
//            this._children = this.InternalChildren;
//            this._generator = this.ItemContainerGenerator;
//            this.SizeChanged += new SizeChangedEventHandler(this.Resizing);
//        }

//        protected override Size MeasureOverride(Size availableSize) {
//            if (this._itemsControl == null || this._itemsControl.Items.Count == 0) {
//                if (availableSize.Width == double.PositiveInfinity || availableSize.Height == double.PositiveInfinity) return Size.Empty;
//                return availableSize;
//            }
//            if (this._abstractPanel == null)
//                this._abstractPanel = new WrapPanelAbstraction(this._itemsControl.Items.Count);

//            this._pixelMeasuredViewport = availableSize;

//            this._realizedChildLayout.Clear();

//            Size realizedFrameSize = availableSize;

//            int itemCount = this._itemsControl.Items.Count;
//            int firstVisibleIndex = this.GetFirstVisibleIndex();

//            GeneratorPosition startPos = this._generator.GeneratorPositionFromIndex(firstVisibleIndex);

//            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
//            int current = firstVisibleIndex;
//            int visibleSections = 1;
//            using (this._generator.StartAt(startPos, GeneratorDirection.Forward, true)) {
//                bool stop = false;
//                bool isHorizontal = this.Orientation == Orientation.Horizontal;
//                double currentX = 0;
//                double currentY = 0;
//                double maxItemSize = 0;
//                int currentSection = this.GetFirstVisibleSection();
//                while (current < itemCount) {
//                    bool newlyRealized;

//                    // Get or create the child                    
//                    UIElement child = this._generator.GenerateNext(out newlyRealized) as UIElement;
//                    if (newlyRealized) {
//                        // Figure out if we need to insert the child at the end or somewhere in the middle
//                        if (childIndex >= this._children.Count) {
//                            base.AddInternalChild(child);
//                        } else {
//                            base.InsertInternalChild(childIndex, child);
//                        }
//                        this._generator.PrepareItemContainer(child);
//                        child.Measure(this.ChildSlotSize);
//                    } else {
//                        // The child has already been created, let's be sure it's in the right spot
//                        Debug.Assert(child == this._children[childIndex], "Wrong child was generated");
//                    }
//                    this.childSize = child.DesiredSize;
//                    Rect childRect = new Rect(new Point(currentX, currentY), this.childSize);
//                    if (isHorizontal) {
//                        maxItemSize = Math.Max(maxItemSize, childRect.Height);
//                        if (childRect.Right > realizedFrameSize.Width) //wrap to a new line
//                        {
//                            currentY = currentY + maxItemSize;
//                            currentX = 0;
//                            maxItemSize = childRect.Height;
//                            childRect.X = currentX;
//                            childRect.Y = currentY;
//                            currentSection++;
//                            visibleSections++;
//                        }
//                        if (currentY > realizedFrameSize.Height)
//                            stop = true;
//                        currentX = childRect.Right;
//                    } else {
//                        maxItemSize = Math.Max(maxItemSize, childRect.Width);
//                        if (childRect.Bottom > realizedFrameSize.Height) //wrap to a new column
//                        {
//                            currentX = currentX + maxItemSize;
//                            currentY = 0;
//                            maxItemSize = childRect.Width;
//                            childRect.X = currentX;
//                            childRect.Y = currentY;
//                            currentSection++;
//                            visibleSections++;
//                        }
//                        if (currentX > realizedFrameSize.Width)
//                            stop = true;
//                        currentY = childRect.Bottom;
//                    }
//                    this._realizedChildLayout.Add(child, childRect);
//                    this._abstractPanel.SetItemSection(current, currentSection);

//                    if (stop)
//                        break;
//                    current++;
//                    childIndex++;
//                }
//            }
//            this.CleanUpItems(firstVisibleIndex, current - 1);

//            this.ComputeExtentAndViewport(availableSize, visibleSections);

//            if (availableSize.Width == double.PositiveInfinity || availableSize.Height == double.PositiveInfinity) return Size.Empty;
//            return availableSize;
//        }
//        protected override Size ArrangeOverride(Size finalSize) {
//            if (this._children != null) {
//                foreach (UIElement child in this._children) {
//                    var layoutInfo = this._realizedChildLayout[child];
//                    child.Arrange(layoutInfo);
//                }
//            }
//            return finalSize;
//        }

//        #endregion

//        #region IScrollInfo Members

//        private bool _canHScroll = false;
//        public bool CanHorizontallyScroll {
//            get { return this._canHScroll; }
//            set { this._canHScroll = value; }
//        }

//        private bool _canVScroll = false;
//        public bool CanVerticallyScroll {
//            get { return this._canVScroll; }
//            set { this._canVScroll = value; }
//        }

//        public double ExtentHeight {
//            get { return this._extent.Height; }
//        }

//        public double ExtentWidth {
//            get { return this._extent.Width; }
//        }

//        public double HorizontalOffset {
//            get { return this._offset.X; }
//        }

//        public double VerticalOffset {
//            get { return this._offset.Y; }
//        }

//        public void LineDown() {
//            if (this.Orientation == Orientation.Vertical)
//                this.SetVerticalOffset(this.VerticalOffset + 20);
//            else
//                this.SetVerticalOffset(this.VerticalOffset + 1);
//        }

//        public void LineLeft() {
//            if (this.Orientation == Orientation.Horizontal)
//                this.SetHorizontalOffset(this.HorizontalOffset - 20);
//            else
//                this.SetHorizontalOffset(this.HorizontalOffset - 1);
//        }

//        public void LineRight() {
//            if (this.Orientation == Orientation.Horizontal)
//                this.SetHorizontalOffset(this.HorizontalOffset + 20);
//            else
//                this.SetHorizontalOffset(this.HorizontalOffset + 1);
//        }

//        public void LineUp() {
//            if (this.Orientation == Orientation.Vertical)
//                this.SetVerticalOffset(this.VerticalOffset - 20);
//            else
//                this.SetVerticalOffset(this.VerticalOffset - 1);
//        }

//        public Rect MakeVisible(Visual visual, Rect rectangle) {
//            var gen = (ItemContainerGenerator)this._generator.GetItemContainerGeneratorForPanel(this);
//            var element = (UIElement)visual;
//            int itemIndex = gen.IndexFromContainer(element);
//            while (itemIndex == -1) {
//                element = (UIElement)VisualTreeHelper.GetParent(element);
//                itemIndex = gen.IndexFromContainer(element);
//            }
//            int section = this._abstractPanel[itemIndex].Section;
//            Rect elementRect = this._realizedChildLayout[element];
//            if (this.Orientation == Orientation.Horizontal) {
//                double viewportHeight = this._pixelMeasuredViewport.Height;
//                if (elementRect.Bottom > viewportHeight)
//                    this._offset.Y += 1;
//                else if (elementRect.Top < 0)
//                    this._offset.Y -= 1;
//            } else {
//                double viewportWidth = this._pixelMeasuredViewport.Width;
//                if (elementRect.Right > viewportWidth)
//                    this._offset.X += 1;
//                else if (elementRect.Left < 0)
//                    this._offset.X -= 1;
//            }
//            this.InvalidateMeasure();
//            return elementRect;
//        }

//        public void MouseWheelDown() {
//            this.PageDown();
//        }

//        public void MouseWheelLeft() {
//            this.PageLeft();
//        }

//        public void MouseWheelRight() {
//            this.PageRight();
//        }

//        public void MouseWheelUp() {
//            this.PageUp();
//        }

//        public void PageDown() {
//            this.SetVerticalOffset(this.VerticalOffset + this._viewport.Height * 0.8);
//        }

//        public void PageLeft() {
//            this.SetHorizontalOffset(this.HorizontalOffset - this._viewport.Width * 0.8);
//        }

//        public void PageRight() {
//            this.SetHorizontalOffset(this.HorizontalOffset + this._viewport.Width * 0.8);
//        }

//        public void PageUp() {
//            this.SetVerticalOffset(this.VerticalOffset - this._viewport.Height * 0.8);
//        }

//        private ScrollViewer _owner;
//        public ScrollViewer ScrollOwner {
//            get { return this._owner; }
//            set { this._owner = value; }
//        }

//        public void SetHorizontalOffset(double offset) {
//            if (offset < 0 || this._viewport.Width >= this._extent.Width) {
//                offset = 0;
//            } else {
//                if (offset + this._viewport.Width >= this._extent.Width) {
//                    offset = this._extent.Width - this._viewport.Width;
//                }
//            }

//            this._offset.X = offset;

//            if (this._owner != null)
//                this._owner.InvalidateScrollInfo();

//            this.InvalidateMeasure();
//            this.firstIndex = this.GetFirstVisibleIndex();
//        }

//        public void SetVerticalOffset(double offset) {
//            if (offset < 0 || this._viewport.Height >= this._extent.Height) {
//                offset = 0;
//            } else {
//                if (offset + this._viewport.Height >= this._extent.Height) {
//                    offset = this._extent.Height - this._viewport.Height;
//                }
//            }

//            this._offset.Y = offset;

//            if (this._owner != null)
//                this._owner.InvalidateScrollInfo();

//            //_trans.Y = -offset;

//            this.InvalidateMeasure();
//            this.firstIndex = this.GetFirstVisibleIndex();
//        }

//        public double ViewportHeight {
//            get { return this._viewport.Height; }
//        }

//        public double ViewportWidth {
//            get { return this._viewport.Width; }
//        }

//        #endregion

//        #region helper data structures

//        class ItemAbstraction {
//            public ItemAbstraction(WrapPanelAbstraction panel, int index) {
//                this._panel = panel;
//                this._index = index;
//            }

//            WrapPanelAbstraction _panel;

//            public readonly int _index;

//            int _sectionIndex = -1;
//            public int SectionIndex {
//                get {
//                    if (this._sectionIndex == -1) {
//                        return this._index % this._panel._averageItemsPerSection - 1;
//                    }
//                    return this._sectionIndex;
//                }
//                set {
//                    if (this._sectionIndex == -1)
//                        this._sectionIndex = value;
//                }
//            }

//            int _section = -1;
//            public int Section {
//                get {
//                    if (this._section == -1) {
//                        return this._index / this._panel._averageItemsPerSection;
//                    }
//                    return this._section;
//                }
//                set {
//                    if (this._section == -1)
//                        this._section = value;
//                }
//            }
//        }

//        class WrapPanelAbstraction : IEnumerable<ItemAbstraction> {
//            public WrapPanelAbstraction(int itemCount) {
//                List<ItemAbstraction> items = new List<ItemAbstraction>(itemCount);
//                for (int i = 0; i < itemCount; i++) {
//                    ItemAbstraction item = new ItemAbstraction(this, i);
//                    items.Add(item);
//                }

//                this.Items = new ReadOnlyCollection<ItemAbstraction>(items);
//                this._averageItemsPerSection = itemCount;
//                this._itemCount = itemCount;
//            }

//            public readonly int _itemCount;
//            public int _averageItemsPerSection;
//            private int _currentSetSection = -1;
//            private int _currentSetItemIndex = -1;
//            private int _itemsInCurrentSecction = 0;
//            private object _syncRoot = new object();

//            public int SectionCount {
//                get {
//                    int ret = this._currentSetSection + 1;
//                    if (this._currentSetItemIndex + 1 < this.Items.Count) {
//                        int itemsLeft = this.Items.Count - this._currentSetItemIndex;
//                        ret += itemsLeft / this._averageItemsPerSection + 1;
//                    }
//                    return ret;
//                }
//            }

//            private ReadOnlyCollection<ItemAbstraction> Items { get; set; }

//            public void SetItemSection(int index, int section) {
//                lock (this._syncRoot) {
//                    if (section <= this._currentSetSection + 1 && index == this._currentSetItemIndex + 1) {
//                        this._currentSetItemIndex++;
//                        this.Items[index].Section = section;
//                        if (section == this._currentSetSection + 1) {
//                            this._currentSetSection = section;
//                            if (section > 0) {
//                                this._averageItemsPerSection = (index) / (section);
//                            }
//                            this._itemsInCurrentSecction = 1;
//                        } else
//                            this._itemsInCurrentSecction++;
//                        this.Items[index].SectionIndex = this._itemsInCurrentSecction - 1;
//                    }
//                }
//            }

//            public ItemAbstraction this[int index] {
//                get { return this.Items[index]; }
//            }

//            #region IEnumerable<ItemAbstraction> Members

//            public IEnumerator<ItemAbstraction> GetEnumerator() {
//                return this.Items.GetEnumerator();
//            }

//            #endregion

//            #region IEnumerable Members

//            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
//                return this.GetEnumerator();
//            }

//            #endregion
//        }

//        #endregion
//    }
//}
