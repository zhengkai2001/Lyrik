using Lyrik.Utilities;
using System.Linq;

namespace Lyrik.Lyrics
{
    internal class Lyric
    {
        //所有英文字母应标准化，写成大写
        private static readonly string[] Filters = {
            "制作:", "★", "【", "】","HTTP", "WWW.", ".COM", "匹配时间为", "EDIT:",
            "歌手:", "专辑:", "词 曲:", "XIAMI", "歌词整理：", "QQ:", "QQ :", "QQ：", "QQ ：",
            "演唱：" };

        private readonly string _lyricLine;

        public Lyric(string l)
        {
            _lyricLine = l;
        }

        public override string ToString()
        {
            return _lyricLine;
        }

        public static Lyric GetLyricFromLrc(string lrcString)
        {
            var seperators = new[] { '\n' };
            var lines = lrcString.Split(seperators);

            //var lyric = "";
            //foreach (var line in lines)
            //{
            //    var temp = line;
            //    temp = DeleteLrcInfo(temp);
            //    temp = DeleteRedundantInfo(temp);
            //    lyric += temp + '\n';
            //}
            //lyric = lyric.Trim();

            // LINQ
            // 下面一行的作用等同于上面九行
            var lyric = lines.Select(DeleteLrcInfo).Select(DeleteRedundantInfo).Aggregate("", (current, temp) => current + (temp + '\n')).Trim();

            return new Lyric(lyric);

            
        }

        private static string DeleteLrcInfo(string line)
        {
            return Text.DeleteSectionBetween(line, '[', ']');
        }

        private static string DeleteRedundantInfo(string line)
        {
            return ContainsRubbishInfo(line) ? "" : line;
        }

        private static bool ContainsRubbishInfo(string line)
        {
            //foreach (var filter in Filters)
            //{
            //    if (line.ToUpperInvariant().Contains(filter))
            //    {
            //        return true;
            //    }
            //}
            //return false;

            // LINQ
            // 下面一行的作用等同于上面七行
            return Filters.Any(filter => line.ToUpperInvariant().Contains(filter));
        }
    }
}
