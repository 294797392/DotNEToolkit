using DotNEToolkit.Modular;
using Factory.NET.IODrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private AbstractIODriver channel;

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

            this.channel = IODriverFactory.Create(this.InputParameters);
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
            byte[] sendPacket = new byte[12];

            // 事务标识符
            ushort transactionId = this.GetTransactionId();
            byte[] transactionIdBytes = BitConverter.GetBytes(transactionId).Reverse().ToArray();
            Buffer.BlockCopy(transactionIdBytes, 0, sendPacket, 0, transactionIdBytes.Length);

            // 长度
            byte[] lengthBytes = BitConverter.GetBytes((ushort)6).Reverse().ToArray();
            Buffer.BlockCopy(lengthBytes, 0, sendPacket, 4, lengthBytes.Length);

            // 单元标识符
            sendPacket[6] = this.UnitIdentifier;

            // 功能码
            sendPacket[7] = 0x01;

            byte[] addrBytes = BitConverter.GetBytes(addr).Reverse().ToArray();
            Buffer.BlockCopy(addrBytes, 0, sendPacket, 8, addrBytes.Length);

            byte[] countBytes = BitConverter.GetBytes(count).Reverse().ToArray();
            Buffer.BlockCopy(countBytes, 0, sendPacket, 10, countBytes.Length);

            try
            {
                this.channel.WriteBytes(sendPacket);
            }
            catch (Exception ex)
            {
                logger.Error("数据交互异常", ex);
                return null;
            }

            byte[] readPacket = this.ReadPacket();
            if (readPacket == null)
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
            byte[] sendPacket = new byte[13 + values.Length];

            // 事务标识符
            ushort transactionId = this.GetTransactionId();
            byte[] transactionIdBytes = BitConverter.GetBytes(transactionId).Reverse().ToArray();
            Buffer.BlockCopy(transactionIdBytes, 0, sendPacket, 0, transactionIdBytes.Length);

            // 长度
            byte[] lengthBytes = BitConverter.GetBytes((ushort)(sendPacket.Length - 6)).Reverse().ToArray();
            Buffer.BlockCopy(lengthBytes, 0, sendPacket, 4, lengthBytes.Length);

            // 单元标识符
            sendPacket[6] = this.UnitIdentifier;

            // 功能码，0x0F是写多个线圈
            sendPacket[7] = 0x0F;

            byte[] addrBytes = BitConverter.GetBytes(addr).Reverse().ToArray();
            Buffer.BlockCopy(addrBytes, 0, sendPacket, 8, addrBytes.Length);

            byte[] countBytes = BitConverter.GetBytes(count).Reverse().ToArray();
            Buffer.BlockCopy(countBytes, 0, sendPacket, 10, countBytes.Length);

            // 数据部分的字节数
            sendPacket[12] = (byte)values.Length;

            Buffer.BlockCopy(values.Reverse().ToArray(), 0, sendPacket, 13, values.Length);

            try
            {
                this.channel.WriteBytes(sendPacket);
            }
            catch (Exception ex)
            {
                logger.Error("数据交互异常", ex);
                return false;
            }

            byte[] readPacket = this.ReadPacket();
            if (readPacket[7] != 0x0F)
            {
                logger.ErrorFormat("写入线圈失败, {0}", this.ErrorCode2Text(readPacket[8]));
                return false;
            }

            return true;
        }

        #endregion
    }
}
