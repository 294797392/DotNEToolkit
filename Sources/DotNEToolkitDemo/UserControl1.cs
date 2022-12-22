using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNEToolkitDemo
{
    public partial class UserControl1 : Label
    {
        public UserControl1()
        {
            InitializeComponent();

  //          SetStyle(ControlStyles.SupportsTransparentBackColor
  //| ControlStyles.UserPaint
  //| ControlStyles.AllPaintingInWmPaint
  //| ControlStyles.Opaque, true);
  //          this.BackColor = Color.Transparent;

            this.Width = 3000;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                //return base.CreateParams;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT 
                return cp;
            }
        }
    }
}
