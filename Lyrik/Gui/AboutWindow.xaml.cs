using System.Windows;

namespace Lyrik.Gui
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
