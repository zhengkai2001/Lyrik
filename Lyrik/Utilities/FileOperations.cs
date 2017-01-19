using System.Collections.Generic;
using System.IO;

namespace Lyrik.Utilities
{
    public static class FileOperations
    {
        public static IList<FileInfo> Travel(string dir, string[] supportedFileTypes)
        {
            IList<FileInfo> fileList = new List<FileInfo>();

            var dirDi = new DirectoryInfo(dir);
            if (!dirDi.Exists)
            {
                return fileList;
            }

            var dirQueue = new Queue<DirectoryInfo>();
            dirQueue.Enqueue(dirDi);

            while (dirQueue.Count != 0)
            {
                var di = dirQueue.Dequeue();

                foreach (var d in di.GetDirectories())
                {
                    dirQueue.Enqueue(d);
                }
                foreach (var f in di.GetFiles())
                {
                    if (supportedFileTypes == null)
                    {
                        continue;
                    }

                    foreach (var supportedFileType in supportedFileTypes)
                    {
                        if (f.Extension.ToUpperInvariant().Equals(supportedFileType))
                        {
                            fileList.Add(f);
                        }
                    }
                }
            }

            return fileList;
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                if(file != null)
                {
                    stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another _thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }
    }
}
