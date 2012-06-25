using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace UserTreeLib
{
    public class UserTreePanel : VirtualizingPanel, IScrollInfo
    {
        public const double ITEM_HEIGHT = 24;

        private double _itemWidth = 0;
        private Size _measureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        private TranslateTransform _translateTransform;
        private ScrollViewer _scrollOwner;
        private Size _extent;
        private Size _viewport = new Size(0, 0);
        private bool _vertScrollable = false;
        private Point _offset = new Point(0, 0);

        public UserTreePanel()
        {
            RenderTransform = _translateTransform = new TranslateTransform();
            _extent = new Size(0, 0);
            _vertScrollable = false;
            _offset = new Point(0, 0);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UIElementCollection children = Children;
            IItemContainerGenerator generator = ItemContainerGenerator;

            RewindScrollBar(availableSize);

            int firstVisibleIndex = GetFirstVisibleIndex();
            int lastVisibleIndex = GetLastVisibleIndex();

            UpdateScrollInfo(availableSize);

            GeneratorPosition start = generator.GeneratorPositionFromIndex(firstVisibleIndex);

            int childIndex = (start.Offset == 0) ? start.Index : start.Index + 1;

            using (generator.StartAt(start, GeneratorDirection.Forward, true))
            {
                _itemWidth = 0;

                for (int i = firstVisibleIndex; i <= lastVisibleIndex; ++i, ++childIndex)
                {
                    bool isNewlyRealized;

                    UIElement child = generator.GenerateNext(out isNewlyRealized) as UIElement;
                    if (isNewlyRealized)
                    {
                        if (childIndex >= children.Count)
                        {
                            base.AddInternalChild(child);
                        }
                        else
                        {
                            base.InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    }
                    else
                    {
                        Debug.Assert(child == children[childIndex]);
                    }

                    child.Measure(_measureSize);
                    if (child.DesiredSize.Width > _itemWidth)
                        _itemWidth = child.DesiredSize.Width;
                }
            }

            for (int i = children.Count - 1; i >= 0; i--)
            {
                GeneratorPosition childGeneratorPosition = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPosition);
                if (itemIndex < firstVisibleIndex || itemIndex > lastVisibleIndex)
                {
                    generator.Remove(childGeneratorPosition, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }

            double width = availableSize.Width;
            if (double.IsInfinity(width))
                width = _itemWidth;

            double height = availableSize.Height;
            if (double.IsInfinity(height))
                height = (lastVisibleIndex - firstVisibleIndex + 1) * ITEM_HEIGHT;

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateScrollInfo(finalSize);

            for (int i = 0; i < Children.Count; i++)
            {
                ArrangeChild(ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0)), Children[i], finalSize);
            }

            return finalSize;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
        }

        private Size CalculateExtent(Size availableSize, int itemCount)
        {
            return new Size(_itemWidth, ITEM_HEIGHT * itemCount);
        }

        private int GetItemsCount()
        {
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            return itemsControl.Items.Count;
        }

        private int GetFirstVisibleIndex()
        {
            return (int)Math.Floor(_offset.Y / ITEM_HEIGHT);
        }

        private int GetLastVisibleIndex()
        {
            if (_viewport.Height == 0)
                return GetItemsCount() - 1;
            else
                return Math.Min((int)Math.Ceiling((_offset.Y + _viewport.Height) / ITEM_HEIGHT) - 1, GetItemsCount() - 1);
        }

        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            if (itemIndex == 0)
                child.Arrange(new Rect(0, 0, _itemWidth, ITEM_HEIGHT));
            else
                child.Arrange(new Rect(0, itemIndex * ITEM_HEIGHT, _itemWidth, ITEM_HEIGHT));
        }

        private void RewindScrollBar(Size availableSize)
        {
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.Items.Count;

            Size extent = CalculateExtent(availableSize, itemCount);
            if (extent.Height < _viewport.Height)
            {
                if (_offset.Y != 0)
                    SetVerticalOffset(0);
            }
        }

        private void UpdateScrollInfo(Size availableSize)
        {
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.Items.Count;

            Size extent = CalculateExtent(availableSize, itemCount);
            if (extent != _extent)
            {
                _extent = extent;
                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }

            if (availableSize != _viewport)
            {
                _viewport = availableSize;
                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }
        }

        #region IScrollInfo

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else if (offset + _viewport.Height >= _extent.Height)
            {
                offset = _extent.Height - _viewport.Height;
            }

            _offset.Y = offset;

            if (_scrollOwner != null)
                _scrollOwner.InvalidateScrollInfo();

            _translateTransform.Y = -offset;

            InvalidateMeasure();
        }

        public ScrollViewer ScrollOwner { get { return _scrollOwner; } set { _scrollOwner = value; } }

        public bool CanVerticallyScroll
        {
            get
            {
                return _vertScrollable;
            }
            set
            {
                _vertScrollable = value;
            }
        }

        public double ExtentHeight
        {
            get
            {
                return _extent.Height;
            }
        }

        public double ExtentWidth
        {
            get
            {
                return _extent.Width;
            }
        }

        public double HorizontalOffset
        {
            get
            {
                return _offset.X;
            }
        }

        public double VerticalOffset
        {
            get
            {
                return _offset.Y;
            }
        }

        public double ViewportHeight
        {
            get
            {
                return _viewport.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return _viewport.Width;
            }
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - ITEM_HEIGHT);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + ITEM_HEIGHT);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ((int)(_viewport.Height / ITEM_HEIGHT)) * ITEM_HEIGHT);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ((int)(_viewport.Height / ITEM_HEIGHT)) * ITEM_HEIGHT);
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - ITEM_HEIGHT);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + ITEM_HEIGHT);
        }

        public Rect MakeVisible(UIElement visual, Rect rectangle)
        {
            return new Rect();
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || _viewport.Width >= _extent.Width)
            {
                offset = 0;
            }
            else if (offset + _viewport.Width >= _extent.Width)
            {
                offset = _extent.Width - _viewport.Width;
            }

            _offset.X = offset;

            if (_scrollOwner != null)
                _scrollOwner.InvalidateScrollInfo();

            _translateTransform.X = -offset;

            InvalidateMeasure();
        }

        public bool CanHorizontallyScroll
        {
            get
            {
                return true;
            }
            set { }
        }

        public void LineLeft()
        {
            SetVerticalOffset(HorizontalOffset - 5);
        }

        public void LineRight()
        {
            SetVerticalOffset(HorizontalOffset + 5);
        }

        public void MouseWheelLeft() { }
        public void MouseWheelRight() { }

        public void PageLeft() { }
        public void PageRight() { }


        #endregion

    }
}
