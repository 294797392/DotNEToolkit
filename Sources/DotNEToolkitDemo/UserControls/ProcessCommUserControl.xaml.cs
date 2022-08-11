using DotNEToolkit.ProcessComm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DotNEToolkitDemo.UserControls
{
    /// <summary>
    /// ProcessCommUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessCommUserControl : UserControl
    {
        private ProcessCommClient commClient;
        private ProcessCommSvc commSvc;

        public ProcessCommUserControl()
        {
            InitializeComponent();
        }

        private void ButtonStartServiceProcess_Click(object sender, RoutedEventArgs e)
        {
            this.commSvc = ProcessCommFactory.CreateSvc(ProcessCommTypes.WCFNamedPipe);
            this.commSvc.DataReceived += CommSvc_DataReceived;
            this.commSvc.URI = TextBoxURI.Text;
            this.commSvc.Initialize();
            this.commSvc.Start();
        }

        private void CommSvc_DataReceived(ProcessCommObject commClient, int cmdType, object cmdParam)
        {
            this.AppendMessage("服务端收到消息:{0}", cmdParam);
        }

        private void ButtonStartClientProcess_Click(object sender, RoutedEventArgs e)
        {
            this.commClient = ProcessCommFactory.CreateClient(ProcessCommTypes.WCFNamedPipe);
            this.commClient.ServiceURI = TextBoxURI.Text;
            this.commClient.StatusChanged += CommClient_StatusChanged;
            this.commClient.DataReceived += CommClient_DataReceived;
            this.commClient.Initialize();
            this.commClient.Connect();
        }

        private void CommClient_DataReceived(ProcessCommObject commClient, int cmdType, object cmdParam)
        {
            this.AppendMessage("客户端收到消息:{0}", cmdParam);
        }

        private void CommClient_StatusChanged(ProcessCommClient commClient, CommStates status)
        {
            this.AppendMessage("客户端状态发生改变:{0}", status);
        }

        private void AppendMessage(string message, params object[] param)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                string msg = string.Format(message, param);
                TextBoxMessage.AppendText(msg + Environment.NewLine);
                ScrollViewerMessage.ScrollToEnd();
            }));
        }

        private void ButtonClient2Service_Click(object sender, RoutedEventArgs e)
        {
            //byte[] data = Encoding.ASCII.GetBytes(TextBoxData.Text);
            this.commClient.Send(1, TextBoxData.Text);
        }

        private void ButtonService2Client_Click(object sender, RoutedEventArgs e)
        {
            //byte[] data = Encoding.ASCII.GetBytes(TextBoxData.Text);
            this.commSvc.Send(1, TextBoxData.Text);
        }
    }
}
