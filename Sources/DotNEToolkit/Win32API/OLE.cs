using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Win32API
{
    /// <summary>
    /// OLE API
    /// </summary>
    public static class OLE
    {
        private const string DllName = "Ole32.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int StringFromIID(GUID rclsid, out string lplpsz);

        /// <summary>
        /// InterfaceID字符串转GUID结构体
        /// </summary>
        /// <param name="rclsid"></param>
        /// <param name="lplpsz"></param>
        /// <returns></returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int IIDFromString(string lpsz, out GUID lpiid);
    }
}