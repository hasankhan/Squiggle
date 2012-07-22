using System;
using System.Diagnostics;
using System.IO;

namespace Squiggle.Utilities.Application
{
    public class Shell
    {
        public static bool CreateDirectoryIfNotExists(string path)
        {
            return ExceptionMonster.EatTheException(() =>
            {
                bool success = true;
                if (!Directory.Exists(path))
                {
                    DirectoryInfo dirInfo = Directory.GetParent(path);
                    if (dirInfo != null)
                        success = CreateDirectoryIfNotExists(dirInfo.FullName);
                    if (success)
                        Directory.CreateDirectory(path);
                }
                return success;
            }, "creating directory if it doesn't exist");            
        }

        public static void ShowInFolder(string filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "explorer.exe";
            startInfo.Arguments = "/select,\"" + filePath + "\"";

            Process.Start(startInfo);
        }

        public static void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
                Process.Start(new ProcessStartInfo(filePath));
        }

        public static void OpenUrl(string url)
        {
            ExceptionMonster.EatTheException(() => Process.Start(url), "opening the url:" + url);
        }

        public static string GetUniqueFilePath(string folderPath, string originalFileName)
        {
            string extension = System.IO.Path.GetExtension(originalFileName);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(originalFileName);

            string filePath = System.IO.Path.Combine(folderPath, originalFileName);
            for (int i = 1; File.Exists(filePath); i++)
            {
                string temp = String.Format("{0}({1}){2}", fileName, i, extension);
                filePath = System.IO.Path.Combine(folderPath, temp);
            }
            return filePath;
        }
    }
}
