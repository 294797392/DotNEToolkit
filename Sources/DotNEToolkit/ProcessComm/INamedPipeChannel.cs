using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DotNEToolkit.ProcessComm
{
    [ServiceContract]
    public interface INamedPipeChannel
    {
        /// <summary>
        /// 向对方发送消息
        /// </summary>
        /// <param name="cmdParam">要发送的消息</param>
        [OperationContract]
        void SendBytes(int cmdType, byte[] cmdParam);

        [OperationContract]
        void SendString(int cmdType, string cmdParam);

        [OperationContract]
        void SendObject(int cmdType, object cmdParam);
    }

    /// <summary>
    /// 客户端访问服务器使用的接口
    /// </summary>
    [ServiceContract]
    public interface INamedPipeServiceChannel : INamedPipeChannel
    {
        /// <summary>
        /// 客户端连接服务端
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        int Connect();

        /// <summary>
        /// 客户端断开与服务端的连接
        /// </summary>
        [OperationContract]
        void Disconnect();
    }

    /// <summary>
    /// 服务器访问客户端使用的接口
    /// </summary>
    [ServiceContract]
    public interface INamedPipeClientChannel : INamedPipeChannel
    {
    }
}

