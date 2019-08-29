﻿using System;

namespace YoloMarkNet
{
    //Adapted from https://www.codeproject.com/Articles/22952/WPF-Diagram-Designer-Part
    public class ResizeThumb : Thumb
    {
        public ResizeThumb() => DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var item = this.DataContext as ContentPresenter;
            if (item != null)
            {
                double deltaVertical, deltaHorizontal;

                switch (VerticalAlignment)
                {
                    case System.Windows.VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(
                            -e.VerticalChange,
                            item.ActualHeight - item.MinHeight);
                        item.Height -= deltaVertical;
                        break;
                    case System.Windows.VerticalAlignment.Top:
                        deltaVertical = Math.Min(
                            e.VerticalChange,
                            item.ActualHeight - item.MinHeight);
                        Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
                        item.Height -= deltaVertical;
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case System.Windows.HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min(
                            e.HorizontalChange,
                            item.ActualWidth - item.MinWidth);
                        Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
                        item.Width -= deltaHorizontal;
                        break;
                    case System.Windows.HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(
                            -e.HorizontalChange,
                            item.ActualWidth - item.MinWidth);
                        item.Width -= deltaHorizontal;
                        break;
                }
            }

            e.Handled = true;
        }
    }
}