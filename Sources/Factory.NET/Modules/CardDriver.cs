using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET.Modules
{
    /// <summary>
    /// 封装采集卡接口
    /// </summary>
    public abstract class CardDriver
    {
        /// <summary>
        /// 采集卡底下的相机列表
        /// </summary>
        public List<CameraDriver> CameraList { get; private set; }

        public int Initialize()
        {
            this.CameraList = new List<CameraDriver>();

            return this.OnInitialize();
        }

        public void Release()
        {
            this.OnRelease();
        }

        public abstract int OnInitialize();

        public abstract void OnRelease();
    }
}
