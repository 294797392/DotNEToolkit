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

        #endregion

        #region 属性

        /// <summary>
        /// 最终的压缩包的名字
        /// </summary>
        public string packagePath;

        /// <summary>
        /// 打包文件的类型
        /// </summary>
        public abstract FilePackages Type { get; }

        #endregion

        #region 构造方法

        internal FilePackage(string packagePath)
        {
            this.packagePath = packagePath;
        }

        #endregion

        /// <summary>
        /// 使用一个空流创建一个压缩包
        /// </summary>
        /// <param name="packagePath">压缩包的路径，如果没有这个压缩包文件，则会创建一个</param>
        /// <param name="package"></param>
        /// <returns></returns>
        public static FilePackage Open(string packagePath, FilePackages package)
        {
            FilePackage filePackage = null;

            switch (package)
            {
                case FilePackages.Zip:
                    {
                        filePackage = new ZIPFilePackage(packagePath) 
                        {
                            CompressionMethod = CompressionMethod.Deflated 
                        };
                        break;
                    } 

                case FilePackages.Stored:
                    {
                        filePackage = new ZIPFilePackage(packagePath)
                        {
                            CompressionMethod = CompressionMethod.Stored
                        };
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }

            filePackage.Open();

            return filePackage;
        }

        #region 抽象方法

        /// <summary>
        /// 创建一个空的压缩包
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// 关闭并保存打包文件
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 往一个已经存在的打包文件里追加目录列表
        /// </summary>
        /// <param name="dirList">要追加到打包文件里的目录列表</param>
        public abstract void PackDirectory(List<DirectoryItem> dirList);

        /// <summary>
        /// 往压缩包里打包文件
        /// </summary>
        /// <param name="fileList"></param>
        public abstract void PackFile(List<FileItem> fileList);

        /// <summary>
        /// 打包一个目录
        /// </summary>
        /// <param name="baseDir">要打包的目录</param>
        public abstract void PackDirectory(string baseDir);

        #endregion

        #region 公开接口

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
            using (FileStream fs = new FileStream(this.packagePath, FileMode.Create, FileAccess.Write, FileShare.Read))
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
            // 如果压缩包不存在，先创建一个空的压缩包
            if (!File.Exists(this.packagePath))
            {
                using (MemoryStream baseStream = new MemoryStream())
                {
                    using (ZipOutputStream zipOutStream = new ZipOutputStream(baseStream))
                    {
                        zipOutStream.Finish();

                        using (FileStream fs = new FileStream(this.packagePath, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(baseStream.GetBuffer(), 0, (int)baseStream.Length);
                        }
                    }
                }
            }

            // 打开一个ZipFile对象，使用ZipFile来更新Zip文件
            this.zipFile = new ZipFile(this.packagePath);
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

            foreach (string fileItem in fileList)
            {
                // 直接把文件完整路径传给SharpZipLib，它可以帮我们处理压缩包里的目录
                this.zipFile.Add(fileItem);
            }
        }

        #endregion
    }
}

