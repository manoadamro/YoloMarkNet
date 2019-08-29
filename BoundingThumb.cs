namespace YoloMarkNet
{
    //Adapted from https://www.codeproject.com/Articles/22952/WPF-Diagram-Designer-Part
    public class BoundingThumb : Thumb
    {
        public BoundingThumb() => DragDelta += new DragDeltaEventHandler(OnDragDelta);

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var item = DataContext as ContentPresenter;
            if (item == null) return;
            double left = Canvas.GetLeft(item);
            double top = Canvas.GetTop(item);
            Canvas.SetLeft(item, left + e.HorizontalChange);
            Canvas.SetTop(item, top + e.VerticalChange);
        }
    }
}