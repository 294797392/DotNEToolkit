using DotNEToolkit.Bindings;
using DotNEToolkit.Modular;
using Factory.NET.Channels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Factory.NET.Modbus
{
    /// <summary>
    /// 数据帧格式：
    /// [设备地址] [功能码] [数据] [CRC校验]
    /// 
    /// 1. 设备地址 (Address Field)
    /// 长度：1字节
    /// 范围：0x01 到 0xFF（通常使用 0x01 到 0x7F）
    /// 描述：标识目标设备的地址。主机发送请求时，指定从机的地址；从机响应时，返回自己的地址。
    /// 
    /// 2. 功能码 (Function Code)
    /// 长度：1字节
    /// 描述：定义操作类型。例如：
    /// 0x01：读线圈状态
    /// 0x02：读离散输入
    /// 0x03：读保持寄存器
    /// 0x06：写单个寄存器
    /// 0x10：写多个寄存器
    /// 
    /// 3. 数据 (Data Field)
    /// 长度：可变
    /// 描述：根据功能码的不同，数据字段的内容和长度会变化。例如：
    /// 功能码 0x03：读取寄存器值时，数据字段包含寄存器的数量和实际值。
    /// 功能码 0x06：写单个寄存器时，数据字段包含寄存器地址和要写入的值。
    /// 
    /// 4. CRC校验 (Cyclic Redundancy Check)
    /// 长度：2字节
    /// 描述：用于检测数据传输过程中是否发生错误。CRC值通过特定算法计算得出，并附加在帧的末尾。
    /// 
    /// RTU：远程终端控制系统
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

        #region 公开接口

        /// <summary>
        /// 读保持寄存器
        /// </summary>
        /// <param name="slaveAddress">从机地址</param>
        /// <param name="addr">要读取的保存寄存器地址</param>
        /// <param name="count">要读取的保持寄存器的数量</param>
        /// <returns></returns>
        public byte[] ReadHoldingRegister(byte slaveAddress, ushort addr, ushort count)
        {
            byte[] dataBytes = new byte[4];
            dataBytes[0] = (byte)(addr >> 8);  // 起始地址高字节
            dataBytes[1] = (byte)(addr & 0xFF); // 起始地址低字节
            dataBytes[2] = (byte)(count >> 8); // 寄存器数量高字节
            dataBytes[3] = (byte)(count & 0xFF); // 寄存器数量低字节

            byte[] readPacket;
            if (!this.SendAndReceive(slaveAddress, 0x03, dataBytes, out readPacket))
            {
                return null;
            }

            // 一个寄存器存储2字节数据
            byte[] result = new byte[count * 2];
            Buffer.BlockCopy(readPacket, 3, result, 0, result.Length);
            return result;
        }

        #endregion

        #region 实例方法

        /// <summary>
        /// 打包Modbus数据
        /// </summary>
        /// <param name="slaveAddress">地址码</param>
        /// <param name="fcode">功能代码</param>
        /// <param name="dataBytes">要发送的数据</param>
        /// <returns></returns>
        private byte[] CreatePacket(byte slaveAddress, byte functionCode, byte[] dataBytes)
        {
            byte[] result = new byte[2 + dataBytes.Length + 2];
            result[0] = slaveAddress;
            result[1] = functionCode;
            Buffer.BlockCopy(dataBytes, 0, result, 2, dataBytes.Length);

            byte[] crcBytes = this.ReverseBytes(CRC16(result, 0, result.Length - 2));
            Buffer.BlockCopy(crcBytes, 0, result, result.Length - 2, crcBytes.Length);
            return result;
        }

        private byte[] ReadPacket()
        {
            byte[] bytes1 = this.channel.ReadBytesFull(2);

            byte functionCode = bytes1[1];
            if (functionCode > 0x80)
            {
                // 如果从机无法处理请求，会返回异常帧，功能码会被设置为 请求功能码 + 0x80，并附加错误代码。
                byte[] bytes2 = this.channel.ReadBytesFull(3);
                List<byte> packet = new List<byte>();
                packet.AddRange(bytes1);
                packet.AddRange(bytes2);
                return packet.ToArray();
            }

            switch (functionCode)
            {
                case 0x03:
                    {
                        byte[] bytes2 = this.channel.ReadBytesFull(1);

                        // 后续有几个字节的数据
                        byte remain = bytes2[0];

                        // 加2是CRC占用2字节
                        byte[] bytes3 = this.channel.ReadBytesFull(remain + 2);

                        List<byte> packet = new List<byte>();
                        packet.AddRange(bytes1);
                        packet.AddRange(bytes2);
                        packet.AddRange(bytes3);

                        return packet.ToArray();
                    }

                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        private bool SendAndReceive(byte slaveAddress, byte functionCode, byte[] dataBytes, out byte[] readPacket)
        {
            readPacket = null;

            byte[] sendPacket = this.CreatePacket(slaveAddress, functionCode, dataBytes);

            try
            {
                this.channel.WriteBytes(sendPacket);

                readPacket = this.ReadPacket();

                if (readPacket[1] != sendPacket[1])
                {
                    logger.ErrorFormat("指令执行失败, {0}, {1}", this.FCode2Text(sendPacket[1]), this.ErrorCode2Text(readPacket[2]));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("发送数据异常", ex);
                return false;
            }
        }

        private byte[] ReverseBytes(byte[] src)
        {
            return src.Reverse().ToArray();
        }

        private string FCode2Text(byte functionCode)
        {
            switch (functionCode)
            {
                case 0x01: return "读线圈状态 (Read Coils)";
                case 0x02: return "读离散输入 (Read Discrete Inputs)";
                case 0x03: return " 读保持寄存器 (Read Holding Registers)";
                case 0x04: return "读输入寄存器 (Read Input Registers)";
                case 0x05: return "写单个线圈 (Write Single Coil)";
                case 0x06: return "写单个寄存器 (Write Single Register)";
                case 0x07: return "写多个线圈 (Write Multiple Coils)";
                case 0x08: return "写多个寄存器 (Write Multiple Registers)";
                default: return string.Format("未知的功能码:{0}", functionCode);
            }
        }

        private string ErrorCode2Text(byte errorCode)
        {
            switch (errorCode)
            {
                case 0x01: return "非法功能 (Illegal Function)";
                case 0x02: return "非法数据地址 (Illegal Data Address)";
                case 0x03: return "非法数据值 (Illegal Data Value)";
                case 0x04: return "从机设备故障 (Slave Device Failure)";
                case 0x05: return "确认 (Acknowledge)";
                case 0x06: return "从机设备忙 (Slave Device Busy)";
                case 0x07: return "内存奇偶校验错误 (Memory Parity Error)";
                case 0x08: return "网关路径不可用 (Gateway Path Unavailable)";
                case 0x0A: return "网关目标设备未响应 (Gateway Target Device Failed to Respond)";
                default: return string.Format("未知的错误码:{0}", errorCode);
            }
        }

        private byte[] CRC16(byte[] data, int offset, int len)
        {
            ushort crc = 0xFFFF;

            for (int i = 0; i < len; i++)
            {
                crc = (ushort)(crc ^ (data[i + offset]));
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                }
            }
            byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
            byte lo = (byte)(crc & 0x00FF);         //低位置

            return new byte[] { hi, lo };
        }

        #endregion
    }
}
