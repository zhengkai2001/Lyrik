using System;
using Lyrik.Lyrics;
using HtmlAgilityPack;
using Lyrik.Utilities;

namespace Lyrik.LyricHelpers
{
    internal class AzlyricsLyricHelper : LyricHelper
    {
        private const string BaseUrlSearch = @"http://search.azlyrics.com/search.php?q=";

        public override LyricHelperName Name => LyricHelperName.Azlyrics;

        protected override string RequestUrl { get; set; }

        protected override Lyric GetLyric(string request)
        {
            RequestUrl = BaseUrlSearch + request.Replace(' ', '+');
            return GetLyric();
        }

        protected override Lyric GetLyric()
        {
            try
            {
                var doc = new HtmlDocument();

                var lyricSearchResultPage = GetHtmlString(RequestUrl);
                doc.LoadHtml(lyricSearchResultPage);
                var songNode = doc.DocumentNode.SelectSingleNode("//td[@class='text-left visitedlyr']/a");
                var lrcAddress = songNode.GetAttributeValue("href", "");

                var lyricPage = GetHtmlString(lrcAddress);
                doc.LoadHtml(lyricPage);
                var brNodes = doc.DocumentNode.SelectNodes("//br");
                var lyricDivNode = brNodes[1].NextSibling.NextSibling;

                var lyricRaw = lyricDivNode.InnerHtml;
                var lyricText = lyricRaw.Replace("\r\n", "");
                lyricText = lyricText.Replace("<br>\n", "\n");
                lyricText = Text.DeleteSectionBetween(lyricText, "<!--", "-->");

                return Lyric.GetLyricFromLrc(lyricText);
            }
            catch (NullReferenceException)
            {
                //若解析html失败
            }

            return null;
        }
    }
}
