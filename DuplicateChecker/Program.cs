using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuplicateChecker
{
    class Program
    {

        static readonly List<(FileData, FileData)> duplicateFiles = new List<(FileData, FileData)>();

        static void Main(string[] args)
        {
            Console.Write("Please enter directory to check for duplicates: ");
            var dir = @"E:/Pictures/2020";
            //var dir = @"C:/tmp/Photos";//Console.ReadLine();
            CheckForDuplicates(dir);
            Console.WriteLine("Finished checking for duplicates");
            foreach (var duplicate in duplicateFiles)
            {
                Console.WriteLine("-------------------");
                Console.WriteLine(Path.Combine(duplicate.Item1.Path, duplicate.Item1.Name));
                Console.WriteLine(Path.Combine(duplicate.Item2.Path, duplicate.Item2.Name));
            }
            Console.WriteLine($"Found {duplicateFiles.Count} duplicates");
        }

        private static void CheckForDuplicates(string dir)
        {
            var files = GetAllFiles(dir);
            for (var i = 0; i < files.Count();)
            {
                for (var j = 0; j < files.Count(); j++)
                {
                    // An item is always equal to itself
                    if (i != j)
                    {
                        if (FilesMatch(files[i], files[j]))
                        {
                            PromptToDeleteDuplicate(files[i], files[j]);
                            duplicateFiles.Add((files[i], files[j]));
                            break;
                        }
                    }
                }
                files.RemoveAt(i);
            }
        }

        private static int PromptToDeleteDuplicate(FileData file1, FileData file2)
        {
            var file1FullPath = Path.Combine(file1.Path, file1.Name);
            var file2FullPath = Path.Combine(file2.Path, file2.Name);
            Console.WriteLine("Found duplicates");
            Console.WriteLine($"1: {file1FullPath}");
            Console.WriteLine($"2: {file2FullPath}");
            Console.WriteLine("Pick a file to delete (1/2):");
            var toDelete = Console.ReadLine();
            if (toDelete == "1")
            {
                File.Delete(file1FullPath);
                return 1;
            }
            else if (toDelete == "2")
            {
                File.Delete(file2FullPath);
                return 2;
            }
            return 0;
        }

        private static bool FilesMatch(FileData file1, FileData file2)
        {
            if (file1.Extension.ToLower() == file2.Extension.ToLower()
                && file1.Size == file2.Size
                && NamesMatch(file1.Name, file2.Name))
            {
                return true;
            }
            return false;
        }

        private static bool NamesMatch(string name1, string name2)
        {
            if (name1.Contains(name2)
                || name2.Contains(name1)
                || name1.Replace("-", "_") == name2.Replace("-", "_"))
            {
                return true;
            }
            return false;
        }

        private static List<FileData> GetAllFiles(string directory)
        {
            var files = new List<FileData>();
            var subDirectories = Directory.GetDirectories(directory);
            foreach (var dir in subDirectories)
            {
                var subDirFiles = GetAllFiles(dir);
                files = files.Concat(subDirFiles).ToList();
            }
            foreach (var file in Directory.GetFiles(directory))
            {
                var info = new FileInfo(file);
                var data = new FileData
                {
                    Name = Path.GetFileName(file),
                    Extension = Path.GetExtension(file),
                    Path = Path.GetDirectoryName(file),
                    Size = info.Length
                };
                files.Add(data);
            }
            return files;
        }

        class FileData
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public long Size { get; set; }
            public string Extension { get; set; }
        }
    }
}
