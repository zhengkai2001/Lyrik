using Microsoft.WindowsAPICodePack.Dialogs;

namespace Lyrik.Gui
{
    internal class DirectoryBrowser
    {
        private static readonly string OperatingSystem = System.Environment.OSVersion.ToString();

        private DirectoryBrowser() { }

        public static string Browser(string initialDirectory, string caption)
        {
            string result = "";

            //MessageBox.Show(os, "version", MessageBoxButton.OK, MessageBoxImage.Information);

            //if run on Windows XP
            if (OperatingSystem.Contains("5.1"))
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
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
                OpenDirectoryDialog dialog = null;
                try
                {
                    dialog = new OpenDirectoryDialog(initialDirectory, caption);
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        result = dialog.FileName;
                    }
                }
                finally
                {
                    dialog?.Dispose();
                }
            }

            return result;
        }
    }
}
