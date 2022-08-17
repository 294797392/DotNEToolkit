using DotNEToolkit;
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
        private FilePackage package;

        public TestFilePackage(string packagePath)
        {
            this.package = FilePackage.Open(packagePath, FilePackages.Stored);
        }

        public void PackDirectory(string dir)
        {
            List<string> dirList = Directory.EnumerateDirectories(dir).ToList();

            List<DirectoryItem> dirItems = new List<DirectoryItem>();

            foreach (string dir1 in dirList)
            {
                DirectoryItem dirItem = new DirectoryItem()
                {
                    Path = Path.GetFileNameWithoutExtension(dir1),
                };

                List<string> fileList = Directory.EnumerateFiles(dir1).ToList();

                foreach (string file in fileList)
                {
                    byte[] fileBytes = File.ReadAllBytes(file);

                    dirItem.FileList.Add(new FileItem()
                    {
                        Name = Path.GetFileName(file),
                        Offset = 0,
                        Size = fileBytes.Length,
                        Content = fileBytes
                    });
                }

                dirItems.Add(dirItem);
            }

            this.package.AppendDirectory(dirItems);
            this.package.Close();
        }
    }
}

