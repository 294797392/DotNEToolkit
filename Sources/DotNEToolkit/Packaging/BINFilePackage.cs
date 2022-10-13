using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Packaging
{
    /// <summary>
    /// 把所有的数据打包成一个文件
    /// 并生成一个索引文件用来保存每个小文件的描述信息
    /// 
    /// Index文件格式：
    /// |-------------|
    /// | header size |   -> 4字节
    /// |-------------|
    /// |  file name  |   -> \0结尾, UTF8编码
    /// |-------------|
    /// 
    /// 
    /// 目录项格式：
    /// |-------------|
    /// |   offset    |   -> 8字节
    /// |-------------|
    /// |  file name  |   -> \0结尾, UTF8编码
    /// |-------------|
    /// 
    /// </summary>
    public class BINFilePackage : FilePackage
    {
        #region 实例变量

        private int fileCount;
        private FileStream fileStream;
        private FileStream indexStream;

        #endregion

        #region 属性

        public override FilePackages Type => FilePackages.Binary;

        /// <summary>
        /// 文件的总大小字节数
        /// 会预先在磁盘里分配Capacity大小的文件
        /// </summary>
        public long Capacity { get; set; }

        /// <summary>
        /// 索引文件的路径
        /// </summary>
        public string IndexPath { get; set; }

        #endregion

        #region 构造方法

        public BINFilePackage(string packagePath) :
            base(packagePath)
        {
        }

        #endregion

        #region 实例方法

        /// <summary>
        /// 创建一个新的索引文件
        /// </summary>
        /// <returns></returns>
        private FileStream CreateIndexFile() 
        {
            FileStream indexStream = new FileStream(this.IndexPath, FileMode.CreateNew, FileAccess.ReadWrite);

            // 写文件头
            throw new NotImplementedException();
        }

        #endregion

        #region FilePackage

        public override void Open()
        {
            this.fileStream = new FileStream(this.packagePath, FileMode.CreateNew, FileAccess.ReadWrite);
            this.fileStream.SetLength(this.Capacity);

            // 创建索引文件
            this.indexStream = this.CreateIndexFile();
        }

        public override void Close()
        {
            this.fileStream.Flush();
            this.fileStream.Close();
            this.fileStream.Dispose();

            this.indexStream.Flush();
            this.indexStream.Close();
            this.indexStream.Dispose();
        }

        public override void PackDirectory(List<DirectoryItem> dirList)
        {
            throw new NotImplementedException();
        }

        public override void PackDirectory(string baseDir)
        {
            throw new NotImplementedException();
        }

        public override void PackFile(List<FileItem> fileList)
        {
            foreach (FileItem fileItem in fileList)
            {

            }
        }

        #endregion
    }
}
