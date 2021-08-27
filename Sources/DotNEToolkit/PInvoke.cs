using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供PInvoke的公共函数
    /// </summary>
    public static class PInvoke
    {
        /// <summary>
        /// 结构体转byte数组, 不会释放内存
        /// </summary>
        /// <param name="structObj">要转换的结构体</param>
        /// <returns>转换后的byte数组</returns>
        public static IntPtr StructureToPtr(object structObj)
        {
            int size = Marshal.SizeOf(structObj);

            IntPtr structPtr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(structObj, structPtr, false);

            //Marshal.FreeHGlobal(structPtr);

            return structPtr;
        }

        /// <summary>
        /// 释放用StructureToPtr函数分配的结构体指针
        /// </summary>
        /// <param name="structPtr">要释放的指针</param>
        public static void FreeStructurePtr(IntPtr structPtr)
        {
            if (structPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(structPtr);
            }
        }
    }
}
