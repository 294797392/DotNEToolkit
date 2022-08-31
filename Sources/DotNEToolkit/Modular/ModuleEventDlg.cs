using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 保存事件参数
    /// </summary>
    public interface IEventArgs
    {

    }

    /// <summary>
    /// 处理模块事件的委托
    /// </summary>
    /// <param name="sender">发布该事件的模块实例</param>
    /// <param name="eventArgs">事件参数</param>
    public delegate int ModuleEventDlg(IModuleInstance sender, IEventArgs eventArgs);
}
