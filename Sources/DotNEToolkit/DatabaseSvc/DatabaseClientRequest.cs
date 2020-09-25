using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc
{
    /// <summary>
    /// 封装客户端请求的信息
    /// </summary>
    internal class DBClientRequest
    {
        /// <summary>
        /// 要查询的表名字
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 要查询的列名
        /// </summary>
        public List<string> ColumnNames { get; private set; }

        /// <summary>
        /// 查询条件
        /// </summary>
        public List<Condition> Conditions { get; private set; }

        public DBClientRequest()
        {
            this.ColumnNames = new List<string>();
        }
    }

    /// <summary>
    /// 封装QueryCondition
    /// </summary>
    internal class Condition
    {
        /// <summary>
        /// 查询条件列名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 查询条件操作符
        /// </summary>
        public int OperatorType { get; set; }

        /// <summary>
        /// 查询条件的值
        /// </summary>
        public string Value { get; set; }
    }
}