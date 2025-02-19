using DotNEToolkit.Modular;
using Factory.NET.Channels;
using System;
using System.Linq;

namespace Factory.NET.Modbus
{
    /// <summary>
    /// +------------------+------------------+------------------+------------------+------------------+
    /// | 事务标识符(2B) | 协议标识符(2B) | 长度(2B)       | 单元标识符(1B) | PDU(N 字节)     |
    /// +------------------+------------------+------------------+------------------+------------------+
    /// </summary>
    public class ModbusTCPClient : ModuleBase
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModbusTCPClient");

        #endregion

        #region 实例变量

        private ushort transactionId;

        /// <summary>
        /// 与PLC通信的对象
        /// </summary>
        private ChannelBase channel;

        #endregion

        #region 属性

        /// <summary>
        /// 单元标识符
        /// 指定目标设备地址，通常为1表示主设备
        /// </summary>
        public byte UnitIdentifier { get; set; }

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            this.UnitIdentifier = 1;

            this.channel = ChannelFactory.Create(this.InputParameters);
            this.channel.Initialize(this.InputParameters);

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        #endregion

        #region 实例方法

        private ushort GetTransactionId()
        {
            return ++this.transactionId;
        }

        private byte[] ReadPacket()
        {
            byte[] bytes1 = null, bytes2 = null;

            try
            {
                bytes1 = this.channel.ReadBytesFull(6);

                bytes2 = this.channel.ReadBytesFull(bytes1[5]);
            }
            catch (Exception ex)
            {
                logger.Error("接收数据包异常", ex);
                return null;
            }

            byte[] packet = new byte[bytes1.Length + bytes2.Length];
            Buffer.BlockCopy(bytes1, 0, packet, 0, bytes1.Length);
            Buffer.BlockCopy(bytes2, 0, packet, bytes1.Length, bytes2.Length);

            return packet;
        }

        private byte[] CreatePacket(byte[] pduBytes) 
        {
            #region MBAP（Modbus Application Protocol Header）

            byte[] mbapBytes = new byte[7];

            // 事务标识符2字节
            ushort transactionId = this.GetTransactionId();
            byte[] transactionIdBytes = BitConverter.GetBytes(transactionId).Reverse().ToArray();
            Buffer.BlockCopy(transactionIdBytes, 0, mbapBytes, 0, transactionIdBytes.Length);

            // 协议标识符2字节，0x0000

            // 长度2字节，表示后续字节数，包括单元标识符和PDU总长度
            ushort length = (ushort)(pduBytes.Length + 1);
            byte[] lengthBytes = BitConverter.GetBytes(length).Reverse().ToArray();
            Buffer.BlockCopy(lengthBytes, 0, mbapBytes, 4, lengthBytes.Length);

            // 单元标识符1字节，指定目标设备地址，通常为1表示主设备
            mbapBytes[6] = this.UnitIdentifier;

            #endregion

            byte[] tcpPacket = new byte[mbapBytes.Length + pduBytes.Length];
            Buffer.BlockCopy(mbapBytes, 0, tcpPacket, 0, mbapBytes.Length);
            Buffer.BlockCopy(pduBytes, 0, tcpPacket, mbapBytes.Length, pduBytes.Length);

            return tcpPacket;
        }

        private bool SendAndReceive(byte[] pduBytes, out byte[] readPacket)
        {
            readPacket = null;

            byte[] sendPacket = this.CreatePacket(pduBytes);

            try
            {
                this.channel.WriteBytes(sendPacket);
            }
            catch (Exception ex)
            {
                logger.Error("发送数据异常", ex);
                return false;
            }

            readPacket = this.ReadPacket();
            if (readPacket[7] != readPacket[7])
            {
                logger.ErrorFormat("指令执行失败, {0}, {1}", this.FCode2Text(readPacket[7]), this.ErrorCode2Text(readPacket[8]));
                return false;
            }

            return true;
        }

        private string ErrorCode2Text(byte code)
        {
            switch (code)
            {
                case 0x01: return "非法功能";
                case 0x02: return "非法数据地址";
                case 0x03: return "非法数据值";
                case 0x04: return "设备故障";
                default: return string.Format("未知错误码:{0}", code);
            }
        }

        private string FCode2Text(byte fcode) 
        {
            switch (fcode) 
            {
                case 0x01: return "读线圈状态";
                case 0x02: return "读离散输入";
                case 0x03: return "读保持寄存器";
                case 0x04: return "读输入寄存器";
                case 0x05: return "写单个线圈";
                case 0x06: return "写单个寄存器";
                case 0x0F: return "写多个线圈";
                case 0x10: return "写多个寄存器";
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取线圈状态
        /// 0是OFF，1是ON，读取出来的数据里每一位表示线圈状态
        /// </summary>
        /// <param name="addr">线圈状态起始地址</param>
        /// <param name="count">要读取的线圈数量</param>
        /// <returns></returns>
        public byte[] ReadCoils(ushort addr, ushort count)
        {
            #region PDU

            byte[] pduBytes = new byte[5];

            // 功能码1字节
            pduBytes[0] = 0x01;

            // 起始地址2字节，从0x0000开始
            byte[] addrBytes = BitConverter.GetBytes(addr).Reverse().ToArray();
            Buffer.BlockCopy(addrBytes, 0, pduBytes, 1, addrBytes.Length);

            // 线圈数量2字节，最大值为0x07D0，即2000个线圈
            byte[] countBytes = BitConverter.GetBytes(count).Reverse().ToArray();
            Buffer.BlockCopy(countBytes, 0, pduBytes, 3, countBytes.Length);

            #endregion

            byte[] readPacket;
            if (!this.SendAndReceive(pduBytes, out readPacket))
            {
                return null;
            }

            byte[] values = new byte[readPacket.Length - 9];
            Buffer.BlockCopy(readPacket, 9, values, 0, values.Length);
            return values;
        }

        /// <summary>
        /// 写入线圈状态
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="count">要写入的线圈数量</param>
        /// <param name="values">要写入的线圈值，按位排列</param>
        /// <returns></returns>
        public bool WriteCoils(ushort addr, ushort count, byte[] values)
        {
            #region PDU

            byte[] pduBytes = new byte[6 + values.Length];

            // 功能码1字节
            pduBytes[0] = 0x0F;

            // 起始地址2字节，从0x0000开始
            byte[] addrBytes = BitConverter.GetBytes(addr).Reverse().ToArray();
            Buffer.BlockCopy(addrBytes, 0, pduBytes, 1, addrBytes.Length);

            // 线圈数量2字节，最大值为0x07B0，即1968个线圈
            byte[] countBytes = BitConverter.GetBytes(count).Reverse().ToArray();
            Buffer.BlockCopy(countBytes, 0, pduBytes, 3, countBytes.Length);

            // 数据部分的字节数1字节
            pduBytes[5] = (byte)values.Length;

            // 数据，N字节，实际要写入的线圈状态，按位排列
            Buffer.BlockCopy(values.Reverse().ToArray(), 0, pduBytes, 6, values.Length);

            #endregion

            byte[] readPacket = null;
            if (!this.SendAndReceive(pduBytes, out readPacket))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
