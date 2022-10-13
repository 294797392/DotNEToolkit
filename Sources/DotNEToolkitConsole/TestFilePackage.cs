using DotNEToolkit;
using DotNEToolkit.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public class TestFilePackage
    {
        public static void WriteOnce()
        {
            List<byte> fileBytes = new List<byte>();

            List<string> fileList = Directory.EnumerateFiles(@"E:\44").ToList();
            foreach (string filePath in fileList)
            {
                fileBytes.AddRange(File.ReadAllBytes(filePath));
            }

            byte[] bytes = fileBytes.ToArray();

            DateTime start2 = DateTime.Now;
            using (FileStream FS = new FileStream("1.test", FileMode.Create, FileAccess.ReadWrite))
            {
                FS.Write(bytes, 0, bytes.Length);
            }
            Console.WriteLine("{0}ms", (DateTime.Now - start2).TotalMilliseconds);
        }

        public static void WriteMulit()
        {
            List<byte[]> fileBytes = new List<byte[]>();

            List<string> fileList = Directory.EnumerateFiles(@"E:\44").ToList();
            foreach (string filePath in fileList)
            {
                fileBytes.Add(File.ReadAllBytes(filePath));
            }

            DateTime start2 = DateTime.Now;
            using (FileStream FS = new FileStream("2.test", FileMode.Create, FileAccess.ReadWrite))
            {
                foreach (byte[] bytes in fileBytes)
                {
                    FS.Write(bytes, 0, bytes.Length);
                }
            }
            Console.WriteLine("{0}ms", (DateTime.Now - start2).TotalMilliseconds);
        }

        public static void PackDirectoryUseFileItem(string dir, string packagePath)
        {
            FilePackage package = FilePackage.Create(packagePath, FilePackages.TarArchive);

            List<string> fileList = Directory.EnumerateFiles(dir).ToList();

            List<FileItem> fileItems = new List<FileItem>();

            foreach (string filePath in fileList)
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);

                fileItems.Add(new FileItem()
                {
                    Name = filePath,
                    Offset = 0,
                    Size = fileBytes.Length,
                    Content = fileBytes
                });
            }

            package.PackFile(fileItems);
            package.Close();
        }

        public static void PackDirectory(string dir, string packagePath)
        {
            FilePackage package = FilePackage.Create(packagePath, FilePackages.TarArchive);
            package.PackDirectory(dir);
            package.Close();
        }

        public static void PackFile(string filePath, string packagePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            List<FileItem> fileItems = new List<FileItem>();
            fileItems.Add(new FileItem()
            {
                Name = filePath,
                PathRelativePackage = filePath,
                Size = fileBytes.Length,
                Content = fileBytes,
                Offset = 0
            });

            FilePackage package = FilePackage.Create(packagePath, FilePackages.Zip);
            package.Open();
            package.PackFile(fileItems);
            package.Close();
        }

        public static void PackFile(string filePath, string filePath2, string packagePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] fileBytes2 = File.ReadAllBytes(filePath2);

            List<FileItem> fileItems = new List<FileItem>();
            fileItems.Add(new FileItem()
            {
                Name = filePath,
                PathRelativePackage = filePath,
                Size = fileBytes.Length,
                Content = fileBytes,
                Offset = 0
            });
            fileItems.Add(new FileItem()
            {
                Name = filePath2,
                PathRelativePackage = filePath2,
                Content = fileBytes2,
                Size = fileBytes2.Length,
                Offset = 0
            });

            FilePackage package = FilePackage.Create(packagePath, FilePackages.TarArchive);
            package.PackFile(fileItems);
            package.Close();
        }


        public static void PackBinary()
        {
            FilePackage package = FilePackage.Create("test.tar", FilePackages.TarArchive);
            package.Open();

            byte[] content = File.ReadAllBytes("1.bmp");

            for (int i = 0; i < 10000; i++)
            {
                FileItem fileItem = new FileItem()
                {
                    Content = content,
                    Size = content.Length,
                    Offset = 0,
                    Name = i.ToString(),
                    PathRelativePackage = string.Format("test/{0}", i)
                };

                package.PackFile(new List<FileItem>() { fileItem });
            }

            package.Close();
        }

    }
}

