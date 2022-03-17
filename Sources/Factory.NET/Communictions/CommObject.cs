using DotNEToolkit;
using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Communictions
{
    /// <summary>
    /// 通信对象模型
    /// </summary>
    public abstract class CommObject : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("CommunicationObject");

        private const string KEY_NEWLINE = "newline";
        private const string DefaultNewLine = "\r\n";

        #endregion

        #region 实例变量

        /// <summary>
        /// 在读取或写入一行数据的时候使用的换行符
        /// </summary>
        protected string newline;

        #endregion

        #region ModuleBase

        public override int Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);

            this.newline = parameters.GetValue<string>(KEY_NEWLINE, DefaultNewLine);

            return DotNETCode.SUCCESS;
        }

        public override void Release()
        {
            base.Release();
        }

        #endregion

        #region 抽象接口

        /// <summary>
        /// 判断IO设备是否已经打开
        /// </summary>
        /// <returns></returns>
        public abstract bool IsOpened();

        /// <summary>
        /// 打开一个IO设备
        /// </summary>
        /// <returns></returns>
        public abstract int Open();

        /// <summary>
        /// 关闭IO设备
        /// </summary>
        /// <returns></returns>
        public abstract void Close();

        /// <summary>
        /// 从通信设备里读取一行数据
        /// </summary>
        /// <returns></returns>
        public abstract string ReadLine();

        /// <summary>
        /// 向通信设备里写入一行数据
        /// </summary>
        /// <param name="line"></param>
        public abstract void WriteLine(string line);

        /// <summary>
        /// 从通信设备里读取一段数据
        /// </summary>
        /// <param name="size">要读取的数据大小</param>
        /// <returns>读取的字节数</returns>
        public abstract byte[] ReadBytes(int size);

        /// <summary>
        /// 向通信设备里写入一段数据
        /// </summary>
        /// <param name="bytes"></param>
        public abstract void WriteBytes(byte[] bytes);

        #endregion

        #region 公开接口

        #endregion
    }
}
