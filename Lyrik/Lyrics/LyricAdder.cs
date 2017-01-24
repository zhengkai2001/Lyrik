using Lyrik.LyricHelpers;
using Lyrik.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using TagLib;
using TextBox = System.Windows.Controls.TextBox;

namespace Lyrik.Lyrics
{
    internal class LyricAdder : BasicTask
    {
        private static readonly string[] SupportedFileTypes = { ".MP3", ".M4A" };

        private readonly List<LyricHelper> _lyricHelpers;

        private string _songDir;
        private bool _addLyricForEmpty;

        //private string skipString = "=====Lyrik Skip=====";

        private TextBox _statusTextBox;
        private Label _statusLabel;
        private string _statusLabelAdding;

        public LyricAdder()
        {
            _lyricHelpers = new List<LyricHelper>
            {
                new AzlyricsLyricHelper(),
                new BaiduLyricHelper()
            };

            TagLib.Id3v2.Tag.DefaultEncoding = StringType.UTF8;
            TagLib.Id3v2.Tag.ForceDefaultEncoding = true;
        }

        public void SetStatusTextBox(TextBox statusTextBox2)
        {
            _statusTextBox = statusTextBox2;
        }

        public void SetStatusLabel(Label statusLabel2, string statusLabelAdding2)
        {
            _statusLabel = statusLabel2;
            _statusLabelAdding = statusLabelAdding2;
        }

        public void SetSongDir(string songDir2)
        {
            _songDir = songDir2;
        }

        public void SetAddLyricRules(bool addLyricForEmpty2)
        {
            _addLyricForEmpty = addLyricForEmpty2;
        }

        public void AddLyric()
        {
            var isHalted = false;
            TaskBegin();

            AppendStatus("=== 开始添加歌词 ===\n");

            var foundCount = 0;
            var allCount = 0;
            var alreadyCount = 0;

            var fileList = FileOperations.Travel(_songDir, SupportedFileTypes);

            foreach (var fi in fileList)
            {
                ++allCount;

                var count = allCount;
                _statusLabel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    _statusLabel.Content = _statusLabelAdding + " (" + count + "/" + fileList.Count + ")";
                });

                if (FileOperations.IsFileLocked(fi))
                {
                    AppendStatus(fi.Name + " 该文件正在使用中，无法读取，跳过。\n");
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
                        AppendStatus(fi.Name + " 该文件的标签可能存在问题，无法读取，跳过。\n");
                        continue;
                    }

                    var title = tag.Title ?? fi.Name.Replace(fi.Extension, "");
                    var performer = tag.Performers.Length == 0 ? "" : tag.Performers[0];

                    title = Text.ProcessString(title);
                    performer = Text.ProcessString(performer);

                    //if (abnormalTagText)
                    //{
                    //    //AppendStatus("abnormal~" + title + " " + performer);

                    //    //songFile.RemoveTags(TagLib.TagTypes.Id3v1);

                    //    //TagLib.Tag newTag = new TagLib.Id3v2.Tag();
                    //    //newTag.Title = title;
                    //    //newTag.Performers = new string[1];
                    //    //newTag.Performers[0] = performer;
                    //    //newTag.Year = tag.Year;

                    //    //songFile.TagTypes = newTag;

                    //    //tag.Clear();
                    //}

                    AppendStatus(title, performer);

                    if (tag.Genres.Length != 0)
                    {
                        var genre = tag.Genres[0];
                        if (genre.Equals("classical", StringComparison.OrdinalIgnoreCase) || genre.Equals("classic", StringComparison.OrdinalIgnoreCase))
                        {
                            AppendStatus("这好像是古典音乐，因此跳过！\n");
                            continue;
                        }
                    }

                    var lyrics = tag.Lyrics;

                    //if (!string.IsNullOrEmpty(lyrics) && lyrics.Contains(skipString))
                    //{
                    //    AppendStatus("这首歌的歌词中包含了让Lyriks跳过它的指令~因此跳过！\n");
                    //    continue;
                    //}

                    //如果仅为尚未包含歌词的歌曲添加歌词，那么要先检查一下歌词是否为空
                    //如果不为空，则跳过
                    if (_addLyricForEmpty && !string.IsNullOrEmpty(lyrics))
                    {
                        ++alreadyCount;
                        AppendStatus("这首歌已经有歌词了，因此跳过！\n");
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
                            //AppendStatus(lyric.ToString());
                            found = true;
                            break;
                        }
                        catch (System.Net.WebException)
                        {
                            AppendStatus("未能连接到歌词站点。你的网络连接似乎有些问题……\n");
                        }
                    }

                    AppendStatus(found ? "找到了！\n" : "没找到~\n");
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

                try
                {
                    TaskProceed();
                }
                catch (OperationCanceledException)
                {
                    isHalted = true;
                    break;
                }
            }

            AppendStatus(isHalted ? "=== 已被手动停止 ===\n" : "=== 完成！ ===\n");
            AppendStatus("=== 本次共处理 " + allCount + " 首歌，其中");
            if (_addLyricForEmpty)
            {
                AppendStatus("原先已包含歌词的有 " + alreadyCount + " 首，");
            }
            AppendStatus("成功添加歌词的有 " + foundCount + " 首 ===\n");
        }

        private void AppendStatus(string title, string performer)
        {
            var perfromerToAppend = string.IsNullOrEmpty(performer) ? "(信息缺失)" : performer;
            var titleToAppend = string.IsNullOrEmpty(title) ? "(信息缺失)" : title;

            AppendStatus(perfromerToAppend + ": " + titleToAppend + "  ...  ");
        }

        private void AppendStatus(string status)
        {
            _statusTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
            {
                _statusTextBox.AppendText(status);
                _statusTextBox.ScrollToEnd();
            });
        }
    }
}
