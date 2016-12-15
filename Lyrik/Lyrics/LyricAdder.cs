using Lyrik.LyricSites;
using Lyrik.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Lyrik.Lyrics
{
    class LyricAdder : BasicTask
    {
        private static string[] supportedFileTypes = { ".MP3", ".M4A" };

        private List<LyricHelper> lyricHelpers;

        private string songDir;
        private bool addLyricForEmpty;

        //private string skipString = "=====Lyrik Skip=====";

        private TextBox statusTextBox;
        private Label statusLabel;
        private string statusLabelAdding;

        public LyricAdder()
        {
            lyricHelpers = new List<LyricHelper>();
            lyricHelpers.Add(new BaiduLyricHelper());

            TagLib.Id3v2.Tag.DefaultEncoding = TagLib.StringType.UTF8;
            TagLib.Id3v2.Tag.ForceDefaultEncoding = true;
        }

        public void setStatusTextBox(TextBox statusTextBox2)
        {
            this.statusTextBox = statusTextBox2;
        }

        public void setStatusLabel(Label statusLabel2, string statusLabelAdding2)
        {
            this.statusLabel = statusLabel2;
            this.statusLabelAdding = statusLabelAdding2;
        }

        public void setSongDir(string songDir2)
        {
            this.songDir = songDir2;
        }

        public void setAddLyricRules(bool addLyricForEmpty2)
        {
            this.addLyricForEmpty = addLyricForEmpty2;
        }

        public void addLyric()
        {
            bool isHalted = false;
            taskBegin();

            appendStatus("=== 开始添加歌词 ===\n");

            int foundCount = 0;
            int allCount = 0;
            int alreadyCount = 0;

            IList<FileInfo> fileList = FileOperations.Travel(songDir, supportedFileTypes);

            foreach (FileInfo fi in fileList)
            {
                ++allCount;

                statusLabel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() {
                    statusLabel.Content = statusLabelAdding + " (" + allCount + "/" + fileList.Count + ")";
                });

                if (FileOperations.IsFileLocked(fi))
                {
                    appendStatus(fi.Name + " 该文件正在使用中，无法读取，跳过。\n");
                    continue;
                }

                TagLib.File songFile = null;
                try
                {
                    songFile = TagLib.File.Create(fi.FullName, TagLib.ReadStyle.Average);

                    //TagLib.Id3v2.Tag tag = songFile.GetTag(TagLib.TagTypes.Id3v2, true) as TagLib.Id3v2.Tag;
                    TagLib.Tag tag = songFile.GetTag(TagLib.TagTypes.Id3v2, true) as TagLib.Id3v2.Tag;
                    if (tag == null)
                    {
                        tag = songFile.GetTag(TagLib.TagTypes.Id3v1, true) as TagLib.Id3v1.Tag;
                    }

                    if (tag == null)
                    {
                        appendStatus(fi.Name + " 该文件的标签可能存在问题，无法读取，跳过。\n");
                        continue;
                    }

                    string title = tag.Title == null ? fi.Name.Replace(fi.Extension, "") : tag.Title;
                    string performer = tag.Performers.Length == 0 ? "" : tag.Performers[0];
                    
                    title = Text.processString(title);
                    performer = Text.processString(performer);

                    //if (abnormalTagText)
                    //{
                    //    //appendStatus("abnormal~" + title + " " + performer);

                    //    //songFile.RemoveTags(TagLib.TagTypes.Id3v1);

                    //    //TagLib.Tag newTag = new TagLib.Id3v2.Tag();
                    //    //newTag.Title = title;
                    //    //newTag.Performers = new string[1];
                    //    //newTag.Performers[0] = performer;
                    //    //newTag.Year = tag.Year;

                    //    //songFile.TagTypes = newTag;

                    //    //tag.Clear();
                    //}

                    appendStatus(title, performer);

                    if (tag.Genres.Length != 0)
                    {
                        string genre = tag.Genres[0];
                        if (genre.Equals("classical", StringComparison.OrdinalIgnoreCase) || genre.Equals("classic", StringComparison.OrdinalIgnoreCase))
                        {
                            appendStatus("这好像是古典音乐，因此跳过！\n");
                            continue;
                        }
                    }

                    string lyrics = tag.Lyrics;

                    //if (!string.IsNullOrEmpty(lyrics) && lyrics.Contains(skipString))
                    //{
                    //    appendStatus("这首歌的歌词中包含了让Lyriks跳过它的指令~因此跳过！\n");
                    //    continue;
                    //}

                    //如果仅为尚未包含歌词的歌曲添加歌词，那么要先检查一下歌词是否为空
                    //如果不为空，则跳过
                    if (addLyricForEmpty && !string.IsNullOrEmpty(lyrics))
                    {
                        ++alreadyCount;
                        appendStatus("这首歌已经有歌词了，因此跳过！\n");
                        continue;
                    }

                    Boolean found = false;
                    foreach (LyricHelper lyricHelper in lyricHelpers)
                    {
                        try
                        {
                            Lyric lyric = lyricHelper.getLyric(title, performer);
                            if (lyric != null)
                            {
                                tag.Lyrics = lyric.ToString();
                                //appendStatus(lyric.ToString());
                                found = true;
                                break;
                            }
                        }
                        catch (System.Net.WebException)
                        {
                            appendStatus("未能连接到歌词站点。你的网络连接似乎有些问题……\n");
                        }
                    }

                    appendStatus(found ? "找到了！\n" : "没找到~\n");
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
                    taskProceed();
                }
                catch(OperationCanceledException)
                {
                    isHalted = true;
                    break;
                }
            }

            appendStatus(isHalted ? "=== 已被手动停止 ===\n" : "=== 完成！ ===\n");
            appendStatus("=== 本次共处理 " + allCount + " 首歌，其中");
            if (addLyricForEmpty)
            {
                appendStatus("原先已包含歌词的有 " + alreadyCount + " 首，");
            }
            appendStatus("成功添加歌词的有 " + foundCount + " 首 ===\n");
        }

        private void appendStatus(string title, string performer)
        {
            string perfromerToAppend = string.IsNullOrEmpty(performer) ? "(信息缺失)" : performer;
            string titleToAppend = string.IsNullOrEmpty(title) ? "(信息缺失)" : title;

            appendStatus(perfromerToAppend + ": " + titleToAppend + "  ...  ");
        }

        private void appendStatus(string status)
        {
            statusTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() {
                statusTextBox.AppendText(status);
                statusTextBox.ScrollToEnd();
            });
        }
    }
}
