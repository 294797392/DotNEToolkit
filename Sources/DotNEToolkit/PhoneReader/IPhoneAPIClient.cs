using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.PhoneReader
{
    public enum PhoneData
    {
        /// <summary>
        /// 归属地
        /// </summary>
        City = 1,

        /// <summary>
        /// 运营商
        /// </summary>
        OrignalISP = 2,

        /// <summary>
        /// 携号转网后的运营商
        /// </summary>
        CurrentISP = 4,

        /// <summary>
        /// 是否是虚拟号码
        /// </summary>
        IsVirtual = 8,

        /// <summary>
        /// 是否携号转网
        /// </summary>
        Transfered = 16
    }

    public interface IPhoneAPIClient
    {
        /// <summary>
        /// 查询手机的归属地和运营商
        /// </summary>
        /// <param name="number">手机号</param>
        /// <param name="city">归属地</param>
        /// <param name="company">运营商</param>
        /// <returns>返回剩余要查询的数据</returns>
        PhoneData Query(string number, PhoneData data, Dictionary<PhoneData, string> values);
    }
}
