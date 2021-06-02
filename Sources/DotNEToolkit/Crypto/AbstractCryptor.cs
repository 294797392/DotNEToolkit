using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Crypto
{
    /// <summary>
    /// 加解密抽象类
    /// </summary>
    public abstract class AbstractCryptor
    {
        /// <summary>
        /// 加密算法是否支持解密
        /// </summary>
        public abstract bool SupportDecrypto { get; }

        /// <summary>
        /// 加解密用到的Key
        /// </summary>
        public byte[] EncryptoKey { get; set; }

        /// <summary>
        /// 解密用到的Key
        /// </summary>
        public byte[] DecryptoKey { get; set; }

        /// <summary>
        /// 加密操作
        /// </summary>
        /// <returns></returns>
        public abstract byte[] Encrypto();

        /// <summary>
        /// 加密后的数据进行Base64处理
        /// </summary>
        /// <returns></returns>
        public string EncryptoBase64()
        {
            byte[] data = this.Encrypto();
            return Convert.ToBase64String(data, Base64FormattingOptions.None);
        }

        /// <summary>
        /// 解密操作
        /// </summary>
        /// <returns></returns>
        public abstract byte[] Decrypto(byte[] data);
    }
}