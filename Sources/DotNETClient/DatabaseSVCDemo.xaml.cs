using DotNEToolkit.DatabaseSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DotNETClient
{
    /// <summary>
    /// DatabaseSVCDemo.xaml 的交互逻辑
    /// </summary>
    public partial class DatabaseSVCDemo : Window
    {
        #region 字段

        private DatabaseSVCHost svchost;

        #endregion

        #region 属性

        #endregion

        #region 构造

        /// <summary>
        /// 构造方法
        /// </summary>
        public DatabaseSVCDemo()
        {
            this.InitializeComponent();
        }

        #endregion

        #region 方法

        #endregion

        private void ButtonStartService_Click(object sender, RoutedEventArgs e)
        {
            this.svchost = DatabaseSVCHost.Create(DatabaseSVCType.WCF);
            this.svchost.Initialize();
            this.svchost.Start();
        }
    }
}
