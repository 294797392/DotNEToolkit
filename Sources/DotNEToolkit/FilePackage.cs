using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 存储一个要打包的文件信息
    /// </summary>
    public class FileItem
    {
        /// <summary>
        /// 文件名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件内容
        /// 使用Size字段指定内容的长度
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// 文件的真实长度
        /// 注意该值可以不等于Content的长度
        /// 写入Size个字节的数据
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 文件在Content里的偏移量
        /// </summary>
        public int Offset { get; set; }
    }

    /// <summary>
    /// 存储一个要打包的目录信息
    /// </summary>
    public class DirectoryItem
    {
        /// <summary>
        /// 带有反斜杠的路径
        /// SharpZipLib会把带有反斜杠结束的路径当成压缩包里的一个目录来处理
        /// 所以在把目录写入压缩包的时候路径要带反斜杠
        /// </summary>
        internal string BackslashPath
        {
            get
            {
                if (!this.Path.EndsWith("/"))
                {
                    return string.Format("{0}/", this.Path);
                }
                return this.Path;
            }
        }

        /// <summary>
        /// 目录名字
        /// 压缩包里的完整路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 该目录下的所有文件
        /// </summary>
        public List<FileItem> FileList { get; private set; }

        public DirectoryItem()
        {
            this.FileList = new List<FileItem>();
        }

        /// <summary>
        /// 计算目录的总大小
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            int size = 0;

            foreach (FileItem fileItem in this.FileList)
            {
                size += fileItem.Content.Length;
            }

            return size;
        }
    }

    /// <summary>
    /// 定义可以打包的文件类型
    /// </summary>
    public enum FilePackages
    {
        /// <summary>
        /// Zip压缩包格式的打包
        /// 后缀名.zip
        /// </summary>
        Zip,

        /// <summary>
        /// 不压缩，直接保存
        /// 当压缩很耗时的时候可以考虑此方法
        /// 后缀名.zip
        /// </summary>
        Stored
    }

    /// <summary>
    /// 将文件流或者目录打包成一个文件（包括但不限于压缩文件）保存
    /// 支持流式打包
    /// </summary>
    public abstract class FilePackage
    {
        /// <summary>
        /// 最终的压缩包的名字
        /// </summary>
        public string packageFilePath;

        /// <summary>
        /// 打包文件的类型
        /// </summary>
        public abstract FilePackages Type { get; }

        /// <summary>
        /// 创建一个压缩包
        /// </summary>
        /// <param name="package">要创建的压缩包类型</param>
        /// <returns></returns>
        public static FilePackage Create(string packagePath, FilePackages package)
        {
            switch (package)
            {
                case FilePackages.Zip: return new ZIPFilePackage() { packageFilePath = packagePath, CompressionMethod = CompressionMethod.Deflated };
                case FilePackages.Stored: return new ZIPFilePackage() { packageFilePath = packagePath, CompressionMethod = CompressionMethod.Stored };

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 读取一个本地目录并进行打包
        /// </summary>
        /// <param name="dir"></param>
        public abstract void PackDir(string dir);

        public abstract void PackDir(List<DirectoryItem> dirList);

        public abstract void PackFile(List<FileItem> fileList);

        /// <summary>
        /// 往一个已经存在的打包文件里追加目录列表
        /// </summary>
        /// <param name="packagePath">已经存在的打包文件完整路径</param>
        /// <param name="dirList">要追加到打包文件里的目录列表</param>
        public abstract void AppendDirectory(List<DirectoryItem> dirList);

        public void AppendDirectory(DirectoryItem dir)
        {
            this.AppendDirectory(new List<DirectoryItem>() { dir });
        }

        public abstract void AppendFile(List<FileItem> fileList);

        public void AppendFile(FileItem file)
        {
            this.AppendFile(new List<FileItem>() { file });
        }

        #region 实例方法

        /// <summary>
        /// 把一个内存流写到一个文件里
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="filePath"></param>
        protected void CreateFile(MemoryStream ms, string filePath)
        {
            byte[] buffer = ms.GetBuffer();

            // 写入文件
            using (FileStream fs = new FileStream(this.packageFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fs.Write(buffer, 0, (int)ms.Length);
            }
        }

        #endregion
    }

    /// <summary>
    /// 使用SharpZipLib库实现的文件打包器
    /// </summary>
    public class ZIPFilePackage : FilePackage
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

        #endregion

        #region 属性

        internal CompressionMethod CompressionMethod { get; set; }

        public override FilePackages Type => FilePackages.Zip;

        #endregion

        #region 构造方法

        public ZIPFilePackage()
        {
        }

        #endregion

        #region 实例方法

        private void PackDir(string parentDir, ZipOutputStream zipOutStream)
        {
            ZipEntry pdirEntry = new ZipEntry(parentDir);
            zipOutStream.PutNextEntry(pdirEntry);
        }

        #endregion

        #region FilePackage

        public override void PackDir(string dir)
        {
        }

        public override void PackDir(List<DirectoryItem> dirList)
        {
            /// <summary>
            /// 为了提高速度
            /// 使用内存来暂时保存压缩后的文件数据
            /// 最后一次性写入文件里
            /// </summary>

            using (MemoryStream baseStream = new MemoryStream())
            {
                using (ZipOutputStream zipOutStream = new ZipOutputStream(baseStream))
                {
                    foreach (DirectoryItem directory in dirList)
                    {
                        // 打包目录
                        ZipEntry dirEntry = new ZipEntry(directory.BackslashPath);
                        zipOutStream.PutNextEntry(dirEntry);

                        // 打包目录下的所有文件
                        foreach (FileItem file in directory.FileList)
                        {
                            // 压缩包里的文件完整路径
                            string fileFullPath = Path.Combine(directory.Path, file.Name);

                            ZipEntry fileEntry = new ZipEntry(fileFullPath) { CompressionMethod = this.CompressionMethod };
                            fileEntry.Size = file.Size;
                            zipOutStream.PutNextEntry(fileEntry);
                            zipOutStream.Write(file.Content, file.Offset, file.Size);  // 文件的Byte数组写入压缩流
                        }
                    }

                    zipOutStream.Finish();

                    // 到这里baseStream里就都是压缩后的数据了
                    byte[] zipBytes = baseStream.GetBuffer();

                    // 写入文件
                    this.CreateFile(baseStream, this.packageFilePath);
                }
            }
        }

        public override void PackFile(List<FileItem> fileList)
        {
            using (MemoryStream baseStream = new MemoryStream())
            {
                using (ZipOutputStream zipOutStream = new ZipOutputStream(baseStream))
                {
                    foreach (FileItem file in fileList)
                    {
                        ZipEntry fileEntry = new ZipEntry(file.Name) { CompressionMethod = this.CompressionMethod };
                        zipOutStream.PutNextEntry(fileEntry);
                        zipOutStream.Write(file.Content, file.Offset, file.Size);
                    }

                    zipOutStream.Finish();

                    // 到这里baseStream里就都是压缩后的数据了
                    byte[] zipBytes = baseStream.GetBuffer();

                    // 写入文件
                    this.CreateFile(baseStream, this.packageFilePath);
                }
            }
        }

        public override void AppendDirectory(List<DirectoryItem> dirList)
        {
            using (FileStream baseStream = File.Open(this.packageFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (ZipOutputStream zipOutStream = new ZipOutputStream(baseStream))
                {
                    foreach (DirectoryItem directory in dirList)
                    {
                        // 打包目录
                        ZipEntry dirEntry = new ZipEntry(directory.BackslashPath);
                        zipOutStream.PutNextEntry(dirEntry);

                        // 打包目录下的所有文件
                        foreach (FileItem file in directory.FileList)
                        {
                            // 压缩包里的文件完整路径
                            string fileFullPath = Path.Combine(directory.Path, file.Name);

                            ZipEntry fileEntry = new ZipEntry(fileFullPath) { CompressionMethod = this.CompressionMethod };
                            fileEntry.Size = file.Size;
                            zipOutStream.PutNextEntry(fileEntry);
                            zipOutStream.Write(file.Content, file.Offset, file.Size);  // 文件的Byte数组写入压缩流
                        }
                    }

                    zipOutStream.Finish();

                    // 保存写完的文件
                    baseStream.Flush();
                }
            }
        }

        public override void AppendFile(List<FileItem> fileList)
        {
            if (!File.Exists(this.packageFilePath))
            {
                this.PackFile(fileList);
            }
            else
            {
                // 开始更新
                using (ZipFile zipFile = new ZipFile(this.packageFilePath))
                {
                    zipFile.BeginUpdate();

                    // 输入更新数据
                    foreach (FileItem fileItem in fileList)
                    {
                        BufferedDataSource bds = new BufferedDataSource(fileItem);
                        zipFile.Add(bds, fileItem.Name, this.CompressionMethod);
                    }

                    zipFile.Close();
                    // 结束更新
                    zipFile.CommitUpdate();
                    zipFile.Close();
                }
            }
        }

        #endregion
    }
}

