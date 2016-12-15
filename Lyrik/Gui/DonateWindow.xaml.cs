using System.Windows;

namespace Lyrik.Gui
{
    /// <summary>
    /// DonateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DonateWindow : Window
    {
        public DonateWindow()
        {
            InitializeComponent();
        }

        private void close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
