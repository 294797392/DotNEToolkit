using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.DataAccess
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DBField : Attribute
    {
        public DBField()
        {
        }

        public DBField(string field)
        {
            this.FieldName = field;
        }


        public string FieldName
        {
            get;
            set;
        }

        /// <summary>
        /// 是否为自增列
        /// </summary>
        public bool IsAutoIncrement
        {
            get;
            set;
        }

        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsKey
        {
            get;
            set;
        }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class DBTable : Attribute
    {
        public DBTable()
        {
        }

        public DBTable(string tableName)
        {
            this.TableName = tableName;
        }

        public string TableName
        {
            get;
            set;
        }
    }
}
