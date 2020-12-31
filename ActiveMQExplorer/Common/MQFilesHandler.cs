using System;
using System.IO;
using System.Text;

namespace ActiveMQExplorer.Common
{
    public class MQFilesHandler
    {
        public static string DumpDirectory { get; set; }
        public static string FileExtention { get; set; }
        public static string SourceDirectory { get; set; }
        public static bool IsInDumpFilesMode { get; set; }

        public static string SaveDumpFile(string fileName, string content, string fileExtension)
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
                string targetFile = Path.Combine(DumpDirectory, $"{fileName}.{fileExtension}");

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

            return $"Successfully saved file: {fileName}.{fileExtension} at directory: {DumpDirectory}";
        }

        public static (bool isSuccess, string log, string fileContent) ReadFile(FileInfo fileInfo)
        {
            if(fileInfo == null)
                return (isSuccess: false, log: "Null FileInfo", fileContent: null);

            if (fileInfo.Exists == false)
                return (isSuccess: false, log: $"File: {fileInfo.FullName} not found!", fileContent: null);

            string fileContent = string.Empty;
            try
            {           
                using (StreamReader stream = fileInfo.OpenText())
                {
                    string line = string.Empty;
                    while ((line = stream.ReadLine()) != null)
                    {
                        fileContent += line + "\n";
                    }
                }
            }
            catch(Exception ex)
            {
                return (isSuccess: false, log: $"Error: {ex.Message}", fileContent);
            }   

            return (isSuccess: true, log: $"Successfully loaded content of file: {fileInfo.FullName}", fileContent);
        }
    }

    public enum FileExtension
    {
        txt = 1,
        xml = 2,
        json = 3,
        yaml = 4
    }
}
