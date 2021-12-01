using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 存储要访问的成员信息
    /// </summary>
    public abstract class AccessMember
    {
        /// <summary>
        /// 要访问的成员名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public abstract MemberTypes Type { get; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Name, this.Type);
        }
    }

    /// <summary>
    /// 表示一个属性成员
    /// </summary>
    public class AccessMemberProperty : AccessMember
    {
        public override MemberTypes Type => MemberTypes.Property;
    }
}
