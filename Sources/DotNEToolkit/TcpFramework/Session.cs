using System;
using System.Net.Sockets;

namespace DotNEToolkit.TcpFramework
{
    /// <summary> 
    /// �ͻ����������֮��ͨѶ�ĻỰ�ࡣ�ûỰ����Ҫ����ĳ��Socket��������
    /// �����ְ������������ȵȡ�
    /// </summary> 
    internal sealed class Session
    {
        /// <summary>
        /// �������ͨѶ�Ự��Session�м�¼��TCP Message����ʱ��Ҫ�����һЩ�м��������TCP Message��
        /// ͷ������Messageʹ�õĻ�������ʣ��message�ĳ��ȵȵȡ�Ϊ�˼����ڴ���Ƭ��Session�ڽ���TCP 
        /// Messageͷʱֱ��ʹ��BufferManagerԤ�ȷ���Ĵ���ڴ档Ȼ���ٸ���TCP Messageͷ�ж����message
        /// ���ȣ�����̬������һ����Ҫ���յ����ݴ�С��Socket���ջ�������������Ϻ�ֱ�ӽ����������ݸ��ϲ�Ӧ�ã����Ա���
        /// �ڴ�ĸ��ơ�
        /// </summary>
        /// <param name="BufferOffset">sessionʹ�õĻ�����ƫ��</param>
        /// <param name="buffer">���ջ�����</param>
        internal Session(byte[] buffer, int bufferOffset)
        {
            this.ReceiveBuffer = buffer;
            this.BufferOffset = bufferOffset;

            this.Expected = 0;
            this.DataInBuffer = 0;
            this.Remaining = 0;
            this.PackageIndex = 0;
            this.EmptyMsgCount = 0;
        }

        /// <summary> 
        /// ���ػỰ��ID. С��0 ��Ϊδ����
        /// </summary>
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// Զ�˽���㣬���ڵ��Ի���ʾ
        /// </summary>
        public string RemoteEndPoint
        {
            get;
            private set;
        }

        /// <summary> 
        /// �����ͻ��˻Ự������Socket���� 
        /// </summary> 
        public Socket ClientSocket
        {
            get;
            private set;
        }

        /// <summary>
        /// ���δ���ı��Ľ�����ʣ����ֽ���
        /// </summary>
        public int Remaining
        {
            get;
            set;
        }

        /// <summary>
        /// ��һ�ν������������ݴ�С
        /// </summary>
        public int Expected
        {
            get;
            set;
        }

        /// <summary>
        /// ��ǰbuffer�ﱣ������ݳ���
        /// </summary>
        public int DataInBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// ���һ��message����һ�ν��ղ��ꡣ
        /// ���ֶμ�¼�Ѿ����յĸ�message�������ص��Ĵ���
        /// </summary>
        public int PackageIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Message������
        /// </summary>
        public byte[] ReceiveBuffer
        {
            get;
            private set;
        }

        /// <summary>
        /// �ڽ�����һ������ʱ��Ӧ�ôӸ�ƫ��λ�ñ�����յ������ݡ�
        /// </summary>
        public int BufferOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// ��¼�����յ��Ŀ���Ϣ�¼���¼. �ںܶ�����£���Զ��socket�Ͽ�ʱ��
        /// �첽���պ��������յ�ConnectionReset���󡣶���ֱ�ӷ��ؿ���Ϣ���������������n�ν��յ�����Ϣ��
        /// ��ô������ΪԶ��Socket�Ѿ��Ͽ���
        /// </summary>
        public int EmptyMsgCount
        {
            get;
            set;
        }

        /// <summary>
        /// ����Session������Socket
        /// </summary>
        /// <param name="cliSocket"></param>
        public void SetSocket(Socket cliSocket)
        {
            this.ClientSocket = cliSocket;
            this.ID = cliSocket.Handle.ToInt32();
            this.RemoteEndPoint = cliSocket.RemoteEndPoint.ToString();
        }

        /// <summary> 
        /// �رջỰ 
        /// </summary> 
        public void Close()
        {
            if (this.ClientSocket != null)
            {
                try
                {
                    //�ر����ݵĽ��ܺͷ��� 
                    this.ClientSocket.Shutdown(SocketShutdown.Both);

                    //������Դ 
                    this.ClientSocket.Close();
                }
                catch 
                {

                }
                finally
                {
                    this.ClientSocket = null;

                    this.ID = 0;
                    this.RemoteEndPoint = string.Empty;

                    //this.ReceiveBuffer = null;
                    //this.BufferOffset = 0;
                    this.Expected = 0;
                    this.Remaining = 0;

                    this.EmptyMsgCount = 0;                    
                }
            }
        }

        #region override

        /// <summary> 
        /// ʹ��Socket�����Handleֵ��ΪHashCode,���������õ���������. 
        /// </summary> 
        /// <returns></returns> 
        public override int GetHashCode()
        {
            return this.ID;
        }

        /// <summary> 
        /// ��������Session�Ƿ����ͬһ���ͻ��� 
        /// </summary> 
        /// <param name="obj"></param> 
        /// <returns></returns> 
        public override bool Equals(object obj)
        {
            Session rightObj = (Session)obj;

            return this.ID == (int)rightObj.ID;

        }

        /// <summary> 
        /// ����ToString()����,����Session��������� 
        /// </summary> 
        /// <returns></returns> 
        public override string ToString()
        {
            return string.Format("Session{0}({1})",
                this.ID, this.ClientSocket.RemoteEndPoint);
        }

        #endregion
    }
}