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
            FilePackage package = FilePackage.Open(packagePath, FilePackages.Stored);

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
            FilePackage package = FilePackage.Open(packagePath, FilePackages.Stored);
            package.PackDirectory(dir);
            package.Close();
        }
    }
}

