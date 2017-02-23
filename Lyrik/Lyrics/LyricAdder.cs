using Lyrik.LyricHelpers;
using Lyrik.Utilities;
using System;
using System.Collections.Generic;
using TagLib;

namespace Lyrik.Lyrics
{
    internal class LyricAdder : BasicTask
    {
        private static readonly string[] SupportedFileTypes = { ".MP3", ".M4A" };

        private readonly List<LyricHelper> _lyricHelpers;

        private string _songDir;
        private bool _addLyricForEmpty;

        private static string _statusLabelAdding;

        public LyricAdder()
        {
            _lyricHelpers = new List<LyricHelper>
            {
                new AzlyricsLyricHelper(),
                new BaiduLyricHelper()
            };

            TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF8;
            TagLib.Id3v2.Tag.ForceDefaultEncoding = true;

            _statusLabelAdding = App.GetLocalizedString("StatusLabelAdding");
        }

        public void SetSongDir(string songDir)
        {
            _songDir = songDir;
        }

        public void SetAddLyricRules(bool addLyricForEmpty)
        {
            _addLyricForEmpty = addLyricForEmpty;
        }

        public void AddLyric()
        {
            var isHalted = false;
            TaskBegin();

            AppendRecord("=== " + App.GetLocalizedString("RecordStartAdding") + " ===\n");

            var foundCount = 0;
            var allCount = 0;
            var alreadyCount = 0;

            var fileList = FileOperations.Travel(_songDir, SupportedFileTypes);

            foreach (var fi in fileList)
            {
                ++allCount;

                SetStatus(_statusLabelAdding + " (" + allCount + "/" + fileList.Count + ")");

                if (FileOperations.IsFileLocked(fi))
                {
                    AppendRecord(fi.Name + " " + App.GetLocalizedString("RecordFileInUse") + "\n");
                    continue;
                }

                File songFile = null;
                try
                {
                    songFile = File.Create(fi.FullName, ReadStyle.Average);

                    //TagLib.Id3v2.Tag tag = songFile.GetTag(TagLib.TagTypes.Id3v2, true) as TagLib.Id3v2.Tag;
                    var tag = songFile.GetTag(TagTypes.Id3v2, true) as TagLib.Id3v2.Tag ??
                                     (Tag)(songFile.GetTag(TagTypes.Id3v1, true) as TagLib.Id3v1.Tag);

                    if (tag == null)
                    {
                        AppendRecord(fi.Name + " " + App.GetLocalizedString("RecordAbnormalTag") + "\n");
                        continue;
                    }

                    var title = tag.Title ?? fi.Name.Replace(fi.Extension, "");
                    var performer = tag.Performers.Length == 0 ? "" : tag.Performers[0];

                    title = Text.ProcessString(title);
                    performer = Text.ProcessString(performer);

                    //if (abnormalTagText)
                    //{
                    //    //AppendRecord("abnormal~" + title + " " + performer);

                    //    //songFile.RemoveTags(TagLib.TagTypes.Id3v1);

                    //    //TagLib.Tag newTag = new TagLib.Id3v2.Tag();
                    //    //newTag.Title = title;
                    //    //newTag.Performers = new string[1];
                    //    //newTag.Performers[0] = performer;
                    //    //newTag.Year = tag.Year;

                    //    //songFile.TagTypes = newTag;

                    //    //tag.Clear();
                    //}

                    AppendRecord(title, performer);

                    if (tag.Genres.Length != 0)
                    {
                        var genre = tag.Genres[0];
                        if (genre.Equals("classical", StringComparison.OrdinalIgnoreCase) || genre.Equals("classic", StringComparison.OrdinalIgnoreCase))
                        {
                            AppendRecord(App.GetLocalizedString("RecordClassicalMusic") + "\n");
                            continue;
                        }
                    }

                    // skip songs that already have lyrics
                    if (_addLyricForEmpty && !string.IsNullOrEmpty(tag.Lyrics))
                    {
                        ++alreadyCount;
                        AppendRecord(App.GetLocalizedString("RecordAlreadyHasLyric") + "\n");
                        continue;
                    }

                    var found = false;
                    foreach (var lyricHelper in _lyricHelpers)
                    {
                        try
                        {
                            var chineseSong = Text.ContainsChinese(performer) || Text.ContainsChinese(title);
                            if (chineseSong)
                            {
                                if (lyricHelper.Name == LyricHelperName.Azlyrics)
                                {
                                    continue;
                                }
                            }

                            var lyric = lyricHelper.GetLyric(title, performer);
                            if (lyric == null)
                            {
                                continue;
                            }
                            tag.Lyrics = lyric.ToString();
                            //AppendRecord(lyric.ToString());
                            found = true;
                            break;
                        }
                        catch (System.Net.WebException)
                        {
                            AppendRecord(App.GetLocalizedString("RecordCannotConnect") + "\n");
                        }
                    }

                    AppendRecord(App.GetLocalizedString(found ? "RecordFound" : "RecordNotFound") + "\n");
                    if (found)
                    {
                        ++foundCount;
                    }

                    songFile.Save();
                }
                finally
                {
                    songFile.Dispose();
                }

                if (TaskCanProceed())
                {
                    continue;
                }
                isHalted = true;
                break;
            }

            AppendRecord(isHalted ? "=== " + App.GetLocalizedString("RecordManuallyStopped") + " ===\n" : "=== " + App.GetLocalizedString("RecordFinished") + " ===\n");
            AppendRecord("=== " + App.GetLocalizedString("RecordToal") + " " + allCount + " " + App.GetLocalizedString("RecordAmongTotal") + " ");
            if (_addLyricForEmpty)
            {
                AppendRecord(alreadyCount + " " + App.GetLocalizedString("RecordSummaryAlready") + " ");
            }
            AppendRecord(foundCount + " " + App.GetLocalizedString("RecordSummarySuccessful") + " ===\n");
        }

        private static void AppendRecord(string title, string performer)
        {
            var perfromerToAppend = string.IsNullOrEmpty(performer) ? App.GetLocalizedString("RecordNoInfo") : performer;
            var titleToAppend = string.IsNullOrEmpty(title) ? App.GetLocalizedString("RecordNoInfo") : title;

            AppendRecord(perfromerToAppend + ": " + titleToAppend + "  ...  ");
        }

        private static void AppendRecord(string record)
        {
            Gui.MainWindow.Window.AppendRecord(record);
        }

        private static void SetStatus(string status)
        {
            Gui.MainWindow.Window.SetStatus(status);
        }
    }
}
