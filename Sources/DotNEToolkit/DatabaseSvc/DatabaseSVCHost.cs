using DotNEToolkit.DatabaseSvc.Attributes;
using DotNEToolkit.DatabaseSvc.Internals;
using DotNEToolkit.Extentions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
        private const string AppSettingKey = "DatabaseSVConfig";

        private const string DefaultConfigName = "DatabaseSvc.json";

        private const int DefaultPort = 1018;

        private const string DefaultRootPath = "svc";

        #region 实例变量

        private DatabaseSVConfig config;

        protected int port;
        protected string rootPath;

        internal List<InternalTable> tables;

        #endregion

        #region 属性

        public abstract DatabaseSVCType Type { get; }

        #endregion

        #region 构造方法

        internal DatabaseSVCHost()
        {

        }

        #endregion

        #region 公开接口

        public virtual int Initialize()
        {
            this.config = JSONHelper.ParseFile<DatabaseSVConfig>(this.GetConfigPath());
            if (this.config == null)
            {
                return DotNETCode.LoadConfigFailed;
            }

            this.port = this.config.ServiceConfig.GetValue<int>("port", DefaultPort);
            this.rootPath = this.config.ServiceConfig.GetValue<string>("root_path", DefaultRootPath);

            return DotNETCode.Success;
        }

        public virtual int Release()
        {
            return DotNETCode.Success;
        }

        public virtual int Start()
        {
            return DotNETCode.Success;
        }

        public virtual int Stop()
        {
            return DotNETCode.Success;
        }

        public static DatabaseSVCHost Create(DatabaseSVCType type)
        {
            switch (type)
            {
                case DatabaseSVCType.HttpListener: return new HTTPDatabaseSVCHost();
                case DatabaseSVCType.WCF: return new WCFDatabaseSVCHost();
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region 实例方法

        internal void ProcessRequest(DBClientRequest request)
        {
            InternalTable table = this.tables.FirstOrDefault(t => t.Name == request.TableName);

            //string path = this.config.PathMapping.TryGetValue(request.Path, out path) ? path : request.Path;
        }

        private string GetConfigPath()
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(AppSettingKey))
            {
                // 如果在App.config里配置了路径，那么使用App.config里的路径
                return ConfigurationManager.AppSettings[AppSettingKey];
            }
            else
            {
                // 没配置就使用默认的
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultConfigName);
            }
        }

        /// <summary>
        /// 获取一个命名空间下的所有表信息
        /// </summary>
        /// <param name="namespaceName"></param>
        /// <returns></returns>
        private List<InternalTable> LookupTables(string namespaceName)
        {
            List<InternalTable> result = new List<InternalTable>();

            Assembly assembly = Assembly.Load(namespaceName);

            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (!type.IsClass)
                {
                    continue;
                }

                //TableAttribute attribute = type.GetCustomAttribute(typeof(TableAttribute), false) as TableAttribute;
                //if (attribute == null)
                //{
                //    continue;
                //}

                //InternalTable table = new InternalTable()
                //{
                //    Columns = this.LookupColumns(type),
                //    ModelType = type,
                //    Name = attribute.Name
                //};

                //result.Add(table);
            }

            return result;
        }

        /// <summary>
        /// 找到一个类型里的所有的列信息
        /// </summary>
        /// <param name="classType">类型</param>
        /// <returns></returns>
        private List<InternalColumn> LookupColumns(Type classType)
        {
            List<InternalColumn> result = new List<InternalColumn>();

            FieldInfo[] fields = classType.GetFields(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);
            if (fields == null || fields.Length == 0)
            {
                return result;
            }

            foreach (FieldInfo field in fields)
            {
                //ColumnAttribute attribute = field.GetCustomAttribute(typeof(ColumnAttribute), false) as ColumnAttribute;
                //if (attribute == null)
                //{
                //    // 不是列
                //    continue;
                //}

                //InternalColumn column = new InternalColumn()
                //{
                //    Name = attribute.Name
                //};

                //result.Add(column);
            }

            return result;
        }

        #endregion
    }
}