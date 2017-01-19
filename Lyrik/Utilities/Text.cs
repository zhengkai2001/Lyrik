using System;
using System.Text;

namespace Lyrik.Utilities
{
    internal class Text
    {
        private Text()
        {
        }

        //0.0.3新增
        //修正了某些标签中文读取为乱码的问题
        public static string ProcessString(string str)
        {
            var buffer = Encoding.Unicode.GetBytes(str);

            if (buffer.Length % 2 == 1)
            {
                return str;
            }

            for (var i = 1; i < buffer.Length; i += 2)
            {
                if (buffer[i] != 0)
                {
                    return str;
                }
            }

            var result = new byte[1];

            for (int i = 0, n = 0; i != buffer.Length; ++i)
            {
                if (buffer[i] == 0)
                {
                    continue;
                }

                Array.Resize(ref result, n + 1);
                result[n] = buffer[i];
                ++n;
            }

            var res = Encoding.Default.GetString(result);
            return res;
        }

        public static string DeleteSectionBetween(string line, char left, char right)
        {
            while (true)
            {
                var begin = line.IndexOf(left);
                var end = line.LastIndexOf(right);

                if (begin == -1 || end == -1)
                {
                    break;
                }

                if (begin >= end)
                {
                    continue;
                }

                if (begin == 0)
                {
                    line = end == line.Length - 1 ? "" : line.Substring(end + 1);
                }
                else
                {
                    if (end == line.Length - 1)
                    {
                        line = line.Substring(0, begin);
                    }
                    else
                    {
                        var part1 = line.Substring(0, begin);
                        var part2 = line.Substring(end + 1);
                        line = part1 + part2;
                    }
                }
            }
            return line;
        }
    }
}
