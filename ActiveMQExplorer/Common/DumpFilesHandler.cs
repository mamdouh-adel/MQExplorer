using System;
using System.IO;
using System.Text;

namespace ActiveMQExplorer.Common
{
    public class DumpFilesHandler
    {
        public static string DumpDirectory { get; set; }
        public static bool IsInDumpFilesMode { get; set; }


        public static string SaveDumpFile(string fileName, string content)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "Null/Empty file name!";

            if (string.IsNullOrWhiteSpace(content))
                return "Null/Empty file content!";

            if (IsInDumpFilesMode == false || string.IsNullOrWhiteSpace(DumpDirectory))
                return "An error occurred, please check the dump directory";

            if (Directory.Exists(DumpDirectory) == false)
                return $"The dump directory: {DumpDirectory} not found!";

            fileName = fileName.Replace(":", "_");

            try
            {
                string targetFile = Path.Combine(DumpDirectory, fileName + ".txt");

                FileInfo fi = new FileInfo(targetFile);

                using (FileStream fs = fi.Create())
                {
                    byte[] msgContent = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(msgContent, 0, msgContent.Length);
                }
            }
            catch (Exception ex)
            {
                return "Error while saving dump file: " + ex.Message;
            }

            return $"Successfully saved file: {fileName}.txt at directory: {DumpDirectory}";
        }
    }
}
