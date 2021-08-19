using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.DirectSound
{
    public class DSSCL
    {
        /// <summary>
        /// Sets the normal level. This level has the smoothest multitasking and resource-sharing behavior, but because it does not allow the primary buffer format to change, output is restricted to the default 8-bit format. 
        /// </summary>
        public static readonly uint DSSCL_NORMAL = 0x00000001;

        /// <summary>
        /// Sets the priority level. Applications with this cooperative level can call the SetFormat and Compact methods. 
        /// </summary>
        public static readonly uint DSSCL_PRIORITY = 0x00000002;

        /// <summary>
        /// For DirectX 8.0 and later, has the same effect as DSSCL_PRIORITY. For previous versions, sets the application to the exclusive level. This means that when it has the input focus, the application will be the only one audible; sounds from applications with the DSBCAPS_GLOBALFOCUS flag set will be muted. With this level, it also has all the privileges of the DSSCL_PRIORITY level. DirectSound will restore the hardware format, as specified by the most recent call to the SetFormat method, after the application gains the input focus. 
        /// </summary>
        public static readonly uint DSSCL_EXCLUSIVE = 0x00000003;

        /// <summary>
        /// Sets the write-primary level. The application has write access to the primary buffer. No secondary buffers can be played. This level cannot be set if the DirectSound driver is being emulated for the device; that is, if the GetCaps method returns the DSCAPS_EMULDRIVER flag in the DSCAPS structure. 
        /// </summary>
        public static readonly uint DSSCL_WRITEPRIMARY = 0x00000004;
    }
}
