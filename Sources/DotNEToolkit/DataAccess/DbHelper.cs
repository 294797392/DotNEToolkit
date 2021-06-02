using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Collections;
using System.Data.SqlClient;

namespace DotNEToolkit.DataAccess
{
    public sealed class DbHelper
    {
        #region Private Utilities

        /// <summary>
        /// This method is used to attach array of DbParameters to a DbCommand.
        /// 
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.  
        /// 
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of DbParameters to be added to command</param>
        private static void AttachParameters(DbCommand command, DbParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (DbParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // Check for derived output value with no value assigned
                        if ((p.Direction == ParameterDirection.InputOutput ||
                          p.Direction == ParameterDirection.Input) &&
                          (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// This method assigns dataRow column values to an array of DbParameters
        /// </summary>
        /// <param name="commandParameters">Array of DbParameters to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        private static void AssignParameterValues(DbParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                // Do nothing if we get no data
                return;
            }

            int i = 0;
            // Set the parameters values
            foreach (DbParameter commandParameter in commandParameters)
            {
                // Check the parameter name
                if (commandParameter.ParameterName == null ||
                  commandParameter.ParameterName.Length <= 1)
                    throw new Exception(
                      string.Format(
                        "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                        i, commandParameter.ParameterName));
                if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                i++;
            }
        }

        /// <summary>
        /// This method assigns an array of values to an array of DbParameters
        /// </summary>
        /// <param name="commandParameters">Array of DbParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private static void AssignParameterValues(DbParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                // Do nothing if we get no data
                return;
            }

            // We must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            // Iterate through the DbParameters, assigning the values from the corresponding position in the 
            // value array
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                // If the current array value derives from IDbDataParameter, then assign its Value property
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                    if (paramInstance.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = paramInstance.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        /// to the provided command
        /// </summary>
        /// <param name="command">The DbCommand to be prepared</param>
        /// <param name="connection">A valid DbConnection, on which to execute this command</param>
        /// <param name="transaction">A valid DbTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-Sql command</param>
        /// <param name="commandParameters">An array of DbParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
        private static void PrepareCommand(DbCommand command, DbConnection connection, DbTransaction transaction, 
            CommandType commandType, string commandText, DbParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // Associate the connection with the command
            command.Connection = connection;

            // Set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            // If we were provided a transaction, assign it
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }

            // Set the command type
            command.CommandType = commandType;

            // Attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        #endregion Private Utilities

        #region Connection

        public static DbConnection CreateDbConnection(string connectionString, ProviderType providerType)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a DbConnection, and dispose of it after we are done
            DbConnection connection = DbFactory.GetProvider(providerType).CreateConnection();
            connection.ConnectionString = connectionString;

            connection.Open();
            return connection;
        }

        #endregion

        #region Transactions

        public static DbTransaction CreateDbTransaction(string connectionString, ProviderType providerType)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a DbConnection, and dispose of it after we are done
            DbConnection connection = DbFactory.GetProvider(providerType).CreateConnection();
            
            connection.ConnectionString = connectionString;

            connection.Open();

            return connection.BeginTransaction();
        }

        public static void CommitTransaction(DbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            transaction.Commit();
        }

        public static void RollbackTransaction(DbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            transaction.Rollback();
        }

        #endregion

        #region Create Parameters

        public static DbParameter CreateDbParameter(ProviderType providerType)
        {
            DbProviderFactory factory = DbFactory.GetProvider(providerType);
            return factory.CreateParameter();
        }

        public static DbParameter[] CreateDbParameters(ProviderType providerType, int count)
        {
            DbParameter[] parameters = new DbParameter[count];
            DbProviderFactory factory = DbFactory.GetProvider(providerType);
            for (int i = 0; i < count; i++)
            {
                parameters[i] = factory.CreateParameter();
            }

            return parameters;
        }

        public static DbParameter[] CreateDbParameters(DbConnection connection, ProviderType providerType, string spName)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, providerType, spName);
            return commandParameters;
        }

        #endregion

        #region CreateCommand

        /// <summary>
        /// Simplify the creation of a Db command object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DbCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid DbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid DbCommand object</returns>
        public static DbCommand CreateCommand(DbConnection connection, ProviderType providerType, int commandTimeout, string spName, params string[] sourceColumns)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // Create a SqlCommand
            DbCommand cmd = DbFactory.GetProvider(providerType).CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            // If we receive parameter values, we need to figure out where they go
            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, providerType, spName);

                // Assign the provided source columns to these parameters based on parameter order
                for (int index = 0; index < sourceColumns.Length; index++)
                    commandParameters[index].SourceColumn = sourceColumns[index];

                // Attach the discovered parameters to the SqlCommand object
                AttachParameters(cmd, commandParameters);
            }

            return cmd;
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Execute a DbCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-Sql command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteNonQuery(connectionString, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a DbConnection, and dispose of it after we are done
            using (DbConnection connection = DbFactory.GetProvider(providerType).CreateConnection())
            {
                connection.ConnectionString = connectionString;

                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteNonQuery(connection, providerType, commandTimeout, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connectionString, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteNonQuery(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns no resultset and takes no parameters) against the provided DbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-Sql command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(DbConnection connection, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteNonQuery(connection, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns no resultset) against the specified DbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(DbConnection connection, ProviderType providerType, int commandTimeout, CommandType commandType, 
            string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            DbCommand cmd = DbFactory.GetProvider(providerType).CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            int retval = cmd.ExecuteNonQuery();

            // Detach the DbParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns no resultset) against the specified DbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(DbConnection connection, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteNonQuery(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns no resultset and takes no parameters) against the provided DbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, ProviderType dbType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteNonQuery(transaction, dbType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns no resultset) against the specified DbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, ProviderType dbType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            DbCommand cmd = DbFactory.GetProvider(dbType).CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            int retval = cmd.ExecuteNonQuery();

            // Detach the DbParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns no resultset) against the specified 
        /// DbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, ProviderType dbType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(transaction.Connection, dbType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteNonQuery(transaction, dbType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(transaction, dbType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteNonQuery

        #region ExecuteDataset

        /// <summary>
        /// Execute a DbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteDataset(connectionString, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a DbConnection, and dispose of it after we are done
            using (DbConnection connection = DbFactory.GetProvider(providerType).CreateConnection())
            {
                connection.ConnectionString = connectionString;

                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteDataset(connection, providerType, commandTimeout, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connectionString, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteDataset(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset and takes no parameters) against the provided DbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(DbConnection connection, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteDataset(connection, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset) against the specified DbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(DbConnection connection, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            DbProviderFactory factory = DbFactory.GetProvider(providerType);
            DbCommand cmd = factory.CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (DbDataAdapter da = factory.CreateDataAdapter())
            {
                da.SelectCommand = cmd;

                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the DbParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                    connection.Close();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a resultset) against the specified DbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(DbConnection connection, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteDataset(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset and takes no parameters) against the provided DbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteDataset(transaction, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset) against the specified DbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            DbProviderFactory factory = DbFactory.GetProvider(providerType);
            DbCommand cmd = factory.CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (DbDataAdapter da = factory.CreateDataAdapter())
            {
                da.SelectCommand = cmd;

                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the DbParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a resultset) against the specified 
        /// DbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(transaction.Connection, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteDataset(transaction, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(transaction, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteDataset

        #region ExecuteReader

        /// <summary>
        /// This enum is used to indicate whether the connection was provided by the caller, or created by DbHelper, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        private enum DbConnectionOwnership
        {
            /// <summary>Connection is owned and managed by DbHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }

        /// <summary>
        /// Create and prepare a DbCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        /// 
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">A valid DbConnection, on which to execute this command</param>
        /// <param name="transaction">A valid DbTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by DbHelper</param>
        /// <returns>DbDataReader containing the results of the command</returns>
        private static DbDataReader ExecuteReader(DbConnection connection, ProviderType providerType, int commandTimeout, DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters, DbConnectionOwnership connectionOwnership)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;
            // Create a command and prepare it for execution
            DbCommand cmd = DbFactory.GetProvider(providerType).CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                // Create a reader
                DbDataReader dataReader;

                // Call ExecuteReader with the appropriate CommandBehavior
                if (connectionOwnership == DbConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }

                // Detach the DbParameters from the command object, so they can be used again.
                // HACK: There is a problem here, the output parameter values are fletched 
                // when the reader is closed, so if the parameters are detached from the command
                // then the DbReader can磘 set its values. 
                // When this happen, the parameters can磘 be used again in other command.
                bool canClear = true;
                foreach (DbParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                        canClear = false;
                }

                if (canClear)
                {
                    cmd.Parameters.Clear();
                }

                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                    connection.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-Sql command</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteReader(connectionString, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            DbConnection connection = null;
            try
            {
                connection = DbFactory.GetProvider(providerType).CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();

                // Call the private overload that takes an internally owned connection in place of the connection string
                return ExecuteReader(connection, providerType, commandTimeout, null, commandType, commandText, commandParameters, DbConnectionOwnership.Internal);
            }
            catch
            {
                // If we fail to return the DbDatReader, we need to close the connection ourselves
                if (connection != null) connection.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(string connectionString, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connectionString, providerType, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset and takes no parameters) against the provided DbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-Db command</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteReader(connection, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset) against the specified DbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, ProviderType providerTpye, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            // Pass through the call to the private overload using a null transaction value and an externally owned connection
            return ExecuteReader(connection, providerTpye, commandTimeout, (DbTransaction)null, commandType, commandText, commandParameters, DbConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a resultset) against the specified DbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, providerType, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset and takes no parameters) against the provided DbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteReader(transaction, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a resultset) against the specified DbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///   DbDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Pass through to private overload, indicating that the connection is owned by the caller
            return ExecuteReader(transaction.Connection, providerType, commandTimeout, transaction, commandType, commandText, commandParameters, DbConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a resultset) against the specified
        /// DbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DbDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DbDataReader containing the resultset generated by the command</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(transaction.Connection, providerType, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(transaction, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteReader

        #region ExecuteScalar

        /// <summary>
        /// Execute a DbCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteScalar(connectionString, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            // Create & open a DbConnection, and dispose of it after we are done
            using (DbConnection connection = DbFactory.GetProvider(providerType).CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteScalar(connection, providerType, commandTimeout, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connectionString, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteScalar(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connectionString, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns a 1x1 resultset and takes no parameters) against the provided DbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(DbConnection connection, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteScalar(connection, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a 1x1 resultset) against the specified DbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(DbConnection connection, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            DbCommand cmd = DbFactory.GetProvider(providerType).CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // Detach the DbParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a 1x1 resultset) against the specified DbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(DbConnection connection, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteScalar(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connection, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a DbCommand (that returns a 1x1 resultset and takes no parameters) against the provided DbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(DbTransaction transaction, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of DbParameters
            return ExecuteScalar(transaction, providerType, commandTimeout, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a DbCommand (that returns a 1x1 resultset) against the specified DbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(DbTransaction transaction, ProviderType providerType, int commandTimeout, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            DbCommand cmd = DbFactory.GetProvider(providerType).CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // Detach the DbParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a DbCommand (that returns a 1x1 resultset) against the specified
        /// DbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid DbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(DbTransaction transaction, ProviderType providerType, int commandTimeout, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // PPull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(transaction.Connection, providerType, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of DbParameters
                return ExecuteScalar(transaction, providerType, commandTimeout, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(transaction, providerType, commandTimeout, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteScalar
    }


    /// <summary>
    /// DbHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class DbHelperParameterCache
    {
        #region private methods, variables, and constructors

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new DbHelperParameterCache()"
        private DbHelperParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Resolve at run time the appropriate set of DbParameters for a stored procedure
        /// </summary>
        /// <param name="connection">A valid DbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        private static DbParameter[] DiscoverSpParameterSet(DbConnection connection, ProviderType providerType, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            DbCommand dbCommand = DbFactory.GetProvider(providerType).CreateCommand();
            dbCommand.Connection = connection;
            dbCommand.CommandText = spName;
            dbCommand.CommandType = CommandType.StoredProcedure;

            connection.Open();
            SqlCommandBuilder.DeriveParameters(dbCommand as SqlCommand);
            connection.Close();

            if (!includeReturnValueParameter)
            {
                dbCommand.Parameters.RemoveAt(0);
            }

            DbParameter[] discoveredParameters = new DbParameter[dbCommand.Parameters.Count];

            dbCommand.Parameters.CopyTo(discoveredParameters, 0);

            // Init the parameters with a DBNull value
            foreach (DbParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

        /// <summary>
        /// Deep copy of cached DbParameter array
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        private static DbParameter[] CloneParameters(DbParameter[] originalParameters)
        {
            DbParameter[] clonedParameters = new DbParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (DbParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of DbParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An array of DbParamters</returns>
        public static DbParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            DbParameter[] cachedParameters = paramCache[hashKey] as DbParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of DbParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of DbParameters</returns>
        public static DbParameter[] GetSpParameterSet(string connectionString, ProviderType providerType, string spName)
        {
            return GetSpParameterSet(connectionString, providerType, spName, false);
        }

        /// <summary>
        /// Retrieves the set of DbParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a DbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of DbParameters</returns>
        public static DbParameter[] GetSpParameterSet(string connectionString, ProviderType providerType, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            using (DbConnection connection = DbFactory.GetProvider(providerType).CreateConnection())
            {
                connection.ConnectionString = connectionString;
                return GetSpParameterSetInternal(connection, providerType, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of DbParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid DbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of DbParameters</returns>
        internal static DbParameter[] GetSpParameterSet(DbConnection connection, ProviderType providerType, string spName)
        {
            return GetSpParameterSet(connection, providerType, spName, false);
        }

        /// <summary>
        /// Retrieves the set of DbParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid DbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of DbParameters</returns>
        internal static DbParameter[] GetSpParameterSet(DbConnection connection, ProviderType providerType, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            using (DbConnection clonedConnection = (DbConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, providerType, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of DbParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid DbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of DbParameters</returns>
        private static DbParameter[] GetSpParameterSetInternal(DbConnection connection, ProviderType providerType, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            DbParameter[] cachedParameters;

            cachedParameters = paramCache[hashKey] as DbParameter[];
            if (cachedParameters == null)
            {
                DbParameter[] spParameters = DiscoverSpParameterSet(connection, providerType, spName, includeReturnValueParameter);
                paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }

        #endregion Parameter Discovery Functions

    }
}
