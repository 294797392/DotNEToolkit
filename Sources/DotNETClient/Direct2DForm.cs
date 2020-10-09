using DotNEToolkit.DirectX.Direct2D;
using DotNEToolkit.Win32API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNETClient
{
    public partial class Direct2DForm : Form
    {
        public Direct2DForm()
        {
            InitializeComponent();

            GUID iid;
            int rc = OLE.IIDFromString(D2DInterfaceID.ID2D1Factory, out iid);

            IntPtr pFactory;
            rc = Direct2DNatives.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, ref iid, IntPtr.Zero, out pFactory);
        }
    }
}
