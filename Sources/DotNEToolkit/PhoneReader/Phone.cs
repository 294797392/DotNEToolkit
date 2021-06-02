using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.PhoneReader
{
    public class Phone
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 运营商
        /// </summary>
        public string CurrentISP { get; set; }

        /// <summary>
        /// 原始运营商
        /// </summary>
        public string OriginalISP { get; set; }

        /// <summary>
        /// 归属地
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 是否是携号转网的号码
        /// </summary>
        public bool Transfered { get; set; }

        /// <summary>
        /// 是否是虚拟号码
        /// </summary>
        public bool IsVirtual { get; set; }

        public override string ToString()
        {
            return string.Format("号码 = {0}, 运营商 = {1}, 归属地 = {2}, 是否携号转网 = {3}, 是否是虚拟号码 = {4}", this.Number, this.CurrentISP, this.City, this.Transfered, this.IsVirtual);
        }
    }
}
