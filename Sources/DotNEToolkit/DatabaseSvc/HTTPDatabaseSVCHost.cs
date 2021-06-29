using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc
{
    /// <summary>
    /// 基于HttpListener的数据库服务
    /// </summary>
    public class HTTPDatabaseSVCHost : DatabaseSVCHost
    {
        private HttpListener listener;

        public override DatabaseSVCType Type => DatabaseSVCType.HttpListener;

        internal HTTPDatabaseSVCHost()
        { }

        public override int Initialize()
        {
            base.Initialize();

            this.listener = new HttpListener();
            string uri = string.Format("http://127.0.0.1:{0}/{1}/", this.port, this.rootPath);
            this.listener.Prefixes.Add(uri);
            return DotNETCode.SUCCESS;
        }

        public override int Start()
        {
            base.Start();
            this.listener.Start();
            Task.Factory.StartNew(this.ProcessHttpRequest);
            return DotNETCode.SUCCESS;
        }

        private void ProcessHttpRequest()
        {
            while (this.listener.IsListening)
            {
                HttpListenerContext context = this.listener.GetContext();

                HttpListenerRequest request = context.Request;

                DBClientRequest clientRequest = new DBClientRequest()
                {
                };

                this.ProcessRequest(clientRequest);
            }
        }
    }
}