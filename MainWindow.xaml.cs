namespace YoloMarkNet
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            ((MainWindowViewModel) DataContext).MouseMove(e.GetPosition(img));
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel) DataContext).MouseDown(e.GetPosition(img));
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel) DataContext).MouseUp(e.GetPosition(img));
        }

        private void Control_MouseLeave(object sender, MouseEventArgs e)
        {
            ((MainWindowViewModel) DataContext).MouseLeave();
        }
    }
}