using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static DotNEToolkit.ZIPFilePackage;

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

        /// <summary>
        /// 构造方法
        /// </summary>
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
        #region 实例变量

        /// <summary>
        /// 存储该打包文件的流
        /// </summary>
        protected Stream stream;

        #endregion

        #region 属性

        /// <summary>
        /// 最终的压缩包的名字
        /// </summary>
        public string packageFilePath;

        /// <summary>
        /// 打包文件的类型
        /// </summary>
        public abstract FilePackages Type { get; }

        #endregion

        #region 构造方法

        internal FilePackage(string filePackagePath, Stream stream)
        {
            this.packageFilePath = filePackagePath;
            this.stream = stream;
        }

        #endregion

        ///// <summary>
        ///// 使用一个文件创建一个压缩包
        ///// 或者打开一个现有的压缩包
        ///// </summary>
        ///// <param name="package">要创建的压缩包类型</param>
        ///// <returns></returns>
        //public static FilePackage Open(string packagePath, FilePackages package)
        //{
        //    FileStream stream = new FileStream(packagePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //    FilePackage filePackage = Open(stream, packagePath, package);
        //    filePackage.packageFilePath = packagePath;
        //    return filePackage;
        //}

        /// <summary>
        /// 使用一个流创建一个压缩包
        /// </summary>
        /// <param name="stream">要从流创建的压缩包</param>
        /// <param name="package">要创建的压缩包类型</param>
        /// <param name="packagePath">压缩包类型</param>
        /// <returns></returns>
        public static FilePackage Open(Stream stream, string packagePath, FilePackages package)
        {
            switch (package)
            {
                case FilePackages.Zip: return new ZIPFilePackage(packagePath, stream) { CompressionMethod = CompressionMethod.Deflated };
                case FilePackages.Stored: return new ZIPFilePackage(packagePath, stream) { CompressionMethod = CompressionMethod.Stored };

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 使用一个空流创建一个压缩包
        /// </summary>
        /// <param name="packagePath">压缩包的路径，如果没有这个压缩包文件，则会创建一个</param>
        /// <param name="package"></param>
        /// <returns></returns>
        public static FilePackage Open(string packagePath, FilePackages package)
        {
            MemoryStream ms = new MemoryStream();
            return Open(ms, packagePath, package);
        }

        #region 抽象方法

        /// <summary>
        /// 关闭并保存打包文件
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 往一个已经存在的打包文件里追加目录列表
        /// </summary>
        /// <param name="dirList">要追加到打包文件里的目录列表</param>
        public abstract void AppendDirectory(List<DirectoryItem> dirList);

        /// <summary>
        /// 往压缩包里打包文件
        /// </summary>
        /// <param name="fileList"></param>
        public abstract void AppendFile(List<FileItem> fileList);

        #endregion

        #region 公开接口

        public void AppendDirectory(DirectoryItem dir)
        {
            this.AppendDirectory(new List<DirectoryItem>() { dir });
        }

        public void AppendFile(FileItem file)
        {
            this.AppendFile(new List<FileItem>() { file });
        }

        #endregion

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

        private ZipOutputStream zipOutStream;

        #endregion

        #region 属性

        internal CompressionMethod CompressionMethod { get; set; }

        public override FilePackages Type => FilePackages.Zip;

        #endregion

        #region 构造方法

        internal ZIPFilePackage(string packagePath, Stream stream) :
            base(packagePath, stream)
        {
            this.zipOutStream = new ZipOutputStream(stream);
        }

        #endregion

        #region 实例方法

        #endregion

        #region FilePackage

        public override void Close()
        {
            this.zipOutStream.Finish();
            using (FileStream fs = new FileStream(this.packageFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                MemoryStream ms = this.stream as MemoryStream;
                fs.Write(ms.GetBuffer(), 0, (int)ms.Position);
            }

            this.zipOutStream.Close();
            this.zipOutStream.Dispose();
            this.stream.Close();
            this.stream.Dispose();
        }

        public override void AppendDirectory(List<DirectoryItem> dirList)
        {
            foreach (DirectoryItem directory in dirList)
            {
                // 打包目录
                ZipEntry dirEntry = new ZipEntry(directory.BackslashPath);
                this.zipOutStream.PutNextEntry(dirEntry);

                // 打包目录下的所有文件
                foreach (FileItem file in directory.FileList)
                {
                    // 压缩包里的文件完整路径
                    string fileFullPath = Path.Combine(directory.Path, file.Name);

                    ZipEntry fileEntry = new ZipEntry(fileFullPath) { CompressionMethod = this.CompressionMethod };
                    fileEntry.Size = file.Size;
                    this.zipOutStream.PutNextEntry(fileEntry);
                    this.zipOutStream.Write(file.Content, file.Offset, file.Size);  // 文件的Byte数组写入压缩流
                }
            }
        }

        public override void AppendFile(List<FileItem> fileList)
        {
            // 打包目录下的所有文件
            foreach (FileItem file in fileList)
            {
                // 压缩包里的文件完整路径

                ZipEntry fileEntry = new ZipEntry(file.Name) { CompressionMethod = this.CompressionMethod };
                fileEntry.Size = file.Size;
                this.zipOutStream.PutNextEntry(fileEntry);
                this.zipOutStream.Write(file.Content, file.Offset, file.Size);  // 文件的Byte数组写入压缩流
            }
        }

        #endregion
    }

    /// <summary>
    /// 当更新压缩包的时候使用这个类
    /// </summary>
    internal class ZIPFilePackageUpdate : FilePackage
    {
        #region 实例变量

        private ZipFile zipFile;

        #endregion

        #region 属性

        internal CompressionMethod CompressionMethod { get; set; }

        public override FilePackages Type => FilePackages.Zip;

        #endregion

        #region 构造方法

        internal ZIPFilePackageUpdate(string packagePath, Stream stream) :
            base(packagePath, stream)
        {
            this.zipFile = new ZipFile(stream);
            this.zipFile.BeginUpdate();
        }

        #endregion

        #region 实例方法

        #endregion

        #region FilePackage

        public override void AppendDirectory(List<DirectoryItem> dirList)
        {
            foreach (DirectoryItem dirItem in dirList)
            {
                this.zipFile.AddDirectory(dirItem.BackslashPath);

                foreach (FileItem fileItem in dirItem.FileList)
                {
                    BufferedDataSource bds = new BufferedDataSource(fileItem);
                    zipFile.Add(bds, fileItem.Name, this.CompressionMethod);
                }
            }
        }

        public override void AppendFile(List<FileItem> fileList)
        {
            foreach (FileItem fileItem in fileList)
            {
                BufferedDataSource bds = new BufferedDataSource(fileItem);
                zipFile.Add(bds, fileItem.Name, this.CompressionMethod);
            }
        }

        public override void Close()
        {
            this.zipFile.CommitUpdate();
            this.zipFile.Close();
        }

        #endregion
    }
}

