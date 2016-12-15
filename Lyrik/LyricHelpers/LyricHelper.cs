using EasyHttp.Http;
using Lyrik.Lyrics;
using Lyrik.Utilities;
using System.Collections.Generic;

namespace Lyrik.LyricSites
{
    abstract class LyricHelper
    {
        private HttpClient http;

        protected LyricHelper()
        {
            http = new HttpClient();
            http.Request.Accept = HttpContentTypes.TextHtml;
        }

        protected string getHtmlString(string url)
        {
            HttpResponse response = http.Get(url);
            return response.RawText;
        }

        private static void addTitle2Attempts(List<string> requestAttempts, string title, string performer)
        {
            if (!string.IsNullOrEmpty(performer))
            {
                requestAttempts.Add(performer + " " + title);
            }

            requestAttempts.Add(title);
        }

        public Lyric getLyric(string title, string performer)
        {
            List<string> requestAttempts = new List<string>();
            
            //用于处理形如“Kwang Chiu (Cantonese Version) [Cantonese Version]”的歌名
            //将所有括号内的内容删去
            string simplifiedTitle = Text.deleteSectionBetween(title, '(', ')');
            simplifiedTitle = Text.deleteSectionBetween(simplifiedTitle, '（', '）');
            simplifiedTitle = Text.deleteSectionBetween(simplifiedTitle, '[', ']');
            simplifiedTitle = Text.deleteSectionBetween(simplifiedTitle, '【', '】');
            simplifiedTitle = simplifiedTitle.Trim();

            if (!requestAttempts.Contains(simplifiedTitle))
            {
                addTitle2Attempts(requestAttempts, simplifiedTitle, performer);
            }

            addTitle2Attempts(requestAttempts, title, performer);

            foreach (string request in requestAttempts)
            {
                Lyric lyric = getLyricFromSite(request);
                if (lyric == null)
                {
                    continue;
                }
                else
                {
                    return lyric;
                }
            }

            return null;
        }

        protected abstract Lyric getLyricFromSite(string request);
    }
}
