﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;

namespace Lyrik.Gui
{
    internal class OpenDirectoryDialog : IDisposable
    {
        private readonly CommonOpenFileDialog _dialog;
        public string FileName;

        public OpenDirectoryDialog(string initDir, string title)
        {
            _dialog = new CommonOpenFileDialog
            {
                Title = title,
                IsFolderPicker = true,
                DefaultDirectory = initDir,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            FileName = initDir;
        }

        public CommonFileDialogResult ShowDialog()
        {
            var cfr = _dialog.ShowDialog();
            if (cfr == CommonFileDialogResult.Ok)
            {
                FileName = _dialog.FileName;
            }
            return cfr;
        }

        public void Dispose()
        {
            _dialog.Dispose();
        }
    }
}
