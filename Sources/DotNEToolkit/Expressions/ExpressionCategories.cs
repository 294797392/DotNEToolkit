using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    public class ExpressionCategoryAttribute : Attribute
    {
        public string Name { get; set; }

        public ExpressionCategoryAttribute(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// 表达式类别
    /// </summary>
    public enum ExpressionCategories
    {
        [ExpressionCategory("字符串函数")]
        String = 0,

        [ExpressionCategory("集合操作函数")]
        Collection = 1,

        [ExpressionCategory("逻辑比较函数")]
        Comparison = 2,

        [ExpressionCategory("转换函数")]
        Converting = 4,

        [ExpressionCategory("数学函数")]
        Math = 5,

        [ExpressionCategory("日期和时间函数")]
        DateTime = 6,

        [ExpressionCategory("模块函数")]
        Module = 7,

        [ExpressionCategory("URI分析函数")]
        URI = 8,

        [ExpressionCategory("XML操作函数")]
        XML = 9,

        [ExpressionCategory("JSON操作函数")]
        JSON = 10,

        [ExpressionCategory("界面数据操作函数")]
        View = 11
    }
}
