using Lyrik.Lyrics;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using WPFCustomMessageBox;

namespace Lyrik.Gui
{
    public delegate void TaskCompleted();

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : IDisposable
    {
        private const string UpdateUrl = @"http://pan.baidu.com/s/1dD1slCt";
        private const string SongDirFile = @".\song_dir.ini";
        private const string DefaultSongDir = @"D:\Music";

        private const string StatusLabelReady = "就绪";
        private const string StatusLabelAdding = "添加歌词中...";
        private const string StatusLabelPause = "已暂停";

        private readonly LyricAdder _lyricAdder;
        private Thread _thread;
        private readonly TaskCompleted _taskCompleted;

        public MainWindow()
        {
            InitializeComponent();

            InitComponents();

            SongDirTextBox.Text = GetSongDir();
            AddLyricForAllRadioButton.IsChecked = true;

            _lyricAdder = new LyricAdder();
            _lyricAdder.SetStatusTextBox(StatusTextBox);
            _lyricAdder.SetStatusLabel(StatusLabel, StatusLabelAdding);

            //_taskCompleted = new TaskCompleted(InitComponents);
            _taskCompleted = InitComponents;
        }

        private void InitComponents()
        {
            AddLyricForAllRadioButton.IsEnabled = true;
            AddLyricForEmptyRadioButton.IsEnabled = true;

            StartButton.IsEnabled = true;
            PauseResumeButton.Content = "暂停";
            PauseResumeButton.IsEnabled = false;
            PauseResumeButton.Click -= Pause;
            PauseResumeButton.Click -= Resume;
            PauseResumeButton.Click += Pause;
            HaltButton.IsEnabled = false;

            StatusLabel.Content = StatusLabelReady;
        }

        private void SwitchLanguage(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            string culture;
            switch (menuItem.Name)
            {
                case "LanguageSimplifiedChinese":
                    culture = "zh-CN";
                    break;
                //case "LanguageEnglish":
                default:
                    culture = "en-US";
                    break;
            }
            App.SetCulture(culture);
        }

        private static string GetSongDir()
        {
            var fi = new FileInfo(SongDirFile);
            if (!fi.Exists)
            {
                return DefaultSongDir;
            }

            var sr = new StreamReader(fi.OpenRead());
            var songDirStr = sr.ReadLine();
            sr.Close();

            return string.IsNullOrWhiteSpace(songDirStr) ? DefaultSongDir : songDirStr;
        }

        private void BrowserSongDir(object sender, EventArgs e)
        {
            var browserSongDirCaption = App.CultureDictionary["BrowserSongDirCaption"].ToString();
            var result = DirectoryBrowser.Browser(SongDirTextBox.Text, browserSongDirCaption);
            if (!string.IsNullOrEmpty(result))
            {
                SongDirTextBox.Text = result;
            }
        }

        private void AddLyric(object sender, EventArgs e)
        {
            StatusTextBox.Clear();

            var songDir = SongDirTextBox.Text;
            if (!new DirectoryInfo(songDir).Exists)
            {
                StatusTextBox.AppendText("指定的音乐文件目录不存在，请修改~\n");
                return;
            }

            //保存当前歌曲目录信息到文件，以便下次运行程序时读取使用
            //以下为微软建议的写法：https://msdn.microsoft.com/zh-cn/library/ms182334
            //别的写法可能会出现CA2202
            Stream stream = null;
            try
            {
                stream = new FileStream(SongDirFile, FileMode.Create, FileAccess.Write);
                using (var writer = new StreamWriter(stream))
                {
                    stream = null;
                    writer.WriteLine(songDir);
                }
            }
            finally
            {
                stream?.Dispose();
            }

            if (_thread != null && _thread.ThreadState != ThreadState.Stopped)
            {
                return;
            }

            StartButton.IsEnabled = false;
            PauseResumeButton.Content = "暂停";
            PauseResumeButton.IsEnabled = true;
            HaltButton.IsEnabled = true;

            AddLyricForAllRadioButton.IsEnabled = false;
            AddLyricForEmptyRadioButton.IsEnabled = false;

            StatusLabel.Content = StatusLabelAdding;

            _lyricAdder.SetSongDir(songDir);
            _lyricAdder.SetAddLyricRules(AddLyricForEmptyRadioButton.IsChecked ?? false);

            _thread = new Thread(AddLyric);
            _thread.Start();
        }

        private void AddLyric()
        {
            _lyricAdder.AddLyric();

            Dispatcher.BeginInvoke(_taskCompleted);
        }

        private void Pause(object sender, EventArgs e)
        {
            _lyricAdder.Pause();

            StatusLabel.Content = StatusLabelPause;

            PauseResumeButton.Content = "继续";
            PauseResumeButton.Click -= Pause;
            PauseResumeButton.Click += Resume;
        }

        private void Resume(object sender, EventArgs e)
        {
            _lyricAdder.Resume();

            StatusLabel.Content = StatusLabelAdding;

            PauseResumeButton.Content = "暂停";
            PauseResumeButton.Click -= Resume;
            PauseResumeButton.Click += Pause;
        }

        private void Update(object sender, EventArgs e)
        {
            Update();
        }

        private static void Update()
        {
            System.Diagnostics.Process.Start(UpdateUrl);
        }

        private void HaltClick(object sender, EventArgs e)
        {
            Halt();
            InitComponents();
        }

        private void Halt()
        {
            if (_thread == null)
            {
                return;
            }
            if (_thread.ThreadState != ThreadState.Running
                && _thread.ThreadState != ThreadState.WaitSleepJoin)
            {
                return;
            }

            _lyricAdder.Halt();
            _thread.Join();
            AddLyricForAllRadioButton.IsEnabled = true;
            AddLyricForEmptyRadioButton.IsEnabled = true;
        }

        private bool _exitConfirmed;

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (ConfirmExit())
            {
                base.OnClosing(e);
                Exit();
            }
            else
            {
                if (e != null)
                {
                    e.Cancel = true;
                }
            }
        }

        private bool ConfirmExit()
        {
            if (_exitConfirmed)
            {
                return true;
            }

            var confirmExitCaption = App.CultureDictionary["ConfirmExitCaption"].ToString();
            var confirmExit = App.CultureDictionary["ConfirmExit"].ToString();

            if (CustomMessageBox.Show(confirmExit, confirmExitCaption, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return false;
            }
            _exitConfirmed = true;
            return true;
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            if (ConfirmExit())
            {
                Exit();
            }
        }

        private void Exit()
        {
            Halt();
            Application.Current.Shutdown();

            //强制结束线程，不好
            //_thread.Abort();

            //强制结束程序，不好
            //Environment.Exit(0);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                _lyricAdder.Dispose();
            }

            _disposed = true;
        }

        private void ShowHelpWindow(object sender, RoutedEventArgs e)
        {
            new HelpWindow().ShowDialog();
        }

        private void ShowAboutWindow(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void ShowDonateWindow(object sender, RoutedEventArgs e)
        {
            new DonateWindow().ShowDialog();
        }
    }
}
