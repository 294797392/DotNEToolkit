using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 处理模块事件的委托
    /// </summary>
    /// <param name="sender">发布该事件的模块实例</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    public delegate void ModuleEventDlg(IModuleInstance sender, int eventType, object eventData);
}
