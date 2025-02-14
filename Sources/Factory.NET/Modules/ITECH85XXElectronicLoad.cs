using DotNEToolkit;
using DotNEToolkit.DataAccess;
using DotNEToolkit.Modular;
using Factory.NET.IODrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static Factory.NET.Modules.ITECH85XXElectronicLoad;
using static System.Collections.Specialized.BitVector32;

namespace Factory.NET.Modules
{
    public class ITECH85XXElectronicLoad : ModuleBase
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("IT85XXElectronicLoad");

        public enum ControlMode
        {
            /// <summary>
            /// 面板操作模式
            /// </summary>
            Panel = 0,

            /// <summary>
            /// 远程操作模式
            /// </summary>
            Remote = 1
        }

        public enum InputMode
        {
            OFF = 0,

            ON = 1
        }

        public enum ElectronicLoadMode
        {
            /// <summary>
            /// 定电流模式
            /// 在定电流模式下，不管输入电压是否改变，电子负载消耗一个恒定的电流。
            /// </summary>
            CC = 0,

            /// <summary>
            /// 定电压模式
            /// 在定电压模式下，电子负载将消耗足够的电流来使输入电压维持在设定的电压上
            /// </summary>
            CV = 1,

            /// <summary>
            /// 定功率模式
            /// 在定功率模式下，电子负载将消耗一个恒定的功率，如果输入电压升高，输入电流将减少，功率 P（=V*I）将维持在设定功率上。
            /// </summary>
            CW = 2,

            /// <summary>
            /// 定电阻模式
            /// 在定电阻模式下，电子负载被等效为一个恒定的电阻，电子负载会随着输入电压的改变来线性改变电流。
            /// </summary>
            CR = 3
        }

        #region 实例变量

        private AbstractIODriver channel;

        #endregion

        #region 属性

        /// <summary>
        /// 负载地址（0-31）
        /// </summary>
        public byte Address { get; set; }

        #endregion

        #region ModuleBase

        protected override int OnInitialize()
        {
            this.channel = IODriverFactory.Create(this.InputParameters);
            this.channel.Initialize(this.InputParameters);

            return ResponseCode.SUCCESS;
        }

        protected override void OnRelease()
        {
        }

        #endregion

        #region 公开接口

        public bool SetControlMode(ControlMode controlMode)
        {
            byte[] buffer = new byte[] { (byte)controlMode };
            byte[] requestPacket = this.CreatePacket(0x20, buffer);
            byte[] responsePacket = null;

            return this.SendRequest(string.Format("设置负载的控制模式, {0}", controlMode), requestPacket, out responsePacket);
        }

        public bool SetInputMode(InputMode inputMode)
        {
            byte[] buffer = new byte[] { (byte)inputMode };
            byte[] requestPacket = this.CreatePacket(0x21, buffer);
            byte[] responsePacket = null;

            return this.SendRequest(string.Format("控制负载输入状态, {0}", inputMode), requestPacket, out responsePacket);
        }

        public bool SetMode(ElectronicLoadMode electronicLoadMode)
        {
            byte[] buffer = new byte[]
            {
                (byte)electronicLoadMode
            };
            byte[] requestPacket = this.CreatePacket(0x28, buffer);
            byte[] responsePacket = null;

            return this.SendRequest(string.Format("设置负载模式, {0}", electronicLoadMode), requestPacket, out responsePacket);
        }

        /// <summary>
        /// 设置定电流
        /// </summary>
        /// <param name="current">要设置的定电流大小，单位是A</param>
        public bool SetCurrent(int current)
        {
            byte[] buffer = BitConverter.GetBytes(current * 10000);
            byte[] requestPacket = this.CreatePacket(0x2A, buffer);
            byte[] responsePacket = null;

            return this.SendRequest(string.Format("设置负载的定电流值, {0}A", current), requestPacket, out responsePacket);
        }

        /// <summary>
        /// 设置定电压值
        /// </summary>
        /// <param name="voltage">要设置的定电压大小，单位是V</param>
        /// <returns></returns>
        public bool SetVoltage(int voltage)
        {
            byte[] buffer = BitConverter.GetBytes(voltage * 1000);
            byte[] requestPacket = this.CreatePacket(0x2C, buffer);
            byte[] responsePacket = null;

            return this.SendRequest(string.Format("设置负载的定电压值, {0}V", voltage), requestPacket, out responsePacket);
        }

        /// <summary>
        /// 设置定功率值
        /// </summary>
        /// <param name="power">单位是W</param>
        /// <returns></returns>
        public bool SetPower(int power)
        {
            byte[] buffer = BitConverter.GetBytes(power * 1000);
            byte[] requestPacket = this.CreatePacket(0x2E, buffer);
            byte[] responsePacket = null;

            return this.SendRequest(string.Format("设置负载的定功率值, {0}W", power), requestPacket, out responsePacket);
        }

        /// <summary>
        /// 设置定电阻值
        /// </summary>
        /// <param name="resistance">单位是R</param>
        /// <returns></returns>
        public bool SetResistance(int resistance)
        {
            byte[] buffer = BitConverter.GetBytes(resistance * 1000);
            byte[] requestPacket = this.CreatePacket(0x30, buffer);
            byte[] responsePacket = null;

            return this.SendRequest(string.Format("设置负载的定电阻值, {0}R", resistance), requestPacket, out responsePacket);
        }

        /// <summary>
        /// 读取负载的输入电压,输入电流,输入功率及相关状态
        /// </summary>
        /// <returns></returns>
        public bool ReadInputState(out double vol, out double cur, out double pow)
        {
            vol = 0;
            cur = 0;
            pow = 0;

            byte[] requestPacket = this.CreatePacket(0x5F);
            byte[] responsePacket = null;

            if (!this.SendRequest("读取负载的输入电压,输入电流,输入功率及相关状态", requestPacket, out responsePacket))
            {
                return false;
            }

            vol = (double)BitConverter.ToInt32(responsePacket, 3) / 1000;   // 1 = 1mV, 转换成V
            cur = (double)BitConverter.ToInt32(responsePacket, 7) / 10000;  // 1 = 0.1mA, 转换成A
            pow = (double)BitConverter.ToInt32(responsePacket, 11) / 1000; // 1 = 1mW, 转换成W

            return true;
        }

        #endregion

        #region 实例方法

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlByte"></param>
        /// <param name="data">从第四个字节开始的数据，不包含最后一个校验码</param>
        /// <returns></returns>
        private byte[] CreatePacket(byte controlByte, byte[] data = null)
        {
            byte[] packet = new byte[26];
            packet[0] = 0xAA;                   // 同步头
            packet[1] = this.Address;           // 负载地址
            packet[2] = controlByte;            // 命令字

            if (data != null)
            {
                // 拷贝从第4字节开始之后的数据
                Array.Copy(data, 0, packet, 3, data.Length);
            }

            // 计算校验和
            int checksum = 0;
            for (int i = 0; i < packet.Length - 1; i++)
            {
                checksum += i;
            }

            packet[25] = checksum > 0xFF ? (byte)0xFF : (byte)checksum;

            return packet;
        }

        private bool SendRequest(string action, byte[] requestPacket, out byte[] responsePacket)
        {
            responsePacket = null;

            try
            {
                this.channel.WriteBytes(requestPacket);

                responsePacket = this.channel.ReadBytesFull(26);
            }
            catch (Exception ex)
            {
                logger.Error("发送请求异常", ex);
                return false;
            }

            byte code = responsePacket[3];

            switch (code)
            {
                case 0x90:
                    {
                        logger.ErrorFormat("{0}, 校验和错误", action);
                        return false;
                    }
                case 0xA0:
                    {
                        logger.ErrorFormat("{0}, 设置参数错误或者参数溢出", action);
                        return false;
                    }

                case 0xB0:
                    {
                        logger.ErrorFormat("{0}, 命令不能被执行", action);
                        return false;
                    }

                case 0xC0:
                    {
                        logger.ErrorFormat("{0}, 命令是无效的", action);
                        return false;
                    }

                case 0x80:
                    {
                        return true;
                    }

                default:
                    {
                        logger.ErrorFormat("未知错误码:{0}, {1}", code, action);
                        return false;
                    }
            }

        }

        #endregion
    }
}
