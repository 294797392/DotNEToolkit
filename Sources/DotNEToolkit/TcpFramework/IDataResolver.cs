using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.TcpFramework
{
    /// <summary>
    /// TCP包头解析器。 
    /// 
    /// TcpFramework假设每一次的TCP通讯总是以报文为单位。报文由报文头+报文内容组成。
    /// 其中报文头的格式是固定，且长度固定。报文内容可以是不定长的，且报文头里总是包含报文内容长度信息。
    /// 
    /// TcpFramework总是用固定长度的报文头缓冲（例如固定的20个字节）接收报文头。从而确定接下来
    /// 需要接收的报文内容的长度。然后根据该长度来安排接下来的接收过程。接收事件的触发也总是尽量以接收到
    /// 该次报文的所有字节后再触发（当报文内容长度大于最大缓冲大小时，同一个报文可能会触发多次接收事件）。
    /// 因此TcpFramework的接收数据的过程总是周而复始的执行  
    ///   1. 接收报文头  
    ///   2. 接收报文内容(1次或多次)...
    /// 
    /// 在处理报文头的时候有2种情况：
    ///   1. TcpFramework收到了正确的报文头。那么IDataResolver只需要根据约定的报文头格式解析出里面包含的报文长度即可
    ///   2. 接收过程由于某种原因，收到的报文头发生了错位。一旦发生了错位，接下来所有正常的接收过程都会被打乱。
    ///      因此TcpFramework需要用某种方式来让接收过程重现回到正确的步骤上。这里有我们将采取断开客户端连接的方式解决
    ///    
    /// IDataResolver接口所提供的主要功能就是
    ///   1. 提供方法来验证报文头的正确性
    ///   2. 确定报文内容的长度。
    /// </summary>
    public interface IDataResolver
    {
        /// <summary>
        /// 解析TCP报文头. 
        /// 根据业务逻辑所定义的TCP报文头格式来解析本次接收中接下来需要接收的TCP报文内容的长度
        /// </summary>
        /// <param name="headerBuffer">报文头缓冲</param>
        /// <param name="offset">本次分析的报文头的偏移</param>
        /// <returns>接下来需要接收报文的大小(整个报文减去报文头的长度)，返回小于0 说明接收到的报文头无效</returns>
        int ResolveHeader(byte[] headerBuffer, int offset);

        /// <summary>
        /// 报文头的大小
        /// </summary>
        int HeaderSize { get; }
    }
}
