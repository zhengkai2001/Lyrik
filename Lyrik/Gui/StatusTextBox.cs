using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Lyrik.Gui
{
    class StatusTextBox : TextBox
    {
        public void appendStatus(string status)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () {
                this.AppendText(status);
                this.ScrollToEnd();
            });
        }
    }
}
