using Lyrik.Utilities;
using System;

namespace Lyrik.Lyrics
{
    class Lyric
    {
        //所有英文字母应标准化，写成大写
        private static string[] filters = new String[] {
            "制作:", "★", "【", "】","HTTP", "WWW.", ".COM", "匹配时间为", "EDIT:",
            "歌手:", "专辑:", "词 曲:", "XIAMI", "歌词整理：", "QQ:", "QQ :", "QQ：", "QQ ：",
            "演唱：" };

        private string lyricLine;

        public Lyric(string l)
        {
            lyricLine = l;
        }

        public override string ToString()
        {
            return lyricLine;
        }

        public static Lyric getLyricFromLRC(string lrcString)
        {
            char[] seperators = new char[] { '\n' };
            string[] lines = lrcString.Split(seperators);
            string lyric = "";

            foreach (string line in lines)
            {
                string temp = line;
                temp = deleteLRCInfo(temp);
                temp = deleteRedundantInfo(temp);

                lyric += temp + '\n';
            }

            lyric = lyric.Trim();

            return new Lyric(lyric);
        }

        private static String deleteLRCInfo(String line)
        {
            return Text.deleteSectionBetween(line, '[', ']');
        }

       
        private static string deleteRedundantInfo(string line)
        {
            if (containsRubbishInfo(line))
            {
                return "";
            }
            else
            {
                return line;
            }
        }

        private static bool containsRubbishInfo(string line)
        {
            foreach (string filter in filters)
            {
                if (line.ToUpperInvariant().Contains(filter))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
