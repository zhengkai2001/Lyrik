using System;
using System.Text;

namespace Lyrik.Utilities
{
    class Text
    {
        private Text()
        {
        }

        //0.0.3新增
        //修正了某些标签中文读取为乱码的问题
        public static string processString(string str)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(str);

            if (buffer.Length % 2 == 1)
            {
                return str;
            }

            for (int i = 1; i < buffer.Length; i += 2)
            {
                if (buffer[i] != 0)
                {
                    return str;
                }
            }

            byte[] result = new byte[1];

            for (int i = 0, n = 0; i != buffer.Length; ++i)
            {
                if (buffer[i] != 0)
                {
                    Array.Resize<byte>(ref result, n + 1);
                    result[n] = buffer[i];
                    ++n;
                }
            }

            string res = Encoding.Default.GetString(result);
            return res;
        }

        public static string deleteSectionBetween(string line, char left, char right)
        {
            while (true)
            {
                int begin = line.IndexOf(left);
                int end = line.LastIndexOf(right);

                if (begin == -1 || end == -1)
                {
                    break;
                }
                else if (begin < end)
                {
                    if (begin == 0)
                    {
                        if (end == line.Length - 1)
                        {
                            line = "";
                        }
                        else
                        {
                            line = line.Substring(end + 1);
                        }
                    }
                    else
                    {
                        if (end == line.Length - 1)
                        {
                            line = line.Substring(0, begin);
                        }
                        else
                        {
                            String part1 = line.Substring(0, begin);
                            String part2 = line.Substring(end + 1);
                            line = part1 + part2;
                        }
                    }
                }
            }
            return line;
        }
    }
}
