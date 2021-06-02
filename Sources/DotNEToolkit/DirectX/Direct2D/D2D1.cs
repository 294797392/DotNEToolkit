using DotNEToolkit.Win32API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DirectX.Direct2D
{
    /// <summary>
    /// Direct2D中的接口ID
    /// </summary>
    public static class D2DInterfaceID
    {
        public const string ID2D1Factory = "{06152247-6F50-465A-9245-118BFD3B6007}";
    }

    /// <summary>
    /// 关于D2D函数的C#封装
    /// D2D相关的帮助函数
    /// </summary>
    public static class D2D1
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("D2D1");

        private const string Direct2DllName = "D2d1.dll";

        /// <summary>
        /// Creates a factory object that can be used to create Direct2D resources.
        /// </summary>
        /// <param name="factoryType"></param>
        /// <param name="riid"></param>
        /// <param name="pFactoryOptions"></param>
        /// <param name="ppIFactory"></param>
        /// <returns></returns>
        [DllImport(Direct2DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int D2D1CreateFactory(D2D1_FACTORY_TYPE factoryType, IntPtr riid, IntPtr pFactoryOptions, [MarshalAs(UnmanagedType.Interface)] out object ppIFactory);

        /// <summary>
        /// The ID2D1Factory interface provides the starting point for Direct2D. 
        /// In general, an object created from a single instance of a factory object can be used with other resources created from that instance, 
        /// but not with resources created by other factory instances.
        /// </summary>
        /// <returns></returns>
        public static bool CreateD2DFactory(ID2D1Factory factory)
        {
            factory = null;
            object ppIFactory = null;
            int rc = Win32Error.S_OK;
            IntPtr ptrIID = IntPtr.Zero;

            GUID iid;
            if ((rc = OLE.IIDFromString(D2DInterfaceID.ID2D1Factory, out iid)) != Win32Error.S_OK)
            {
                logger.ErrorFormat("获取ID2D1Factory IID失败, guid = {0}, code = {1}", D2DInterfaceID.ID2D1Factory, rc);
                return false;
            }

            try
            {
                ptrIID = Marshal.AllocHGlobal(Marshal.SizeOf(iid));
                Marshal.StructureToPtr(iid, ptrIID, true);
                if ((rc = D2D1.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, ptrIID, IntPtr.Zero, out ppIFactory)) != Win32Error.S_OK)
                {
                    logger.ErrorFormat("D2D1CreateFactory失败, code = {0}", rc);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("创建D2DFactory异常", ex);
                return false;
            }
            finally
            {
                if (ptrIID != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptrIID);
                }
            }

            factory = ppIFactory as ID2D1Factory;

            return true;
        }
    }
}