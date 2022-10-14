using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Packaging
{
    /// <summary>
    /// https://www.moon-soft.com/program/FORMAT/comm/tar.htm
    /// https://blog.csdn.net/qq_20553613/article/details/109892160#:~:text=2.2%20tar%E6%96%87%E4%BB%B6%E7%89%B9%E7%82%B9%201,%E4%BB%A5512%E5%AD%97%E8%8A%82%E4%B8%BA%E4%B8%80%E4%B8%AA%E8%AE%B0%E5%BD%95%E5%9D%97%E8%BF%9B%E8%A1%8C%E8%AE%B0%E5%BD%95%E6%95%B0%E6%8D%AE%EF%BC%8C%E4%B8%8D%E8%B6%B3%E4%B8%80%E4%B8%AA%E8%AE%B0%E5%BD%95%E5%9D%97%E7%9A%84%E4%BB%A50%E5%A1%AB%E5%85%85%202%20%E4%BB%A5%E2%80%9C%E6%96%87%E4%BB%B6%E5%A4%B4%2B%E6%9C%89%E6%95%88%E6%96%87%E4%BB%B6%E5%86%85%E5%AE%B9%E2%80%9D%E6%A0%BC%E5%BC%8F%E5%AD%98%E5%82%A8
    /// 一个空文件打包后为512*3字节，包括一个tar结构头，一个全零的block存储文件内容，一个全零的tar结构
    /// </summary>
    internal class TARFilePackage : FilePackage
    {
        private static readonly byte[] MagicBytes = new byte[] { (byte)'u', (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)' ' };

        #region 常量

        /// <summary>
        /// 默认的每次写入tar包的大小
        /// 默认是2M
        /// 这个值越大，磁盘的写入速度越高
        /// </summary>
        private const int DefaultWriteBufferSize = 1 * 1024 * 1024;

        #endregion

        #region 实例变量

        private FileStream tarFs;
        private TarOutputStream baseStream;

        #endregion

        #region 属性

        public override FilePackages Type => FilePackages.TarArchive;

        /// <summary>
        /// 每次写入文件的大小
        /// 单位字节
        /// </summary>
        public int WriteBufferSize { get; set; }

        #endregion

        #region 构造方法

        internal TARFilePackage(string packagePath) :
            base(packagePath)
        {
            this.WriteBufferSize = DefaultWriteBufferSize;
        }

        #endregion

        #region 实例方法

        private void WriteFile(FileItem fileItem)
        {
            //#region 写文件头

            //byte[] hdrBytes = new byte[512];

            //// 0x00：名称, 100字节
            //byte[] nameBytes = Encoding.ASCII.GetBytes(fileItem.Name);
            //Buffer.BlockCopy(nameBytes, 0, hdrBytes, 0, nameBytes.Length <= 100 ? nameBytes.Length : 100);

            //// 0x64：mode, 8字节

            //// 0x6C：uid, 8字节
            //hdrBytes[0x73] = 0;

            //// 0x74：gid, 8字节
            //hdrBytes[0x7B] = 0;

            //// 0x7C：大小, 12字节
            //byte[] sizeBytes = Encoding.ASCII.GetBytes((fileItem.Size.ToString() + " ").PadLeft(12, '0'));
            //sizeBytes[sizeBytes.Length - 1] = 0;
            //Buffer.BlockCopy(sizeBytes, 0, hdrBytes, 124, sizeBytes.Length);

            //// 0x88：mtime，12字节，存档时文件修改时间，以八进制ASCII码表示，含义是从1970年1月1日00:00起的协调世界时（UTC）所经过的秒数
            //byte[] mtimeBytes = Encoding.ASCII.GetBytes((DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + " ").PadLeft(12, '0'));
            //mtimeBytes[mtimeBytes.Length - 1] = 0;
            //Buffer.BlockCopy(mtimeBytes, 0, hdrBytes, 136, mtimeBytes.Length);

            ///***************************************************************************************************************************
            // * 0x9C：typeflag，1字节
            // * LF_NORMAL                '\0'                普通文件                            目前tar版本使用              
            // * LF_OLDNORMAL             '\0'                普通文件                            老版本tar使用
            // * LF_LINK                  '1'                 硬链接文件                          链接名由字段linkname指定，以’\0’为结尾符
            // * LF_SYMLINK               '2'                 软链接文件                          链接名由字段linkname指定，以’\0’为结尾符
            // * LF_CHR                   '3'                 字符设备文件                        字段devmajor存储主设备号；devminor存储子设备号
            // * LF_BLK                   '4'                 块设备文件                          字段devmajor存储主设备号；devminor存储子设备号
            // * LF_DIR                   '5'                 目录文件                            目录名由字段name指定，以’/'结尾；目录对应的磁盘分配是基于字段size，分配到最近的大小合适的磁盘块
            // * LF_FIFO                  '6'                 管道文件	                           管道文件的归档只保存文件记录，不保存文件内容
            // * LF_CONTIG                '7'                 连续文件                            物理磁盘空间存储是连续的文件；对于不支持连续文件的系统，该类文件视为普通文件
            // *****************************************************************************************************************************/
            //hdrBytes[156] = 0;

            //// 0x9D：linkname，100字节，链接文件名称


            //// 0x101：magic，6个字节，文件标识
            //hdrBytes[257] = (byte)'u';
            //hdrBytes[258] = (byte)'s';
            //hdrBytes[259] = (byte)'t';
            //hdrBytes[260] = (byte)'a';
            //hdrBytes[261] = (byte)'r';
            //hdrBytes[262] = 0;

            //// 最后写checksum
            //// 0x94：checksum，8字节
            //string checksum = ((nameBytes.Sum(v => (int)v) + sizeBytes.Sum(v => (int)v) + mtimeBytes.Sum(v => (int)v) + MagicBytes.Sum(v => (int)v) + 256).ToString() + " ").PadLeft(8, '0');
            //byte[] checksumBytes = Encoding.ASCII.GetBytes(checksum);
            //checksumBytes[checksumBytes.Length - 1] = 0;
            //Buffer.BlockCopy(checksumBytes, 0, hdrBytes, 148, checksumBytes.Length);

            //this.baseStream.Write(hdrBytes, 0, hdrBytes.Length);

            //#endregion

            //#region 写文件

            //this.baseStream.Write(fileItem.Content, fileItem.Offset, fileItem.Size);
            //int alignOffset = fileItem.Size % 512;
            //this.baseStream.Seek(alignOffset, SeekOrigin.Current);

            //#endregion

            TarEntry entry = TarEntry.CreateTarEntry(fileItem.PathRelativePackage);
            entry.Size = fileItem.Size;
            this.baseStream.PutNextEntry(entry);
            this.baseStream.Write(fileItem.Content, fileItem.Offset, fileItem.Size);
            this.baseStream.CloseEntry();
        }

        #endregion

        #region FilePackage

        public override void Open()
        {
            this.tarFs = new FileStream(this.packagePath, FileMode.CreateNew, FileAccess.ReadWrite);
            int blockFactor = this.WriteBufferSize / 512;
            this.baseStream = new TarOutputStream(this.tarFs, blockFactor, Encoding.Default);
        }

        public override void Close()
        {
            this.baseStream.Finish();
            this.baseStream.Close();
        }

        public override void PackDirectory(List<DirectoryItem> dirList)
        {
            throw new NotImplementedException();
        }

        public override void PackFile(List<FileItem> fileList)
        {
            foreach (FileItem fileItem in fileList)
            {
                this.WriteFile(fileItem);
            }
        }

        public override void PackDirectory(string baseDir)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
