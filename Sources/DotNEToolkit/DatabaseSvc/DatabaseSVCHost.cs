using DotNEToolkit.DatabaseSvc.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.DatabaseSvc
{
    /// <summary>
    /// 承载服务的对象
    /// </summary>
    public abstract class DatabaseSVCHost
    {
        protected DatabaseSVConfig config;

        public virtual int Initialize()
        {
            foreach (string ns in this.config.Namespaces)
            {
                this.InitializeTypeInfo(ns);
            }

            return ResponseCode.Success;
        }

        public virtual int Release()
        {
            return ResponseCode.Success;
        }

        public virtual int Start()
        {
            return ResponseCode.Success;
        }

        public virtual int Stop()
        {
            return ResponseCode.Success;
        }

        protected void ProcessRequest(string path)
        {

        }

        #region 实例方法

        /// <summary>
        /// 获取一个命名空间下的所有表信息
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        private List<TableAttribute> LookupTables(string ns)
        {
            List<TableAttribute> result = new List<TableAttribute>();

            Assembly assembly = Assembly.Load(ns);

            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (!type.IsClass)
                {
                    continue;
                }

                TableAttribute attribute = type.GetCustomAttribute(typeof(TableAttribute), false) as TableAttribute;
                if (attribute == null)
                {
                    continue;
                }

                attribute.Columns = this.LookupColumns(type);
            }

            return result;
        }

        /// <summary>
        /// 找到一个类型里的所有的列名
        /// </summary>
        /// <param name="classType">类型</param>
        /// <returns></returns>
        private List<ColumnAttribute> LookupColumns(Type classType)
        {
            List<ColumnAttribute> result = new List<ColumnAttribute>();

            FieldInfo[] fields = classType.GetFields(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);
            if (fields == null || fields.Length == 0)
            {
                return result;
            }

            foreach (FieldInfo field in fields)
            {
                ColumnAttribute attribute = field.GetCustomAttribute(typeof(ColumnAttribute), false) as ColumnAttribute;
                if (attribute == null)
                {
                    // 不是列
                    continue;
                }

                result.Add(attribute);
            }

            return result;
        }

        #endregion
    }
}