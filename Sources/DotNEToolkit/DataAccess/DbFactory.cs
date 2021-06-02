using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace DotNEToolkit.DataAccess
{
    public enum ProviderType
    {
        Unknown = 0,

        SqlServer = 1,
        OleDb = 2,
        Oracle = 3,
        ODBC = 4,
        MySql = 5,
        SqlServerCompact = 6,

        Sqlite = 7,

        ConfigDefined = 10240
    }

    public class DbFactory
    {
        #region 常量

        private const string SqlServerCeProviderType = "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe";
        private const string SqlServerCeAdapterType = "System.Data.SqlServerCe.SqlCeDataAdapter, System.Data.SqlServerCe";

        private const string MySqlProviderType = "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";
        private const string MySqlAdapterType = "MySql.Data.MySqlClient.MySqlDataAdapter, MySql.Data";

        private const string OracleProviderType = "Oracle.DataAccess.Client.OracleClientFactory, Oracle.DataAccess";
        private const string OracleAdapterType = "Oracle.DataAccess.Client.OracleDataAdapter, Oracle.DataAccess";

        private const string SqliteProviderType = "System.Data.SQLite.SQLiteFactory, System.Data.SQLite";
        private const string SqliteAdapterType = "System.Data.SQLite.SQLiteDataAdapter, System.Data.SQLite";

        #endregion

        #region 静态变量

        /// <summary>
        /// SqlServerCe Provider缓存实例
        /// </summary>
        private static DbProviderFactory sqlServerCeProviderInst;
        private static Type sqlServerCeAdapterType;

        /// <summary>
        /// Oracle Provider缓存实例
        /// </summary>
        private static DbProviderFactory oracleProviderInst;
        private static Type oracleAdapterType;

        /// <summary>
        /// MySql Provider缓存实例
        /// </summary>
        private static DbProviderFactory mysqlProviderInst;
        private static Type mysqlAdapterType;

        /// <summary>
        /// Sqlite Provider缓存实例
        /// </summary>
        private static DbProviderFactory sqliteProviderInst;
        private static Type sqliteAdapterType;      

        #endregion

        public static DbProviderFactory GetProvider(ProviderType provider)
        {
            switch (provider)
            {
                case ProviderType.SqlServerCompact:
                    if (sqlServerCeProviderInst == null)
                    {
                        Type type = Type.GetType(SqlServerCeProviderType, true);
                        sqlServerCeProviderInst = type.GetField("Instance").GetValue(null) as DbProviderFactory;
                    }

                    return sqlServerCeProviderInst;

                case ProviderType.SqlServer:
                    return SqlClientFactory.Instance;

                case ProviderType.OleDb:
                    return OleDbFactory.Instance;

                case ProviderType.Oracle:
                    if (oracleProviderInst == null)
                    {
                        Type type = Type.GetType(OracleProviderType, true);
                        oracleProviderInst = type.GetField("Instance").GetValue(null) as DbProviderFactory;
                    }

                    return oracleProviderInst;

                case ProviderType.Sqlite:
                    if (sqliteProviderInst == null)
                    {
                        Type type = Type.GetType(SqliteProviderType, true);
                        sqliteProviderInst = type.GetField("Instance").GetValue(null) as DbProviderFactory;
                    }

                    return sqliteProviderInst;

                case ProviderType.MySql:
                    if (mysqlProviderInst == null)
                    {
                        Type type = Type.GetType(MySqlProviderType, true);
                        mysqlProviderInst = type.GetField("Instance").GetValue(null) as DbProviderFactory;
                    }

                    return mysqlProviderInst;

                case ProviderType.ODBC:
                    return OdbcFactory.Instance;

                default:
                    return null;
            }
        }

        public static DbDataAdapter GetDataAdapter(ProviderType providerType)
        {

            switch (providerType)
            {
                case ProviderType.SqlServerCompact:
                    if (sqlServerCeAdapterType == null)
                    {
                        sqlServerCeAdapterType = Type.GetType(SqlServerCeAdapterType, true);
                    }

                    return (DbDataAdapter)Activator.CreateInstance(sqlServerCeAdapterType);

                case ProviderType.SqlServer:
                    return new SqlDataAdapter();

                case ProviderType.MySql:
                    if (mysqlAdapterType == null)
                    {
                        mysqlAdapterType = Type.GetType(MySqlAdapterType, true);
                    }

                    return (DbDataAdapter)Activator.CreateInstance(mysqlAdapterType);

                case ProviderType.OleDb:
                    return new OleDbDataAdapter();

                case ProviderType.ODBC:
                    return new OdbcDataAdapter();

                case ProviderType.Oracle:
                    if (oracleAdapterType == null)
                    {
                        oracleAdapterType = Type.GetType(OracleAdapterType, true);
                    }

                    return (DbDataAdapter)Activator.CreateInstance(oracleAdapterType);

                case ProviderType.Sqlite:
                    if (sqliteAdapterType == null)
                    {
                        sqliteAdapterType = Type.GetType(SqliteAdapterType, true);
                    }

                    return (DbDataAdapter)Activator.CreateInstance(sqliteAdapterType);

                default:
                    return null;
            }

        }
    }
}
