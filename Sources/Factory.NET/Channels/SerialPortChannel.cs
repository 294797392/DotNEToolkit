using DotNEToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime;

namespace Factory.NET.Channels
{
    /// <summary>
    /// 修改历史
    /// 20210304: 
    ///     1. 在读写串口之前加入串口是否打开的检测
    ///     2. 读写失败后，对InvalidOperationException做关闭串口的处理
    /// </summary>
    public class SerialPortChannel : ChannelBase
    {
        private const string DefaultCOMPort = "COM3";
        private const int DefaultBaudRate = 115200;
        private const int DefaultDataBits = 8;
        private const StopBits DefaultStopBits = StopBits.One;

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("SerialPortIODriver");

        #endregion

        #region 实例变量

        private SerialPort port;

        private string portName;
        private int baudRate;
        private int dataBits;
        private StopBits stopBits;

        #endregion

        #region 属性

        public override ChannelTypes Type { get { return ChannelTypes.SerialPort; } }

        #endregion

        #region 构造方法

        public SerialPortChannel()
        {
        }

        #endregion

        #region AbstractIODriver

        protected override int OnInitialize()
        {
            this.portName = this.GetParameter<string>("PortName", DefaultCOMPort);
            this.baudRate = this.GetParameter<int>("BaudRate", DefaultBaudRate);
            this.dataBits = this.GetParameter<int>("DataBits", DefaultDataBits);
            this.stopBits = this.GetParameter<StopBits>("StopBits", DefaultStopBits);
            logger.DebugFormat("串口信息：Port:{0}, baudRate:{1}", this.portName, this.baudRate);

            try
            {
                this.port = new SerialPort(this.portName);
                this.port.BaudRate = this.baudRate;
                this.port.DataBits = this.dataBits;
                this.port.StopBits = this.stopBits;
                this.port.Parity = Parity.None;
                if (!string.IsNullOrEmpty(base.newLine))
                {
                    this.port.NewLine = base.newLine;
                }
                this.port.ReadTimeout = this.readTimeout;
                this.port.ReadBufferSize = base.readBufferSize;
                this.port.WriteBufferSize = base.writeBufferSize;
                this.port.Open();

                logger.DebugFormat("串口连接成功, {0}, {1}, ReadTimeout = {2}", this.portName, this.baudRate, this.port.ReadTimeout);
                return ResponseCode.SUCCESS;
            }
            catch (System.InvalidOperationException ex)
            {
                logger.ErrorFormat("串口连接失败, {0}被占用, {1}", this.portName, ex);
                return ResponseCode.IODRV_OPEN_FAILED;
            }
            catch (Exception ex)
            {
                // 尝试重连
                logger.Error(string.Format("串口连接异常, portName = {0}", this.portName), ex);
                return ResponseCode.IODRV_RECONNECT;
            }
        }

        protected override void OnRelease()
        {
            if (this.port != null)
            {
                try
                {
                    if (this.port.IsOpen)
                    {
                        this.port.Close();
                    }
                    this.port.Dispose();
                    this.port = null;
                }
                catch (Exception ex)
                {
                    logger.Error("关闭串口异常", ex);
                }
            }
        }

        public override int ReadBytes(byte[] bytes, int offset, int len)
        {
            if (!this.port.IsOpen)
            {
                return ResponseCode.IODRV_NOT_OPENED;
            }

            return this.port.Read(bytes, offset, len);
        }

        public override string ReadLine()
        {
            return this.port.ReadLine();
        }

        public override byte[] ReadBytesFull(int size)
        {
            return StreamUtils.ReadFull(this.port.BaseStream, size);
        }

        public override void WriteBytes(byte[] buffer)
        {
            this.port.Write(buffer, 0, buffer.Length);
        }

        public override void WriteLine(string cmd)
        {
            this.port.WriteLine(cmd);
            logger.DebugFormat("向串口发送数据:{0}", cmd);
        }

        public override void ClearExisting()
        {
            if (!this.port.IsOpen)
            {
                return;
            }

            if (this.port.BytesToRead > 0)
            {
                string data = this.port.ReadExisting();
                //logger.DebugFormat("ClearReceiveBuffer:{0}", data);
            }
        }

        #endregion
    }
}