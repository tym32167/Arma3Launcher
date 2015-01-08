using System;
using System.IO;
using Arma3LauncherWPF.Config;

namespace Arma3LauncherWPF.Core
{
    public class FileSystemHelper
    {
        public static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        /// <summary>
        /// Blocks until the file is not locked any more.
        /// </summary>
        /// <param name="fullPath"></param>
        public static bool DeleteDirectory(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    Directory.Delete(fullPath, true);
                    return true;
                }
                catch (Exception ex)
                {
                    if (numTries > 10)
                    {
                        return false;
                    }

                    // Wait for the lock to be released
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public FileSystemItem CheckPath(string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (!Directory.Exists(path)) return  FileSystemItem.Unknown;
                return FileSystemItem.Directory;
            }
            else
            {
                if (!File.Exists(path)) return  FileSystemItem.Unknown;
                return FileSystemItem.File;
            }
        }



        public static bool UnpackZipFile(string filename, string outFolder)
        {
            var commandArgs = string.Format(" x {0} -o{1} -y", filename, outFolder);

            var exe = new Executable(AppSettingsHelper.Arc7ZipPath);
            exe.StandardOutputFileName = AppSettingsHelper.Arc7ZipLogsPath;
            exe.Arguments = commandArgs;
            return exe.Run() == 0;
        }

        public enum FileSystemItem
        {
            File, Directory, Unknown
        }
    }
}