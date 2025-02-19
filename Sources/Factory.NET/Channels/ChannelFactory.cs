using System;
using System.Collections;
using System.Collections.Generic;
using DotNEToolkit;

namespace Factory.NET.Channels
{
    public static class ChannelFactory
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("ChannelFactory");

        public static ChannelBase Create(ChannelTypes driverType)
        {
            switch (driverType)
            {
                case ChannelTypes.SerialPort: return new SerialPortChannel();
                case ChannelTypes.TcpClient: return new TcpClientChannel();
                case ChannelTypes.VirtualDevice: return new VirtualChannel();
                default:
                    // 不应该发生
                    throw new NotImplementedException();
            }
        }

        public static ChannelBase Create(IDictionary parameters)
        {
            string entryClass = parameters.GetValue<string>("IODriverEntryClass", string.Empty);
            if (!string.IsNullOrEmpty(entryClass))
            {
                try
                {
                    return ConfigFactory<ChannelBase>.CreateInstance(entryClass);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("创建IODriver异常, classEntry:{0}, {1}", entryClass, ex);
                    return null;
                }
            }
            else
            {
                ChannelTypes types = parameters.GetValue<ChannelTypes>("IODriverType", ChannelTypes.SerialPort);
                return Create(types);
            }
        }
    }
}