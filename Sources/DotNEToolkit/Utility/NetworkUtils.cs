using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Utility
{
    /// <summary>
    /// 提供网络相关的帮助函数
    /// </summary>
    public static class NetworkUtils
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("NetworkUtils");

        /// <summary>
        /// 获取所有网卡的IPv4广播地址
        /// </summary>
        /// <returns></returns>
        public static List<IPAddress> GetBroadcastAddresses()
        {
            List<IPAddress> broadcastAddresses = new List<IPAddress>();

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface @interface in interfaces)
            {
                IPInterfaceProperties properties = @interface.GetIPProperties();

                // 一张网卡可以设置多个IP地址，这里取第一个，取IPV4
                UnicastIPAddressInformation unicastIPAddress = properties.UnicastAddresses.FirstOrDefault(v => v.Address.AddressFamily == AddressFamily.InterNetwork);
                if (unicastIPAddress == null)
                {
                    continue;
                }

                IPAddress maskAddress = unicastIPAddress.IPv4Mask;
                IPAddress ipAddress = unicastIPAddress.Address;

                byte[] ipBytes = ipAddress.GetAddressBytes();
                byte[] maskBytes = maskAddress.GetAddressBytes();
                byte[] broadcastDomain = new byte[maskBytes.Length];
                byte[] broadcastBytes = new byte[maskBytes.Length];

                // 1. IP与子网掩码与运算，即为广播域
                for (int i = 0; i < ipBytes.Length; i++)
                {
                    broadcastDomain[i] = (byte)(ipBytes[i] & maskBytes[i]);
                }

                // 2. 子网掩码取反后与广播域或运算，即为广播地址
                for (int i = 0; i < ipBytes.Length; i++)
                {
                    broadcastBytes[i] = (byte)(~maskBytes[i] | broadcastDomain[i]);
                }

                broadcastAddresses.Add(new IPAddress(broadcastBytes));
            }

            return broadcastAddresses;
        }

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
