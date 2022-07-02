using DotNEToolkit;
using DotNEToolkit.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Communictions
{
    /// <summary>
    /// 创建通信对象的工厂
    /// </summary>
    public static class CommObjectFactory
    {
        private const string KEY_TYPE = "type";
        private const string KEY_CLASS_NAME = "className";

        public static CommObject Create(IDictionary parameters)
        {
            // 优先通过className加载
            string className = parameters.GetValue<string>(KEY_CLASS_NAME, string.Empty);
            if (string.IsNullOrEmpty(className))
            {
                // className为空，那么通过type加载
                int type = parameters.GetValue<int>(KEY_TYPE, -1);
                if (type == -1)
                {
                    throw new NotImplementedException();
                }
                return CommObjectFactory.Create((CommTypes)type);
            }

            return ConfigFactory<CommObject>.CreateInstance(className);
        }

        public static CommObject Create(CommTypes type)
        {
            switch (type)
            {
                case CommTypes.SerialPort: return new SerialPortCommObject();
                case CommTypes.TcpClient: return new TcpClientCommObject();
                //case CommTypes.TcpService: return new TcpServiceCommObject();

                default:
                    throw new NotImplementedException(string.Format("未实现{0}的通信设备", type));
            }
        }
    }
}
