using DotNEToolkit;
using DotNEToolkit.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Communictions
{
    /// <summary>
    /// 封装串口通信对象
    /// </summary>
    public class SerialPortCommObject : CommObject
    {
        private const string KEY_BAUDRATE = "BaudRate";
        private const string KEY_PORT = "Port";
        private const string KEY_DATABITS = "DataBits";
        private const string KEY_STOPBITS = "StopBits";

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("SerialPortCommObject");

        #endregion

        #region 实例变量

        private SerialPort serialPort;

        #endregion

        #region ModuleBase

        public override int Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);

            int baudRate = parameters.GetValue<int>(KEY_BAUDRATE);
            string portName = parameters.GetValue<string>(KEY_PORT);
            int dataBits = parameters.GetValue<int>(KEY_DATABITS, 8);
            StopBits stopBits = parameters.GetValue<StopBits>(KEY_STOPBITS, StopBits.One);

            this.serialPort = new SerialPort();
            this.serialPort.PortName = portName;
            this.serialPort.BaudRate = baudRate;
            this.serialPort.DataBits = dataBits;
            this.serialPort.StopBits = stopBits;
            this.serialPort.NewLine = this.newline;

            logger.InfoFormat("初始化串口成功, PortName = {0}, BaudRate = {1}, DataBits = {2}, StopBits = {3}", portName, baudRate, dataBits, stopBits);

            return DotNETCode.SUCCESS;
        }

        public override void Release()
        {
            this.serialPort.Dispose();
            this.serialPort = null;

            base.Release();
        }

        #endregion

        #region CommunicationObject

        public override int Open()
        {
            this.serialPort.Open();

            logger.InfoFormat("打开串口成功");

            return DotNETCode.SUCCESS;
        }

        public override void Close()
        {
            this.serialPort.Close();
        }

        public override bool IsOpened()
        {
            return this.serialPort != null && this.serialPort.IsOpen;
        }

        public override string ReadLine()
        {
            return this.serialPort.ReadLine();
        }

        public override void WriteLine(string line)
        {
            this.serialPort.WriteLine(line);
        }

        public override byte[] ReadBytes(int size)
        {
            return Streams.ReadFull(this.serialPort.Read, size);
        }

        public override void WriteBytes(byte[] bytes)
        {
            this.serialPort.Write(bytes, 0, bytes.Length);
        }

        #endregion
    }
}
