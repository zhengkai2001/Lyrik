using EasyHttp.Http;
using Lyrik.Lyrics;
using Lyrik.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Lyrik.LyricHelpers
{
    internal abstract class LyricHelper
    {
        public abstract LyricHelperName Name { get; }
        protected abstract string RequestUrl { get; set; }
        protected abstract Lyric GetLyric(string request);
        protected abstract Lyric GetLyric();

        private readonly HttpClient _http;

        protected LyricHelper()
        {
            _http = new HttpClient();
            _http.Request.Accept = HttpContentTypes.TextHtml;
        }

        protected string GetHtmlString(string url)
        {
            try
            {
                var response = _http.Get(url);
                return response.RawText;
            }
            catch (WebException)
            {
            }
            return "";
        }

        private static void AddTitleToAttempts(ICollection<string> requestAttempts, string title, string performer)
        {
            if (!string.IsNullOrEmpty(performer))
            {
                requestAttempts.Add(performer + " " + title);
            }

            requestAttempts.Add(title);
        }

        public Lyric GetLyric(string title, string performer)
        {
            var requestAttempts = new List<string>();

            //用于处理形如“Kwang Chiu (Cantonese Version) [Cantonese Version]”的歌名
            //将所有括号内的内容删去
            var simplifiedTitle = Text.DeleteSectionBetween(title, '(', ')');
            simplifiedTitle = Text.DeleteSectionBetween(simplifiedTitle, '（', '）');
            simplifiedTitle = Text.DeleteSectionBetween(simplifiedTitle, '[', ']');
            simplifiedTitle = Text.DeleteSectionBetween(simplifiedTitle, '【', '】');
            simplifiedTitle = simplifiedTitle.Trim();

            if (!requestAttempts.Contains(simplifiedTitle))
            {
                AddTitleToAttempts(requestAttempts, simplifiedTitle, performer);
            }

            if (!title.Equals(simplifiedTitle))
            {
                AddTitleToAttempts(requestAttempts, title, performer);
            }

            //foreach (var request in requestAttempts)
            //{
            //    Lyric lyric = GetLyricFromSite(request);
            //    if (lyric != null)
            //    {
            //        return lyric;
            //    }
            //}

            // LINQ
            // 下面一行的作用等同于上面八行
            return requestAttempts.Select(GetLyric).FirstOrDefault(lyric => lyric != null);
        }
    }
}
