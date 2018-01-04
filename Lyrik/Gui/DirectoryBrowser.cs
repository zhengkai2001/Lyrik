using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Lyrik.Gui
{
    internal class DirectoryBrowser
    {
        private static readonly string OperatingSystem = System.Environment.OSVersion.ToString();

        private DirectoryBrowser() { }

        [SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "CA is wrong about this!")]
        public static string Browser(string initialDirectory, string caption)
        {
            //MessageBox.Show(os, "version", MessageBoxButton.OK, MessageBoxImage.Information);

            //if run on Windows XP
            if (OperatingSystem.Contains("5.1"))
            {
                FolderBrowserDialog dialog1 = new FolderBrowserDialog();
                try
                {
                    return dialog1.ShowDialog() == DialogResult.OK ? dialog1.SelectedPath : initialDirectory;
                }
                finally
                {
                    dialog1?.Dispose();
                }
            }
            else
            {
                CommonOpenFileDialog dialog2 = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    InitialDirectory = initialDirectory,
                    Title = caption,

                    AddToMostRecentlyUsedList = false,
                    AllowNonFileSystemItems = false,
                    EnsureFileExists = true,
                    EnsurePathExists = true,
                    EnsureReadOnly = false,
                    EnsureValidNames = true,
                    Multiselect = false,
                    ShowPlacesList = true
                };

                try
                {
                    return dialog2.ShowDialog() == CommonFileDialogResult.Ok ? dialog2.FileName : initialDirectory;
                }
                finally
                {
                    dialog2?.Dispose();
                }
            }
        }
    }
}
