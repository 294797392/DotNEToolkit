using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Media
{
    /// <summary>
    /// 封装播放器列表逻辑
    /// </summary>
    public class Playlist
    {
        #region 实例变量

        /// <summary>
        /// 当前播放的项的索引
        /// </summary>
        private int playIndex;

        #endregion

        #region 实例变量

        public List<PlayItem> PlayItems { get; private set; }

        #endregion

        #region 构造方法

        public Playlist()
        {
            this.PlayItems = new List<PlayItem>();
        }

        #endregion

        #region 公开接口

        public void AddItem(PlayItem playItem)
        {

        }

        public void DeleteItem(PlayItem playItem)
        { }

        /// <summary>
        /// 获取下一个播放项目
        /// </summary>
        public PlayItem GetNextItem()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取上一个播放项目
        /// </summary>
        public PlayItem GetPreviousItem()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
