using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Bindings
{
    /// <summary>
    /// 表示一个属性可绑定的对象
    /// 可依赖注入的对象
    /// 
    /// 可以让普通的对象像WPF里的DependecyObject一样，对DependencyObject的属性进行绑定（WPF是在XAML里绑定）。
    /// 这样就不用每次都对属性赋值了，可以更专注于每个类的业务逻辑的开发，没有了很多属性的赋值操作，代码可读性也更强，同时也弱化了类与类之间的耦合性
    /// </summary>
    public abstract class BindableBase : Attribute
    {
    }
}


