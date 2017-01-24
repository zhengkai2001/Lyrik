using HtmlAgilityPack;
using Lyrik.Lyrics;
using Newtonsoft.Json.Linq;
using System;

namespace Lyrik.LyricHelpers
{
    internal class BaiduLyricHelper : LyricHelper
    {
        private const string BaseUrlSearch = @"http://music.baidu.com/search/lrc?key=";
        private const string BaseUrlLyric = @"http://music.baidu.com";

        public override LyricHelperName Name => LyricHelperName.Baidu;

        protected override string RequestUrl { get; set; }

        protected override Lyric GetLyric(string request)
        {
            RequestUrl = BaseUrlSearch + request;
            return GetLyric();
        }

        protected override Lyric GetLyric()
        {
            try
            {
                var doc = new HtmlDocument();

                var lyricPage = GetHtmlString(RequestUrl);
                doc.LoadHtml(lyricPage);
                var songNodes = doc.DocumentNode.SelectNodes("//li[@class='clearfix bb']");

                foreach (var songNode in songNodes)
                {
                    //从网页中提取出标题和表演者
                    //HtmlNode titleNode = songNode.SelectSingleNode("//span[@class='song-title']/a");
                    //string title = titleNode.GetAttributeValue("title", "");

                    //HtmlNode artistNode = songNode.SelectSingleNode("//span[@class='artist-title']/span");
                    //string artist = artistNode.GetAttributeValue("title", "");

                    //如果标题和表演者都与搜索的字段相同，则说明找到了对应的歌曲，下载歌词并返回
                    //if (this.title.Equals(title) && this.performer.Equals(artist))
                    //{
                    //从网页中提取出lrc的下载地址
                    //HtmlNodeCollection lrcNodes = songNode.SelectNodes("//span[@class='lyric-action'][contains(a, \"下载\")]/a");

                    //直接下载搜索结果中的第一份歌词
                    var lrcNode = songNode.SelectSingleNode("//span[@class='lyric-action']/a[contains(@class, \"down-lrc-btn\")]");
                    var lrcAddressRaw = lrcNode.GetAttributeValue("class", "");
                    var openingBrace = lrcAddressRaw.IndexOf('{');
                    var closingBrace = lrcAddressRaw.LastIndexOf('}');
                    var hrefJson = lrcAddressRaw.Substring(openingBrace, closingBrace + 1 - openingBrace);
                    var hrefJsonObject = JObject.Parse(hrefJson);
                    var href = hrefJsonObject.GetValue("href", StringComparison.CurrentCultureIgnoreCase).ToString();
                    var lrcAddress = BaseUrlLyric + href;

                    return Lyric.GetLyricFromLrc(GetHtmlString(lrcAddress));
                    //}
                }
            }
            catch (NullReferenceException)
            {
                //若解析html失败
            }

            return null;
        }
    }
}
