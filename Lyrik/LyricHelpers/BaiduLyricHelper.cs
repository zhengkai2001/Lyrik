using HtmlAgilityPack;
using Lyrik.Lyrics;
using Newtonsoft.Json.Linq;
using System;

namespace Lyrik.LyricSites
{
    class BaiduLyricHelper : LyricHelper
    {
        private const string baseUrl_search = @"http://music.baidu.com/search/lrc?key=";
        private const string baseUrl_lrc = @"http://music.baidu.com";

        public BaiduLyricHelper()
        {
        }

        protected override Lyric getLyricFromSite(string request)
        {
            string requestUrl = baseUrl_search + request;
            string htmlText = getHtmlString(requestUrl);

            return string.IsNullOrEmpty(htmlText) ? null : extractLyric(htmlText);
        }

        private Lyric extractLyric(string htmlText)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlText);

                HtmlNodeCollection songNodes = doc.DocumentNode.SelectNodes("//li[@class='clearfix bb']");
                foreach (HtmlNode songNode in songNodes)
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
                    HtmlNode lrcNode = songNode.SelectSingleNode("//span[@class='lyric-action']/a[contains(@class, \"down-lrc-btn\")]");
                    string lrcAddressRaw = lrcNode.GetAttributeValue("class", "");
                    int openingBrace = lrcAddressRaw.IndexOf('{');
                    int closingBrace = lrcAddressRaw.LastIndexOf('}');
                    string hrefJson = lrcAddressRaw.Substring(openingBrace, closingBrace + 1 - openingBrace);
                    JObject jo = JObject.Parse(hrefJson);
                    string href = jo.GetValue("href", StringComparison.CurrentCultureIgnoreCase).ToString();
                    string lrcAddress = baseUrl_lrc + href;

                    return Lyric.getLyricFromLRC(getHtmlString(lrcAddress)); ;
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
