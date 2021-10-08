using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DotNEToolkit.Android
{
    public partial class WatcherForm : Form
    {
        internal IDeviceNotificationHandler Handler { get; set; }

        public WatcherForm()
        {
            InitializeComponent();
            
            this.Visible = false;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case WinUser.WM_DEVICECHANGE:
                    {
                        switch (m.WParam.ToInt32())
                        {
                            case Dbt.DBT_DEVICEARRIVAL:
                                {
                                    break;
                                }

                            case Dbt.DBT_DEVICEREMOVECOMPLETE:
                                {
                                    break;
                                }

                            case Dbt.DBT_DEVNODES_CHANGED:
                                {
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }

                        break;
                    }
            }
        }
    }
}
