using System.Collections.Generic;
using System.IO;

namespace Lyrik.Utilities
{
    public sealed class FileOperations
    {
        private FileOperations() { }

        public static IList<FileInfo> Travel(string dir, string[] supportedFileTypes)
        {
            IList<FileInfo> fileList = new List<FileInfo>();

            DirectoryInfo dirDI = new DirectoryInfo(dir);
            if (dirDI.Exists)
            {
                Queue<DirectoryInfo> dirQueue = new Queue<DirectoryInfo>();
                dirQueue.Enqueue(dirDI);

                while (dirQueue.Count != 0)
                {
                    DirectoryInfo di = dirQueue.Dequeue();

                    foreach (DirectoryInfo d in di.GetDirectories())
                    {
                        dirQueue.Enqueue(d);
                    }
                    foreach (FileInfo f in di.GetFiles())
                    {
                        if(supportedFileTypes!=null)
                        {
                            foreach (string supportedFileType in supportedFileTypes)
                            {
                                if (f.Extension.ToUpperInvariant().Equals(supportedFileType))
                                {
                                    fileList.Add(f);
                                }
                            }
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
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
