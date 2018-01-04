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

        private readonly LyricAdder _lyricAdder;
        private Thread _thread;
        private readonly TaskCompleted _taskCompleted;

        internal static MainWindow Window;

        public MainWindow()
        {
            Window = this;

            InitializeComponent();
            InitComponents();

            var musicDir = Properties.Settings.Default.MusicDir.Trim();
            if (string.IsNullOrEmpty(musicDir))
            {
                musicDir = Properties.Settings.Default.DefaultMusicDir.Trim();
            }
            MusicDirTextBox.Text = musicDir;

            AddLyricForAllRadioButton.IsChecked = true;

            _lyricAdder = new LyricAdder();

            _taskCompleted = InitComponents;
        }

        private void InitComponents()
        {
            AddLyricForAllRadioButton.IsEnabled = true;
            AddLyricForEmptyRadioButton.IsEnabled = true;

            StartButton.IsEnabled = true;
            PauseResumeButton.Content = App.GetLocalizedString("ButtonPause");
            PauseResumeButton.IsEnabled = false;
            PauseResumeButton.Click -= Pause;
            PauseResumeButton.Click -= Resume;
            PauseResumeButton.Click += Pause;
            HaltButton.IsEnabled = false;

            SetStatus(App.GetLocalizedString("StatusLabelReady"));
        }

        public void SetStatus(string status)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                StatusLabel.Content = status;
            }));
        }

        public void AppendRecord(string record)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                RecordTextBox.AppendText(record);
                RecordTextBox.ScrollToEnd();
            }));
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

        private void BrowserSongDir(object sender, RoutedEventArgs e)
        {
            var browserSongDirCaption = App.GetLocalizedString("BrowserSongDirCaption");
            var result = DirectoryBrowser.Browser(MusicDirTextBox.Text, browserSongDirCaption);
            if (!string.IsNullOrEmpty(result))
            {
                MusicDirTextBox.Text = result;
            }
        }

        private void AddLyric(object sender, EventArgs e)
        {
            RecordTextBox.Clear();

            var musicDir = MusicDirTextBox.Text;
            if (!new DirectoryInfo(musicDir).Exists)
            {
                RecordTextBox.AppendText(App.GetLocalizedString("MusicDirNotExist") + "\n");
                return;
            }

            Properties.Settings.Default.MusicDir = musicDir;
            Properties.Settings.Default.Save();

            if (_thread != null
                && _thread.ThreadState != ThreadState.Stopped
                && _thread.ThreadState != ThreadState.Aborted
                && _thread.ThreadState != ThreadState.AbortRequested)
            {
                return;
            }

            StartButton.IsEnabled = false;
            PauseResumeButton.Content = App.GetLocalizedString("ButtonPause");
            PauseResumeButton.IsEnabled = true;
            HaltButton.IsEnabled = true;

            AddLyricForAllRadioButton.IsEnabled = false;
            AddLyricForEmptyRadioButton.IsEnabled = false;

            StatusLabel.Content = App.GetLocalizedString("StatusLabelAdding");

            _lyricAdder.SetSongDir(musicDir);
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

            StatusLabel.Content = App.GetLocalizedString("StatusLabelPause");

            PauseResumeButton.Content = App.GetLocalizedString("ButtonResume");
            PauseResumeButton.Click -= Pause;
            PauseResumeButton.Click += Resume;
        }

        private void Resume(object sender, EventArgs e)
        {
            _lyricAdder.Resume();

            StatusLabel.Content = App.GetLocalizedString("StatusLabelAdding");

            PauseResumeButton.Content = App.GetLocalizedString("ButtonPause");
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
            if (_thread != null
                && _thread.ThreadState != ThreadState.Running
                && _thread.ThreadState != ThreadState.WaitSleepJoin)
            {
                return;
            }

            _lyricAdder.Halt();

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

            var confirmExitCaption = App.GetLocalizedString("ConfirmExitCaption");
            var confirmExit = App.GetLocalizedString("ConfirmExit");

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

            // bad ways to exit
            //_thread.Abort();
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
