using Lyrik.Lyrics;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using WPFCustomMessageBox;

namespace Lyrik.Gui
{
    public delegate void TaskCompleted();

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private const string updateURL = @"http://pan.baidu.com/s/1dD1slCt";
        private const string songDirFile = @".\song_dir.ini";
        private const string defaultSongDir = @"D:\Music";

        private const string statusLabelReady = "就绪";
        private const string statusLabelAdding = "添加歌词中...";
        private const string statusLabelPause = "已暂停";

        private LyricAdder lyricAdder;
        private Thread thread;
        private TaskCompleted taskCompleted;

        private ResourceManager stringManager;

        public MainWindow()
        {
            InitializeComponent();

            initComponents();

            songDirTextBox.Text = getSongDir();
            addLyricForAllRadioButton.IsChecked = true;

            lyricAdder = new LyricAdder();
            lyricAdder.setStatusTextBox(statusTextBox);
            lyricAdder.setStatusLabel(statusLabel, statusLabelAdding);

            taskCompleted = new TaskCompleted(initComponents);

            stringManager = new ResourceManager("Lyrik.Resources.StringResource", Assembly.GetExecutingAssembly());
        }

        private void initComponents()
        {
            addLyricForAllRadioButton.IsEnabled = true;
            addLyricForEmptyRadioButton.IsEnabled = true;

            startButton.IsEnabled = true;
            pauseResumeButton.Content = "暂停";
            pauseResumeButton.IsEnabled = false;
            pauseResumeButton.Click -= pause;
            pauseResumeButton.Click -= resume;
            pauseResumeButton.Click += pause;
            haltButton.IsEnabled = false;

            statusLabel.Content = statusLabelReady;
        }

        private static string getSongDir()
        {
            string songDirStr = null;
            FileInfo fi = new FileInfo(songDirFile);
            if (fi.Exists)
            {
                StreamReader sr = new StreamReader(fi.OpenRead());
                songDirStr = sr.ReadLine();
                sr.Close();
            }

            return String.IsNullOrWhiteSpace(songDirStr) ? defaultSongDir : songDirStr;
        }

        private void browserSongDir(object sender, EventArgs e)
        {
            string browserSongDirCaption = stringManager.GetString("browserSongDirCaption", CultureInfo.CurrentUICulture);
            string result = DirectoryBrowser.browser(songDirTextBox.Text, browserSongDirCaption);
            if (!string.IsNullOrEmpty(result))
            {
                songDirTextBox.Text = result;
            }
        }

        private void addLyric(object sender, EventArgs e)
        {
            statusTextBox.Clear();

            string songDir = songDirTextBox.Text;
            if (!new DirectoryInfo(songDir).Exists)
            {
                statusTextBox.AppendText("指定的音乐文件目录不存在，请修改~\n");
                return;
            }

            //保存当前歌曲目录信息到文件，以便下次运行程序时读取使用
            //以下为微软建议的写法：https://msdn.microsoft.com/zh-cn/library/ms182334
            //别的写法可能会出现CA2202
            Stream stream = null;
            try
            {
                stream = new FileStream(songDirFile, FileMode.Create, FileAccess.Write);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    stream = null;
                    writer.WriteLine(songDir);
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }

            if (thread == null ||
                thread.ThreadState == ThreadState.Stopped)
            {
                startButton.IsEnabled = false;
                pauseResumeButton.Content = "暂停";
                pauseResumeButton.IsEnabled = true;
                haltButton.IsEnabled = true;

                addLyricForAllRadioButton.IsEnabled = false;
                addLyricForEmptyRadioButton.IsEnabled = false;

                statusLabel.Content = statusLabelAdding;

                lyricAdder.setSongDir(songDir);
                lyricAdder.setAddLyricRules(addLyricForEmptyRadioButton.IsChecked ?? false);

                thread = new Thread(new ThreadStart(addLyric));
                thread.Start();
            }
        }

        private void addLyric()
        {
            lyricAdder.addLyric();

            this.Dispatcher.BeginInvoke(taskCompleted);
        }

        private void pause(object sender, EventArgs e)
        {
            lyricAdder.pause();

            statusLabel.Content = statusLabelPause;

            pauseResumeButton.Content = "继续";
            pauseResumeButton.Click -= pause;
            pauseResumeButton.Click += resume;
        }

        private void resume(object sender, EventArgs e)
        {
            lyricAdder.resume();

            statusLabel.Content = statusLabelAdding;

            pauseResumeButton.Content = "暂停";
            pauseResumeButton.Click -= resume;
            pauseResumeButton.Click += pause;
        }

        private void update(object sender, EventArgs e)
        {
            update();
        }

        private static void update()
        {
            System.Diagnostics.Process.Start(updateURL);
        }

        private void haltClick(object sender, EventArgs e)
        {
            halt();
            initComponents();
        }

        private void halt()
        {
            if (thread == null)
            {
                return;
            }
            if (thread.ThreadState == ThreadState.Running
                        || thread.ThreadState == ThreadState.WaitSleepJoin)
            {
                lyricAdder.halt();
                thread.Join();
                addLyricForAllRadioButton.IsEnabled = true;
                addLyricForEmptyRadioButton.IsEnabled = true;
            }
        }

        private bool exitConfirmed = false;

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (confirmExit())
            {
                base.OnClosing(e);
                exit();
            }
            else
            {
                if (e != null)
                {
                    e.Cancel = true;
                }
            }
        }

        private bool confirmExit()
        {
            if (exitConfirmed)
            {
                return true;
            }

            string confirmExitCaption = stringManager.GetString("confirmExitCaption", CultureInfo.CurrentUICulture);
            string confirmExit = stringManager.GetString("confirmExit", CultureInfo.CurrentUICulture);
            if (CustomMessageBox.Show(confirmExitCaption, confirmExit, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                exitConfirmed = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void exitClick(object sender, RoutedEventArgs e)
        {
            if (confirmExit())
            {
                exit();
            }
        }

        private void exit()
        {
            halt();
            Application.Current.Shutdown();

            //强制结束线程，不好
            //thread.Abort();

            //强制结束程序，不好
            //Environment.Exit(0);
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                lyricAdder.Dispose();
            }

            disposed = true;
        }

        private void showHelpWindow(object sender, RoutedEventArgs e)
        {
            new HelpWindow().ShowDialog();
        }

        private void showAboutWindow(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void showDonateWindow(object sender, RoutedEventArgs e)
        {
            new DonateWindow().ShowDialog();
        }
    }
}
