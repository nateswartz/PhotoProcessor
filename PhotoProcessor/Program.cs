using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PhotoProcessor
{
    class Program
    {

        private static ISet<string> _unknownFileTypes = new HashSet<string>();
        private static int _errorCount = 0;
        static void Main(string[] args)
        {
            Console.Write("Please enter root directory to process: ");
            var rootDir = @"C:/PhotosToProcess/Ready";//Console.ReadLine();
            Console.Write("Please enter target directory: ");
            var targetDir = @"C:/ProcessedPhotos";// Console.ReadLine();
            ProcessDirectory(rootDir, targetDir);

            foreach (var extension in _unknownFileTypes)
                Console.WriteLine($"Encountered unknown extension: {extension}");

            Console.WriteLine($"Encountered {_errorCount} errors.");
        }

        static void ProcessDirectory(string directory, string targetDirectory)
        {
            var subDirectories = Directory.GetDirectories(directory);
            foreach (var dir in subDirectories)
            {
                Console.WriteLine($"Processing directory: {dir}");
                ProcessDirectory(dir, targetDirectory);
            }
            var files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                Console.WriteLine($"Processing file: {file}");
                ProcessFile(file, targetDirectory);
            }
        }

        static void ProcessFile(string filePath, string targetDirectory)
        {
            var path = Path.GetDirectoryName(filePath);
            var originalFileName = Path.GetFileName(filePath);
            var newFileName = originalFileName;
            var year = Regex.Match(Path.GetDirectoryName(filePath).Split(Path.DirectorySeparatorChar).Last(), @"\d+").Value;
            var type = "";
            try
            {
                string prefix = "";
                if (IsImageFile(originalFileName))
                {
                    prefix = GetDateTakenFromImage(filePath);
                    type = "Image";
                }
                else if (IsVideoFile(originalFileName))
                {
                    Console.WriteLine("Processing video file");
                    type = "Video";
                }
                else
                {
                    _unknownFileTypes.Add(Path.GetExtension(originalFileName));
                    //Console.WriteLine("Unknown File!!");
                    return;
                }

                if (!string.IsNullOrEmpty(prefix))
                {
                    newFileName = $"{prefix}_{newFileName}";
                }

                Directory.CreateDirectory(Path.Combine(targetDirectory, type, year));
                File.Copy(Path.Combine(path, originalFileName), Path.Combine(targetDirectory, type, year, newFileName));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to process: {filePath}");
                _errorCount++;
            }
        }

        //we init this once so that if the function is repeatedly called
        //it isn't stressing the garbage man
        private static Regex r = new Regex(":");

        //retrieves the datetime WITHOUT loading the whole image
        public static string GetDateTakenFromImage(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (Image myImage = Image.FromStream(fs, false, false))
                {
                    PropertyItem propItem = myImage.GetPropertyItem(36867);
                    string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "_", 2).Split(' ')[0];
                    return dateTaken;
                }
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static bool IsVideoFile(string fileName)
        {
            var videoExtensions = new List<string> { ".mp4", ".mov" };
            return videoExtensions.Contains(Path.GetExtension(fileName).ToLower());
        }

        public static bool IsImageFile(string fileName)
        {
            var imageExtensions = new List<string> { ".jpg", ".png" };
            return imageExtensions.Contains(Path.GetExtension(fileName).ToLower());
        }
    }
}
