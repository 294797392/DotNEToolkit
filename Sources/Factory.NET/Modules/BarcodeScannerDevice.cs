using DotNEToolkit.Modular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 条码扫描器设备
    /// </summary>
    public abstract class BarcodeScannerDevice : ModuleBase
    {
        /// <summary>
        /// 执行扫码操作
        /// </summary>
        /// <param name="barcodeList">扫描到的条码列表</param>
        /// <returns></returns>
        public abstract int Scan(out List<string> barcodeList);
    }
}
