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
    /// 一个空文件打包后为512*3字节，包括一个tar结构头，一个全零的block存储文件内容，一个全零的tar结构
    /// </summary>
    //internal class TARFilePackage : FilePackage
    //{
    //    #region 实例变量

    //    private TarOutputStream tarStream;

    //    #endregion

    //    #region 属性

    //    public override FilePackages Type => FilePackages.TarArchive;

    //    #endregion

    //    #region 构造方法

    //    internal TARFilePackage(string packagePath) :
    //        base(packagePath)
    //    {

    //    }

    //    #endregion

    //    #region 实例方法

    //    private void WriteHeader()
    //    {

    //    }

    //    #endregion

    //    #region FilePackage

    //    public override void Open()
    //    {
    //        FileStream packageStream = null;

    //        // 如果压缩包不存在，先创建一个空的压缩包
    //        if (!File.Exists(this.packagePath))
    //        {
    //            using (MemoryStream baseStream = new MemoryStream())
    //            {
    //                using (TarOutputStream zipOutStream = new TarOutputStream(baseStream, Encoding.Default))
    //                {
    //                    zipOutStream.Finish();

    //                    packageStream = new FileStream(this.packagePath, FileMode.Create, FileAccess.ReadWrite);
    //                    packageStream.Write(baseStream.GetBuffer(), 0, (int)baseStream.Length);
    //                    packageStream.Seek(0, SeekOrigin.Begin);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            // TODO：判断
    //            packageStream = new FileStream(this.packagePath, FileMode.Open, FileAccess.ReadWrite);
    //            //packageStream.Seek()
    //        }

    //        this.baseStream = packageStream;
    //    }

    //    public override void Close()
    //    {
    //        this.tarArchive.Close();
    //    }

    //    public override void PackDirectory(List<DirectoryItem> dirList)
    //    {
    //    }

    //    public override void PackFile(List<FileItem> fileList)
    //    {
    //        foreach (FileItem fileItem in fileList)
    //        {
    //            TarEntry entry = TarEntry.CreateTarEntry(fileItem.Name);
    //            this.tarArchive.WriteEntry(entry, false);
    //        }
    //    }

    //    public override void PackDirectory(string baseDir)
    //    {
    //    }

    //    #endregion
    //}
}
