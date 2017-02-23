using Microsoft.WindowsAPICodePack.Dialogs;

namespace Lyrik.Gui
{
    internal class DirectoryBrowser
    {
        private static readonly string OperatingSystem = System.Environment.OSVersion.ToString();

        private DirectoryBrowser() { }

        public static string Browser(string initialDirectory, string caption)
        {
            var result = "";

            //MessageBox.Show(os, "version", MessageBoxButton.OK, MessageBoxImage.Information);

            //if run on Windows XP
            if (OperatingSystem.Contains("5.1"))
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                try
                {
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        {
                            result = dialog.SelectedPath;
                        }
                    }
                }
                finally
                {
                    dialog.Dispose();
                }
            }
            else
            {
                var dialog = new OpenDirectoryDialog(initialDirectory, caption);
                try
                {
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        result = dialog.DirName;
                    }
                }
                finally
                {
                    dialog.Dispose();
                }
            }

            return result;
        }
    }
}
