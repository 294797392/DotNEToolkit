using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Communictions
{
    /// <summary>
    /// 创建通信对象的工厂
    /// </summary>
    public static class CommObjectFactory
    {
        public static CommunicationObject Create(CommunicationTypes type)
        {
            switch (type)
            {
                case CommunicationTypes.SerialPort: return new SerialPortCommObject();
                case CommunicationTypes.TcpClient: return new TcpClientCommObject();
                case CommunicationTypes.TcpService: return new TcpServiceCommObject();

                default:
                    throw new NotImplementedException(string.Format("未实现{0}的通信设备", type));
            }
        }
    }
}
