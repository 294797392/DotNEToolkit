using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 表示播放列表中的一项
    /// </summary>
    public class PlayItem
    {
        /// <summary>
        /// 媒体文件的ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 媒体文件名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 媒体文件的URI
        /// </summary>
        public string URI { get; set; }
    }
}
