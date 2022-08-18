using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Packaging
{
    /// <summary>
    /// 使用SharpZipLib库实现的文件打包器
    /// </summary>
    internal class ZIPFilePackage : FilePackage
    {
        public class BufferedDataSource : IStaticDataSource
        {
            private FileItem fileItem;

            public BufferedDataSource(FileItem fileItem)
            {
                this.fileItem = fileItem;
            }

            public Stream GetSource()
            {
                MemoryStream ms = new MemoryStream(this.fileItem.Content, this.fileItem.Offset, this.fileItem.Size);
                return ms;
            }
        }

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ZIPFilePackage");

        #endregion

        #region 实例变量

        private ZipFile zipFile;

        #endregion

        #region 属性

        internal CompressionMethod CompressionMethod { get; set; }

        public override FilePackages Type => FilePackages.Zip;

        #endregion

        #region 构造方法

        internal ZIPFilePackage(string packagePath) :
            base(packagePath)
        {
        }

        #endregion

        #region 实例方法

        #endregion

        #region FilePackage

        public override void Open()
        {
            FileStream packageStream = null;

            // 如果压缩包不存在，先创建一个空的压缩包
            if (!File.Exists(this.packagePath))
            {
                using (MemoryStream baseStream = new MemoryStream())
                {
                    using (ZipOutputStream zipOutStream = new ZipOutputStream(baseStream))
                    {
                        zipOutStream.Finish();

                        packageStream = new FileStream(this.packagePath, FileMode.Create, FileAccess.ReadWrite);
                        packageStream.Write(baseStream.GetBuffer(), 0, (int)baseStream.Length);
                        packageStream.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
            else
            {
                packageStream = new FileStream(this.packagePath, FileMode.Open, FileAccess.ReadWrite);
            }

            // 打开一个ZipFile对象，使用ZipFile来更新Zip文件
            this.zipFile = new ZipFile(packageStream);
            this.zipFile.BeginUpdate();
        }

        public override void Close()
        {
            this.zipFile.CommitUpdate();
            this.zipFile.Close();
        }

        public override void PackDirectory(List<DirectoryItem> dirList)
        {
            foreach (DirectoryItem dirItem in dirList)
            {
                this.zipFile.AddDirectory(dirItem.BackslashPath);

                foreach (FileItem fileItem in dirItem.FileList)
                {
                    BufferedDataSource bds = new BufferedDataSource(fileItem);
                    this.zipFile.Add(bds, fileItem.Name, this.CompressionMethod);
                }
            }
        }

        public override void PackFile(List<FileItem> fileList)
        {
            // 打包目录下的所有文件
            foreach (FileItem fileItem in fileList)
            {
                BufferedDataSource bds = new BufferedDataSource(fileItem);
                this.zipFile.Add(bds, fileItem.Name, this.CompressionMethod);
            }
        }

        public override void PackDirectory(string baseDir)
        {
            IEnumerable<string> fileList = Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories);

            foreach (string filePath in fileList)
            {
                byte[] bytes = File.ReadAllBytes(filePath);

                FileItem fileItem = new FileItem()
                {
                    Name = filePath,
                    Content = bytes,
                    Size = bytes.Length,
                    Offset = 0
                };

                BufferedDataSource bds = new BufferedDataSource(fileItem);
                this.zipFile.Add(bds, fileItem.Name, this.CompressionMethod);
            }
        }

        #endregion
    }
}
