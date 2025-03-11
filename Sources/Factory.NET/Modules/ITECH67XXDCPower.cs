using DotNEToolkit.Modular;
using Factory.NET.Channels;
using System;

namespace Factory.NET.Modules
{
    /// <summary>
    /// ITECH6700系列直流程控电源
    /// </summary>
    public class ITECH67XXDCPower : ModuleBase
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ITECH67XXDCPower");

        private static readonly char[] Splitter = new char[] { ',' };

        /// <summary>
        /// 通讯方式
        /// </summary>
        public enum CommTypes
        {
            Frame,

            SCPI
        }

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

        #region 实例变量

        private ChannelBase channel;

        private CommTypes CommType;

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
            this.CommType = this.GetParameter<CommTypes>("commType", CommTypes.SCPI);
            if (this.CommType == CommTypes.SCPI)
            {
                // SCPI用的是\n换行符
                this.InputParameters["NewLine"] = "\n";
            }

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

        public bool SetControlMode(ControlMode controlMode)
        {
            if (this.CommType == CommTypes.SCPI)
            {
                switch (controlMode)
                {
                    case ControlMode.Remote:
                        {
                            return this.SendSCPIRequest("设置电源远程控制模式", "SYST:REM");
                        }

                    case ControlMode.Panel:
                        {
                            return this.SendSCPIRequest("设置电源本地控制模式", "SYST:LOC");
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                byte[] buffer = new byte[] { (byte)controlMode };
                byte[] requestPacket = this.CreatePacket(0x20, buffer);
                byte[] responsePacket = null;

                return this.SendRequest(string.Format("设置电源的控制模式, {0}", controlMode), requestPacket, out responsePacket);
            }
        }

        public bool SetInputMode(InputMode inputMode)
        {
            if (this.CommType == CommTypes.SCPI)
            {
                switch (inputMode)
                {
                    case InputMode.OFF:
                        {
                            return this.SendSCPIRequest("关闭电源", "OUTP 0");
                        }

                    case InputMode.ON:
                        {
                            return this.SendSCPIRequest("打开电源", "OUTP 1");
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                byte[] buffer = new byte[] { (byte)inputMode };
                byte[] requestPacket = this.CreatePacket(0x21, buffer);
                byte[] responsePacket = null;

                return this.SendRequest(string.Format("控制电源输入状态, {0}", inputMode), requestPacket, out responsePacket);
            }
        }

        /// <summary>
        /// 设置电源的电压上限
        /// </summary>
        /// <param name="limitVoltage"></param>
        /// <returns></returns>
        public bool SetLimitVoltage(double voltage)
        {
            if (this.CommType == CommTypes.SCPI)
            {
                throw new NotImplementedException();
            }
            else
            {
                byte[] buffer = BitConverter.GetBytes(voltage * 1000);
                byte[] requestPacket = this.CreatePacket(0x22, buffer);
                byte[] responsePacket = null;

                return this.SendRequest(string.Format("设置电源的定电压上限, {0}V", voltage), requestPacket, out responsePacket);
            }
        }

        /// <summary>
        /// 设置电源的输出电压
        /// </summary>
        /// <param name="voltage">要设置的电压大小，单位是V</param>
        /// <returns></returns>
        public bool SetVoltage(double voltage)
        {
            if (this.CommType == CommTypes.SCPI)
            {
                return this.SendSCPIRequest("设置输出电压", string.Format("VOLT {0}", voltage));
            }
            else
            {
                byte[] buffer = BitConverter.GetBytes(voltage * 1000);
                byte[] requestPacket = this.CreatePacket(0x23, buffer);
                byte[] responsePacket = null;

                return this.SendRequest(string.Format("设置电源的输出电压, {0}V", voltage), requestPacket, out responsePacket);
            }
        }

        /// <summary>
        /// 设置电源的输出电流
        /// </summary>
        /// <param name="current">要设置的电流大小，单位是A</param>
        /// <returns></returns>
        public bool SetCurrent(double current)
        {
            if (this.CommType == CommTypes.SCPI)
            {
                return this.SendSCPIRequest("设置最大电流保护", string.Format(":CURR {0}", current));
            }
            else
            {
                byte[] buffer = BitConverter.GetBytes(current * 1000);
                byte[] requestPacket = this.CreatePacket(0x24, buffer);
                byte[] responsePacket = null;

                return this.SendRequest(string.Format("设置电源的输出电流, {0}A", current), requestPacket, out responsePacket);
            }
        }

        /// <summary>
        /// 读取负载的输入电压,输入电流,输入功率及相关状态
        /// </summary>
        /// <returns></returns>
        public bool ReadState(out double vol, out double cur)
        {
            vol = 0;
            cur = 0;

            if (this.CommType == CommTypes.SCPI)
            {
                string volstr, curstr;
                if (!this.SendSCPIRequest("读取电压", ":MEAS:VOLT?", out volstr) ||
                    !this.SendSCPIRequest("读取电流", ":MEAS:CURR?", out curstr))
                {
                    return false;
                }

                if (!double.TryParse(volstr, out vol)) 
                {
                    logger.ErrorFormat("电压格式不正确, {0}", volstr);
                    return false;
                }

                if (!double.TryParse(curstr, out cur))
                {
                    logger.ErrorFormat("电流值格式不正确, {0}", cur);
                    return false;
                }

                return true;
            }
            else
            {

                byte[] requestPacket = this.CreatePacket(0x26);
                byte[] responsePacket = null;

                if (!this.SendRequest("读取电源的实际输出电流, 实际输出电压及相关状态", requestPacket, out responsePacket))
                {
                    return false;
                }

                cur = (double)BitConverter.ToUInt16(responsePacket, 3) / 1000;
                vol = (double)BitConverter.ToInt32(responsePacket, 5) / 10000;

                return true;
            }
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
                checksum += packet[i];
            }

            packet[25] = checksum > byte.MaxValue ? (byte)(checksum % 256) : (byte)checksum;

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

        private bool SendSCPIRequest(string action, string scpi, out string result)
        {
            result = string.Empty;

            try
            {
                this.channel.WriteLine(scpi);

                result = this.channel.ReadLine();

                return this.SYST_ERR(action);
            }
            catch (Exception ex)
            {
                logger.Error("发送SCPI指令异常", ex);
                return false;
            }
        }

        private bool SendSCPIRequest(string action, string scpi)
        {
            try
            {
                this.channel.WriteLine(scpi);
            }
            catch (Exception ex)
            {
                logger.Error("发送SCPI指令异常", ex);
                return false;
            }

            return this.SYST_ERR(action);
        }

        private bool SYST_ERR(string action)
        {
            string result = string.Empty;

            try
            {
                this.channel.WriteLine("SYST:ERR?");

                result = this.channel.ReadLine();
            }
            catch (Exception ex)
            {
                logger.Error("发送SCPI指令异常", ex);
                return false;
            }

            if (string.IsNullOrEmpty(result))
            {
                logger.ErrorFormat("{0}, 执行失败, 仪器有错误发生, SYST:ERR?返回值为空", action);
                return false;
            }

            string[] items = result.Split(Splitter, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length != 2)
            {
                logger.ErrorFormat("{0}, 执行失败, SYST:ERR?返回值不正确, {1}", action, result);
                return false;
            }

            if (items[0] != "0")
            {
                logger.ErrorFormat("{0}, 执行失败, {1}", action, result);
                return false;
            }

            return true;
        }

        #endregion
    }
}
