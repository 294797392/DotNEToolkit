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
        private const string KEY_NEWLINE = "NewLine";

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

            int baudRate = this.InputParameters.GetValue<int>(KEY_BAUDRATE);
            string portName = this.InputParameters.GetValue<string>(KEY_PORT);
            int dataBits = this.InputParameters.GetValue<int>(KEY_DATABITS, 8);
            StopBits stopBits = this.InputParameters.GetValue<StopBits>(KEY_STOPBITS, StopBits.One);
            string newLine = this.InputParameters.GetValue<string>(KEY_NEWLINE, Environment.NewLine);

            this.serialPort = new SerialPort();
            this.serialPort.PortName = portName;
            this.serialPort.BaudRate = baudRate;
            this.serialPort.DataBits = dataBits;
            this.serialPort.StopBits = stopBits;
            this.serialPort.NewLine = newLine;

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

        protected override int ReadBytes(byte[] bytes, int offset, int count)
        {
            return this.serialPort.Read(bytes, offset, count);
        }

        protected override void WriteBytes(byte[] bytes, int offset, int count)
        {
            this.serialPort.Write(bytes, offset, count);
        }

        #endregion
    }
}
