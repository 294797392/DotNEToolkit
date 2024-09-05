using System;
using System.Collections;
using System.Collections.Generic;
using DotNEToolkit;

namespace Factory.NET.IODrivers
{
    public static class IODriverFactory
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("IODriverFactory");

        public static AbstractIODriver Create(IODriverTypes driverType)
        {
            switch (driverType)
            {
                case IODriverTypes.SerialPort: return new SerialPortIODriver();
                case IODriverTypes.TcpClient: return new TcpClientIODriver();
                case IODriverTypes.VirtualDevice: return new VirtualIODriver();
                default:
                    // 不应该发生
                    throw new NotImplementedException();
            }
        }

        public static AbstractIODriver Create(IDictionary parameters)
        {
            string entryClass = parameters.GetValue<string>("IODriverEntryClass", string.Empty);
            if (!string.IsNullOrEmpty(entryClass))
            {
                try
                {
                    return ConfigFactory<AbstractIODriver>.CreateInstance(entryClass);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("创建IODriver异常, classEntry:{0}, {1}", entryClass, ex);
                    return null;
                }
            }
            else
            {
                IODriverTypes types = parameters.GetValue<IODriverTypes>("IODriverType", IODriverTypes.SerialPort);
                return Create(types);
            }
        }
    }
}