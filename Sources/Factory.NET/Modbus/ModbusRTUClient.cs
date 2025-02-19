using DotNEToolkit.Bindings;
using DotNEToolkit.Crypto;
using DotNEToolkit.Modular;
using DotNEToolkit.Utility;
using Factory.NET.Channels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Factory.NET.Modbus
{
    /// <summary>
    /// AddressCode(1字节) FunctionCode(1字节) Data(4字节，Address + Value) CRC(2字节)
    /// 
    /// Modbus名词解释：https://blog.csdn.net/lingshi75/article/details/105991450
    /// Modbus协议：https://blog.csdn.net/qq_36339249/article/details/90664839
    /// 
    /// RTU：远程终端控制系统
    /// 
    /// 
    /// 简单点说，modbus有四种数据，DI、DO、AI、AO
    /// DI: 数字输入，离散输入，一个地址一个数据位，用户只能读取它的状态，不能修改。比如面板上的按键、开关状态，电机的故障状态。
    /// DO: 数字输出，线圈输出，一个地址一个数据位，用户可以置位、复位，可以回读状态，比如继电器输出，电机的启停控制信号。
    /// AI: 模拟输入，输入寄存器，一个地址16位数据，用户只能读，不能修改，比如一个电压值的读数。
    /// AO: 模拟输出，保持寄存器，一个地址16位数据，用户可以写，也可以回读，比如一个控制变频器的电流值。
    /// 无论这些东西被叫做什么名字，其内容不外乎这几种，输入的信号用户只能看不能改，输出的信号用户控制，并可以回读。离散的数据只有一位，模拟的数据有16位。
    /// </summary>
    public class ModbusRTUClient : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModbusRTUClient");

        private const byte FUNCTION_CODE_READ_DO = 0x01;
        private const byte FUNCTION_CODE_READ_DI = 0x02;
        private const byte FUNCTION_CODE_READ_AO = 0x03;
        private const byte FUNCTION_CODE_READ_AI = 0x04;

        private const byte FUNCTION_CODE_WRITE_DO = 0x05;
        private const byte FUNCTION_CODE_WRITE_DO_MULIT = 0x0F;

        private const byte FUNCTION_CODE_WRITE_AO = 0x06;
        private const byte FUNCTION_CODE_WRITE_AO_MULIT = 0x10;

        #endregion

        #region 实例变量

        /// <summary>
        /// 与PLC通信的对象
        /// </summary>
        private ChannelBase channel;

        #endregion

        #region 属性

        /// <summary>
        /// Modbus设备的地址码，这个地址码一般使用厂商提供的软件去修改
        /// </summary>
        [BindableProperty(1)]
        public byte AddressCode { get; private set; }

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            this.channel = ChannelFactory.Create(this.InputParameters);
            this.channel.Initialize(this.InputParameters);

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
            this.channel.Release();
        }

        #endregion

        #region 数字输入

        /// <summary>
        /// 读取一路线圈输入寄存器的值
        /// </summary>
        /// <param name="address">要读取的线圈寄存器的地址</param>
        /// <param name="value">读取到寄存器的数据</param>
        /// <returns></returns>
        public bool ReadDI(byte address, out byte value)
        {
            return this.ReadDigtalIO(address, FUNCTION_CODE_READ_DI, out value);
        }

        #endregion

        #region 数字输出

        public bool ReadDO(ushort address, out byte value)
        {
            return this.ReadDigtalIO(FUNCTION_CODE_READ_DO, address, out value);
        }

        public bool WriteDO(ushort address, ushort value)
        {
            return this.WriteDigtalOutput(FUNCTION_CODE_WRITE_DO, address, value);
        }

        #endregion

        #region 模拟输出

        public bool ReadAO(byte address, short numReg, out List<short> value)
        {
            return this.ReadAnalogIO(address, FUNCTION_CODE_READ_AO, numReg, out value);
        }

        public bool WriteAO(byte address, short value)
        {
            return this.WriteAnalogOutput(FUNCTION_CODE_WRITE_AO, address, value);
        }

        #endregion

        #region 模拟输入

        public bool ReadAI(byte address, short numReg, out List<short> value)
        {
            return this.ReadAnalogIO(address, FUNCTION_CODE_READ_AI, numReg, out value);
        }

        #endregion

        #region 实例方法

        /// <summary>
        /// 读取数字量
        /// </summary>
        /// <param name="address"></param>
        /// <param name="fcode"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool ReadDigtalIO(byte fcode, ushort address, out byte value)
        {
            value = 0;

            byte[] buffer = new byte[4];
            byte[] addressByte = this.ReverseBytes(BitConverter.GetBytes(address));
            byte[] numBytes = this.ReverseBytes(BitConverter.GetBytes((short)8));                  // 一次性读取8路
            Buffer.BlockCopy(addressByte, 0, buffer, 0, addressByte.Length);
            Buffer.BlockCopy(numBytes, 0, buffer, 2, numBytes.Length);

            byte[] data = this.PackData(this.AddressCode, fcode, buffer);
            this.channel.WriteBytes(data);

            byte[] result = this.channel.ReadBytesFull(6);

            if (result[0] != this.AddressCode || result[1] != fcode || result[2] != 1)
            {
                logger.ErrorFormat("收到的Mosbus数据格式不正确");
                return false;
            }

            value = result[3];

            return true;
        }

        /// <summary>
        /// 写数字输出寄存器
        /// </summary>
        /// <param name="address">要写的寄存器地址</param>
        /// <param name="fcode">功能代码</param>
        /// <param name="value">寄存器的值</param>
        /// <returns></returns>
        private bool WriteDigtalOutput(byte fcode, ushort address, ushort value)
        {
            byte[] buffer = new byte[4];
            byte[] addressByte = this.ReverseBytes(BitConverter.GetBytes(address));
            byte[] valueBytes = this.ReverseBytes(BitConverter.GetBytes(value));
            Buffer.BlockCopy(addressByte, 0, buffer, 0, addressByte.Length);
            Buffer.BlockCopy(valueBytes, 0, buffer, 2, valueBytes.Length);

            byte[] data = this.PackData(this.AddressCode, fcode, buffer);
            this.channel.WriteBytes(data);

            byte[] result = this.channel.ReadBytesFull(8);

            if (!ByteUtils.Compare(result, data))
            {
                logger.ErrorFormat("WriteDigtalOutput失败, 返回的数据和发送的数据不一致");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 读取单路模拟量
        /// </summary>
        /// <param name="address">要读取的数据的地址</param>
        /// <param name="fcode">功能码</param>
        /// <param name="numReg">要读取的寄存器的数量</param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool ReadAnalogIO(byte address, byte fcode, short numReg, out List<short> values)
        {
            values = null;

            byte[] buffer = new byte[4];
            byte[] addressByte = this.ReverseBytes(BitConverter.GetBytes(address));
            byte[] numBytes = this.ReverseBytes(BitConverter.GetBytes(numReg));                  // 要读取的寄存器的数量，一个寄存器是两个字节
            Buffer.BlockCopy(addressByte, 0, buffer, 0, addressByte.Length);
            Buffer.BlockCopy(numBytes, 0, buffer, 2, numBytes.Length);

            byte[] data = this.PackData(this.AddressCode, fcode, buffer);
            this.channel.WriteBytes(data);

            int valueBytes = numReg * 2;    // 寄存器的值所占用的字节数
            byte[] result = this.channel.ReadBytesFull(3 + valueBytes + 2);

            // result[2]是字节数，参考MODBUS_Communication_Protocol_Chinese_Version# MODBUS通讯协议中文版.pdf, 16页
            if (result[0] != this.AddressCode || result[1] != fcode || result[2] != valueBytes)
            {
                logger.ErrorFormat("收到的Mosbus数据格式不正确");
                return false;
            }

            values = new List<short>();

            for (int i = 0; i < valueBytes; i += 2)
            {
                byte[] vbs = new byte[2] { result[3 + i + 1], result[3 + i] };
                short v = BitConverter.ToInt16(vbs, 0);
                values.Add(v);
            }

            return true;
        }

        private bool WriteAnalogOutput(byte fcode, short address, short value)
        {
            byte[] buffer = new byte[4];
            byte[] addressByte = this.ReverseBytes(BitConverter.GetBytes(address));
            byte[] valueBytes = this.ReverseBytes(BitConverter.GetBytes(value));
            Buffer.BlockCopy(addressByte, 0, buffer, 0, addressByte.Length);
            Buffer.BlockCopy(valueBytes, 0, buffer, 2, valueBytes.Length);

            byte[] data = this.PackData(this.AddressCode, fcode, buffer);
            this.channel.WriteBytes(data);

            byte[] result = this.channel.ReadBytesFull(8);

            if (!ByteUtils.Compare(result, data))
            {
                logger.ErrorFormat("WriteDigtalOutput失败, 返回的数据和发送的数据不一致");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 打包Modbus数据
        /// </summary>
        /// <param name="addressCode">地址码</param>
        /// <param name="fcode">功能代码</param>
        /// <param name="data">要发送的数据</param>
        /// <returns></returns>
        private byte[] PackData(byte addressCode, byte fcode, byte[] data)
        {
            byte[] buffer = new byte[1 + 1 + data.Length];
            buffer[0] = addressCode;
            buffer[1] = fcode;
            Buffer.BlockCopy(data, 0, buffer, 2, data.Length);

            byte[] crcBytes = this.ReverseBytes(CRC.CRC16(buffer));

            byte[] result = new byte[buffer.Length + crcBytes.Length];
            Buffer.BlockCopy(buffer, 0, result, 0, buffer.Length);
            Buffer.BlockCopy(crcBytes, 0, result, buffer.Length, crcBytes.Length);
            return result;
        }

        private byte[] ReverseBytes(byte[] src)
        {
            return src.Reverse().ToArray();
        }

        #endregion
    }
}
