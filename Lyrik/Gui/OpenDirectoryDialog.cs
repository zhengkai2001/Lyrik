using Microsoft.WindowsAPICodePack.Dialogs;
using System;

namespace Lyrik.Gui
{
    class OpenDirectoryDialog : IDisposable
    {
        private CommonOpenFileDialog cofd;
        public string FileName;
        public OpenDirectoryDialog(string initDir, string title)
        {
            cofd = new CommonOpenFileDialog();

            cofd.Title = title;
            cofd.IsFolderPicker = true;
            cofd.DefaultDirectory = initDir;

            cofd.AddToMostRecentlyUsedList = false;
            cofd.AllowNonFileSystemItems = false;
            cofd.EnsureFileExists = true;
            cofd.EnsurePathExists = true;
            cofd.EnsureReadOnly = false;
            cofd.EnsureValidNames = true;
            cofd.Multiselect = false;
            cofd.ShowPlacesList = true;

            FileName = initDir;
        }
        public CommonFileDialogResult ShowDialog()
        {
            CommonFileDialogResult cfr = cofd.ShowDialog();
            if (cfr == CommonFileDialogResult.Ok)
            {
                this.FileName = cofd.FileName;
            }
            return cfr;
        }

        public void Dispose()
        {
            cofd.Dispose();
        }
    }
}
