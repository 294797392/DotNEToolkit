using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using System.Data.Common;

namespace DotNEToolkit.DataAccess
{
    public class AccessBase 
    {
        #region 公开接口

        #region Get Data

        /// <summary>
        /// Sql参数必须以@0, @1，@2等方式写出，args次序需和sql中的一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbManager"></param>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IList<T> GetModels<T>(DBManager dbManager, string sql, CommandType commandType, params object[] args)
        {
            DbParameter[] dbParams = dbManager.CreateDbParameters(args.Length);
            for (int i = 0; i < dbParams.Length; i++)
            {
                dbParams[i].ParameterName = "@" + i;
                dbParams[i].Value = args[i];
            }

            DataSet ds = dbManager.ExecuteDataSet(sql, CommandType.Text, dbParams);
            return GetModels<T>(ds.Tables[0]);
        }

        public static IList<T> GetModels<T>(DataSet ds)
        {
            Type type = typeof(T);
            string tableName = GetTableName(type);

            foreach (DataTable table in ds.Tables)
            {
                if (string.Compare(table.TableName, tableName) == 0)
                {
                    return GetModels<T>(table);
                }
            }

            return new List<T>();
        }

        public static IList<T> GetModels<T>(DataTable table)
        {
            List<T> result = new List<T>();

            if (table.Rows.Count > 0)
            {
                Type type = typeof(T);

                foreach (DataRow row in table.Rows)
                {
                    result.Add(Row2Model<T>(row, type));
                }
            }

            return result;
        }

        /// <summary>
        /// Sql参数必须以@0, @1，@2等方式写出，args次序需和sql中的一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbManager"></param>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T GetModel<T>(DBManager dbManager, string sql, CommandType commandType, params object[] args)
        {
            DbParameter[] dbParams = dbManager.CreateDbParameters(args.Length);
            for (int i = 0; i < dbParams.Length; i++)
            {
                dbParams[i].ParameterName = "@" + i;
                dbParams[i].Value = args[i];
            }

            DataSet ds = dbManager.ExecuteDataSet(sql, CommandType.Text, dbParams);
            return GetModel<T>(ds.Tables[0]);
        }

        public static T GetModel<T>(DataSet ds)
        {
            Type type = typeof(T);
            string tableName = GetTableName(type);

            foreach (DataTable table in ds.Tables)
            {
                if (string.Compare(table.TableName, tableName) == 0)
                {
                    return GetModel<T>(table);
                }
            }

            return default(T);
        }

        public static T GetModel<T>(DataTable table)
        {
            return table.Rows.Count > 0 ? Row2Model<T>(table.Rows[0], typeof(T)) : default(T);
        }

        public static T Row2Model<T>(DataRow row, Type type)
        {
            T targetObj = Activator.CreateInstance<T>();

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                DBField dbField = Attribute.GetCustomAttribute(propertyInfo, typeof(DBField)) as DBField;
                if (dbField != null)
                {
                    string fieldName = string.IsNullOrEmpty(dbField.FieldName) ? propertyInfo.Name : dbField.FieldName;
                    object value = row[fieldName];
                    if (value != DBNull.Value)
                    {
                        if (propertyInfo.PropertyType.IsEnum)
                        {
                            propertyInfo.SetValue(targetObj, Enum.ToObject(propertyInfo.PropertyType, value), null);
                        }
                        else
                        {
                            propertyInfo.SetValue(targetObj, ChangeType(value, propertyInfo.PropertyType), null);
                        }
                    }
                }
            }

            return targetObj;
        }

        #endregion

        #region Insert data

        public static int Insert<T>(T t, DBManager dbManager)
        {
            Type type = typeof(T);

            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("INSERT INTO {0} (", GetTableName(type));

            List<DbParameter> dbParams = new List<DbParameter>();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                DBField dbField = Attribute.GetCustomAttribute(propertyInfo, typeof(DBField)) as DBField;
                if (dbField != null && !dbField.IsAutoIncrement)
                {
                    object value = propertyInfo.GetValue(t, null);
                    if (value != null)
                    {
                        string fieldName = string.IsNullOrEmpty(dbField.FieldName) ? propertyInfo.Name : dbField.FieldName;
                        dbParams.Add(dbManager.CreateDbParameter(fieldName, propertyInfo.GetValue(t, null)));

                        buffer.AppendFormat("{0},", fieldName);
                    }
                }
            }

            if (dbParams.Count > 0)
            {
                buffer.Remove(buffer.Length - 1, 1); // 去除多余的 ,
                buffer.AppendFormat(") VALUES (@{0}", dbParams[0].ParameterName);
                for (int i = 1; i < dbParams.Count; i++)
                {
                    buffer.AppendFormat(",@{0}", dbParams[i].ParameterName);
                }

                buffer.Append(")");
                string sql = buffer.ToString();
                return dbManager.ExecuteNonQuery(sql, CommandType.Text, dbParams.ToArray());
            }
            else
            {
                return 0;
            }
        }

        #endregion 

        #region Update data

        public static int Update<T>(T t, DBManager dbManager)
        {
            Type type = typeof(T);
            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("UPDATE {0} SET ", GetTableName(type));

            List<DbParameter> dbParams = new List<DbParameter>();
            List<string> keyList = new List<string>();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                DBField dbField = Attribute.GetCustomAttribute(propertyInfo, typeof(DBField)) as DBField;
                if (dbField != null)
                {
                    string fieldName = string.IsNullOrEmpty(dbField.FieldName) ? propertyInfo.Name : dbField.FieldName;
                    dbParams.Add(dbManager.CreateDbParameter(fieldName, propertyInfo.GetValue(t, null)));

                    if (dbField.IsKey)
                    {
                        keyList.Add(fieldName);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}=@{0},", fieldName);
                    }
                }
            }

            if (keyList.Count != 0 && keyList.Count != dbParams.Count)
            {
                buffer.Remove(buffer.Length - 1, 1); // remove last ","
                buffer.AppendFormat(" WHERE {0}=@{0}", keyList[0]);

                for (int i = 1; i < keyList.Count; i++)
                {
                    buffer.AppendFormat(" AND {0}=@{0}", keyList[i]);
                }

                string sql = buffer.ToString();
                return dbManager.ExecuteNonQuery(sql, CommandType.Text, dbParams.ToArray());
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region Delete data

        public static int Delete<T>(T t, DBManager dbManager)
        {
            Type type = typeof(T);
            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("DELETE FROM {0} WHERE ", GetTableName(type));

            List<DbParameter> dbParams = new List<DbParameter>();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                DBField dbField = Attribute.GetCustomAttribute(propertyInfo, typeof(DBField)) as DBField;
                if (dbField != null && dbField.IsKey)
                {
                    string fieldName = string.IsNullOrEmpty(dbField.FieldName) ? propertyInfo.Name : dbField.FieldName;
                    dbParams.Add(dbManager.CreateDbParameter(fieldName, propertyInfo.GetValue(t, null)));

                    buffer.AppendFormat("{0}=@{0} AND", fieldName);
                }
            }

            if (dbParams.Count > 0)
            {
                buffer.Remove(buffer.Length - 3, 3); // remove last "AND"
                string sql = buffer.ToString();
                return dbManager.ExecuteNonQuery(sql, CommandType.Text, dbParams.ToArray());
            }
            else
            {
                return 0;
            }
        }


        #endregion

        #endregion

        #region 私有函数

        private static string GetTableName(Type type)
        {
            DBTable tableAttribute = Attribute.GetCustomAttribute(type, typeof(DBTable)) as DBTable;
            return (tableAttribute != null && !string.IsNullOrEmpty(tableAttribute.TableName)) ?
                tableAttribute.TableName : type.Name;
        }

        private static object ChangeType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType
                 && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if(value==null || value==DBNull.Value)
                {
                    return null;
                }

                NullableConverter nullableConverter = new NullableConverter(conversionType);

                conversionType = nullableConverter.UnderlyingType;
            }

            return Convert.ChangeType(value, conversionType);
        }

        #endregion
    }
}
