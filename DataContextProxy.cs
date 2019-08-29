using System.Windows;

namespace YoloMarkNet
{
    public class DataContextProxy : Freezable
    {
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register(
                "DataContext",
                typeof(object),
                typeof(DataContextProxy),
                new PropertyMetadata(null));

        public object DataContext
        {
            get => GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

        protected override Freezable CreateInstanceCore() => new DataContextProxy();
    }
}