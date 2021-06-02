namespace DotNEToolkit.DataAccess
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class DBManager 
    {
        public DBManager(string connectionString, ProviderType providerType)
        {
            this.ConnectionString = connectionString;
            this.DbType = providerType;
            this.CommandTimeout = 0;
        }

        public DBManager(string connectionString, ProviderType providerType, int commandTimeout)
        {
            this.ConnectionString = connectionString;
            this.DbType = providerType;
            this.CommandTimeout = commandTimeout;
        }

        public string ConnectionString { get; set; }

        public ProviderType DbType { get; set; }

        /// <summary>
        /// 命令执行的缺省超时时间
        /// </summary>
        public int CommandTimeout { get; set; }


        public bool TestConnection()
        {
            DbConnection connection = null;

            try
            {
                connection = DbHelper.CreateDbConnection(this.ConnectionString, this.DbType);
                return true;
            }
            catch
            {
            }
            finally
            {
                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }

                    connection.Dispose();
                }
            }

            return false;
        }

        #region parameters

        /// <summary>
        /// 产生以@0, @1, @2...的一组DB变量
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public DbParameter[] FillDbParameters(params object[] args)
        {
            DbParameter[] result = CreateDbParameters(args.Length);

            for (int i = 0; i < result.Length; i++)
            {
                result[i].ParameterName = i.ToString();
                result[i].Value = args[i];
            }

            return result;
        }

        public DbParameter[] CreateDbParameters(int count)
        {
            return DbHelper.CreateDbParameters(this.DbType, count);
        }

        public DbParameter CreateDbParameter()
        {
            return DbHelper.CreateDbParameter(this.DbType);
        }

        public DbParameter CreateDbParameter(string parameterName, object value)
        {
            DbParameter param = DbHelper.CreateDbParameter(this.DbType);
            param.ParameterName = parameterName;
            param.Value = value;
            return param;
        }

        public DbParameter CreateDbParameter(string parameterName, DbType dataType, object value)
        {
            DbParameter param = DbHelper.CreateDbParameter(this.DbType);
            param.ParameterName = parameterName;
            param.DbType = dataType;
            param.Value = value;
            return param;
        }

        #endregion

        #region Transaction

        public DbTransaction BeginTransaction()
        {
            return DbHelper.CreateDbTransaction(this.ConnectionString, this.DbType);
        }

        public void CommitTransaction(DbTransaction transaction)
        {
            DbHelper.CommitTransaction(transaction);
        }

        public void RollbackTransaction(DbTransaction transaction)
        {
            DbHelper.RollbackTransaction(transaction);
        }

        #endregion

        #region ExecuteReader

        public DbDataReader ExecuteReader(string query)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DbDataReader ExecuteReader(int commandTimeout, string query)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public DbDataReader ExecuteReader(string query, CommandType commandtype)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DbDataReader ExecuteReader(int commandTimeout, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, commandTimeout, CommandType.Text, query);
        }



        public DbDataReader ExecuteReader(string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, this.CommandTimeout, commandType, commandText, commandParameters);
        }

        public DbDataReader ExecuteReader(int commandTimeout, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, commandTimeout, commandType, commandText, commandParameters);
        }


        public DbDataReader ExecuteReader(string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        public DbDataReader ExecuteReader(int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteReader(this.ConnectionString, this.DbType, commandTimeout, spName, parameterValues);
        }


        public DbDataReader ExecuteReader(DbTransaction transaction, string query)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DbDataReader ExecuteReader(DbTransaction transaction, int commandTimeout, string query)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public DbDataReader ExecuteReader(DbTransaction transaction, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DbDataReader ExecuteReader(DbTransaction transaction, int commandTimeout, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public DbDataReader ExecuteReader(DbTransaction transaction, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, this.CommandTimeout, commandType, commandText, commandParameters);
        }

        public DbDataReader ExecuteReader(DbTransaction transaction, int commandTimeout, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, commandTimeout, commandType, commandText, commandParameters);
        }


        public DbDataReader ExecuteReader(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        public DbDataReader ExecuteReader(DbTransaction transaction, int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteReader(transaction, this.DbType, commandTimeout, spName, parameterValues);
        }

        #endregion

        #region ExecuteScalar

        public object ExecuteScalar(string query)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public object ExecuteScalar(int commandTimeout, string query)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public object ExecuteScalar(string query, CommandType commandtype)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public object ExecuteScalar(int commandTimeout, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public object ExecuteScalar(string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, this.CommandTimeout, commandType, commandText, commandParameters);
        }

        public object ExecuteScalar(int commandTimeout, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, commandTimeout, commandType, commandText, commandParameters);
        }


        public object ExecuteScalar(string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        public object ExecuteScalar(int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteScalar(this.ConnectionString, this.DbType, commandTimeout, spName, parameterValues);
        }


        public object ExecuteScalar(DbTransaction transaction, string query)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public object ExecuteScalar(DbTransaction transaction, int commandTimeout, string query)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public object ExecuteScalar(DbTransaction transaction, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public object ExecuteScalar(DbTransaction transaction, int commandTimeout, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public object ExecuteScalar(DbTransaction transaction, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, this.CommandTimeout, commandType, commandText, commandParameters);
        }

        public object ExecuteScalar(DbTransaction transaction, int commandTimeout, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, commandTimeout, commandType, commandText, commandParameters);
        }


        public object ExecuteScalar(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        public object ExecuteScalar(DbTransaction transaction, int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteScalar(transaction, this.DbType, commandTimeout, spName, parameterValues);
        }

        #endregion

        #region ExecuteDataSet

        public DataSet ExecuteDataSet(string query)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DataSet ExecuteDataSet(int commandTimeout, string query)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public DataSet ExecuteDataSet(string query, CommandType commandtype)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DataSet ExecuteDataSet(int commandTimeout, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public DataSet ExecuteDataSet(string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, this.CommandTimeout, commandType, commandText, commandParameters);
        }

        public DataSet ExecuteDataSet(int commandTimeout, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, commandTimeout, commandType, commandText, commandParameters);
        }


        public DataSet ExecuteDataSet(string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        public DataSet ExecuteDataSet(int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteDataset(this.ConnectionString, this.DbType, commandTimeout, spName, parameterValues);
        }


        public DataSet ExecuteDataSet(DbTransaction transaction, string query)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DataSet ExecuteDataSet(DbTransaction transaction, int commandTimeout, string query)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public DataSet ExecuteDataSet(DbTransaction transaction, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, this.CommandTimeout, CommandType.Text, query);
        }

        public DataSet ExecuteDataSet(DbTransaction transaction, int commandTimeout, string query, CommandType commandtype)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, commandTimeout, CommandType.Text, query);
        }


        public DataSet ExecuteDataSet(DbTransaction transaction, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, this.CommandTimeout, commandType, commandText, commandParameters);
        }

        public DataSet ExecuteDataSet(DbTransaction transaction, int commandTimeout, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, commandTimeout, commandType, commandText, commandParameters);
        }


        public DataSet ExecuteDataSet(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        public DataSet ExecuteDataSet(DbTransaction transaction, int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteDataset(transaction, this.DbType, commandTimeout, spName, parameterValues);
        }

        #endregion

        #region ExecuteNonQuery

        public int ExecuteNonQuery(string commandText, CommandType commandtype)
        {
            return DbHelper.ExecuteNonQuery(this.ConnectionString, this.DbType, this.CommandTimeout, commandtype, commandText);
        }

        public int ExecuteNonQuery(int commandTimeout, string commandText, CommandType commandtype)
        {
            return DbHelper.ExecuteNonQuery(this.ConnectionString, this.DbType, commandTimeout, commandtype, commandText);
        }


        public int ExecuteNonQuery(string commandText, CommandType commandtype, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteNonQuery(this.ConnectionString, this.DbType, this.CommandTimeout, commandtype, commandText, commandParameters);
        }

        public int ExecuteNonQuery(int commandTimeout, string commandText, CommandType commandtype, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteNonQuery(this.ConnectionString, this.DbType, commandTimeout, commandtype, commandText, commandParameters);
        }


        public int ExecuteNonQuery(string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteNonQuery(this.ConnectionString, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        public int ExecuteNonQuery(int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteNonQuery(this.ConnectionString, this.DbType, commandTimeout, spName, parameterValues);
        }

        public int ExecuteNonQuery(DbTransaction transaction, int commandTimeout, string commandText, CommandType commandType)
        {
            return DbHelper.ExecuteNonQuery(transaction, this.DbType, commandTimeout, commandType, commandText);
        }

        public int ExecuteNonQuery(DbTransaction transaction, string commandText, CommandType commandType)
        {
            return DbHelper.ExecuteNonQuery(transaction, this.DbType, this.CommandTimeout, commandType, commandText);
        }


        public int ExecuteNonQuery(DbTransaction transaction, int commandTimeout, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteNonQuery(transaction, this.DbType, commandTimeout, commandType, commandText, commandParameters);
        }

        public int ExecuteNonQuery(DbTransaction transaction, string commandText, CommandType commandType, params DbParameter[] commandParameters)
        {
            return DbHelper.ExecuteNonQuery(transaction, this.DbType, this.CommandTimeout, commandType, commandText, commandParameters);
        }


        public int ExecuteNonQuery(DbTransaction transaction, int commandTimeout, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteNonQuery(transaction, this.DbType, commandTimeout, spName, parameterValues);
        }

        public int ExecuteNonQuery(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DbHelper.ExecuteNonQuery(transaction, this.DbType, this.CommandTimeout, spName, parameterValues);
        }

        #endregion
    }
}
