using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace DotNEToolkit
{
    public static class Networks
    {
        /// <summary>
        /// 获取所有网卡的MAC地址
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMACAddress()
        {
            List<string> result = new List<string>();

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface iface in interfaces)
            {
                result.Add(iface.GetPhysicalAddress().ToString());
            }

            return result;
        }
    }
}
