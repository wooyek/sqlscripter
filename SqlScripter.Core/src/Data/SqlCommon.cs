/* Copyright 2003-2007 Janusz Skonieczny
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Created by: WooYek on 13:20:43 2003-01-30
 *
 * Last changes made by:
 * $Id: SqlCommon.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using log4net;
using Qualent.Util;

namespace WooYek.Common.Database {
    /// <summary>
    /// Summary description for SqlCommon.
    /// </summary>
    /// <remarks>Still in dev. For now wraps an older version of SqlCommon</remarks>
    public class SqlCommon {
        private static ILog log = LogManager.GetLogger(typeof (SqlCommon));
        private string connectionString;
        private IsolationLevel defaultIsolationLevel = IsolationLevel.ReadCommitted;

        public static SqlCommon New4Database(string database) {
            return New(database, "localhost");
        }

        public static SqlCommon New(string database, string host) {
            return new SqlCommon((String.Format("data source={0};initial catalog={1}; integrated security=sspi", host, database)));
        }

        public static SqlCommon New(string connectionStringName) {
            log.DebugFormat("New: {0}", connectionStringName);
            string connectionString = GetConnectionString(connectionStringName);
            return new SqlCommon(connectionString);
        }

        public static string GetConnectionString(string connectionStringName) {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            Guard.NotNull(settings, "settings", "There is no settings in ConfigurationManager.ConnectionStrings colletion for " + connectionStringName);
            return settings.ConnectionString;
        }

        //        public static SqlCommon New() {
        //            return new SqlCommon(NHibernateUtils.ConnectionString);
        //        }

        public SqlCommon(string connectionString) {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Prepares a <see cref="SqlDataAdapter"/> with given command as SelectCommand.
        /// </summary>
        /// <param name="selectCommand">A SelectCommand</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public static SqlDataAdapter BuildAdapter(SqlCommand selectCommand) {
            SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCommand);
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder();
            commandBuilder.DataAdapter = dataAdapter;
            return dataAdapter;
        }

        /// <summary>
        /// Prepares a <see cref="SqlDataAdapter"/> with given command as SelectCommand.
        /// </summary>
        /// <param name="selectCommand">A SelectCommand</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public static SqlDataAdapter BuildAdapter(string selectCommand) {
            return BuildAdapter(new SqlCommand(selectCommand));
        }

        /// <summary>
        /// Executes <a cref="Fill(SqlDataAdapter, SqlFlag, IsolationLevel)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="sqlDataAdapter"><see cref="SqlDataAdapter"/> used to fill new <see cref="DataSet"/>.</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public DataSet Fill(SqlDataAdapter sqlDataAdapter) {
            return Fill(sqlDataAdapter, SqlFlag.CommitAndClose, defaultIsolationLevel);
        }

        /// <summary>
        /// Creates a new <see cref="DataSet"/> and uses given <see cref="SqlDataAdapter"/> to fill it.
        /// </summary>
        /// <param name="sqlDataAdapter"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public DataSet Fill(SqlDataAdapter sqlDataAdapter, SqlFlag flag, IsolationLevel isolationLevel) {
            if (sqlDataAdapter == null) {
                throw new ArgumentNullException("sqlDataAdapter");
            }
            SqlCommand selectCommand = sqlDataAdapter.SelectCommand;
            if (selectCommand == null) {
                throw new ArgumentException("Given sqlDataAdapter must have SelectCommand set", "sqlDataAdapter");
            }
            EnsureOpenConnection(selectCommand);
            EnsureOpenTransaction(selectCommand, isolationLevel);
            if (selectCommand.CommandType == CommandType.StoredProcedure) {
                EnsureReturnValueParameter(selectCommand);
            }
            try {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder();
                commandBuilder.DataAdapter = sqlDataAdapter;
                DataSet ds = new DataSet();
                sqlDataAdapter.Fill(ds);
                CleanUp(selectCommand, flag);
                return ds;
            } catch (Exception e) {
                //if (log.IsWarnEnabled) log.Warn("Exception caugth: "+e.Message,e);
                if ((flag & SqlFlag.RollbackTxOnError) == SqlFlag.RollbackTxOnError) {
                    RollbackTx(selectCommand.Transaction);
                }
                if ((flag & SqlFlag.CloseConnectionOnError) == SqlFlag.CloseConnectionOnError) {
                    CloseConnection(selectCommand.Connection);
                }
                throw new SqlCommonException(selectCommand, e); // always throwing exception
            }
        }

        /// <summary>
        /// Executes <a cref="Update(SqlDataAdapter, DataSet, SqlFlag)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="sqlDataAdapter"><see cref="SqlDataAdapter"/> used to Update <see cref="DataSet"/>.</param>
        /// <param name="dataSet"><see cref="DataSet"/> to Update <see cref="DataSet"/>.</param>
        /// <returns>Updated DataSet</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public void Update(SqlDataAdapter sqlDataAdapter, DataSet dataSet) {
            Update(sqlDataAdapter, dataSet, SqlFlag.CommitAndClose);
        }

        /// <summary>
        /// Creates a new <see cref="DataSet"/> and uses given <see cref="SqlDataAdapter"/> to fill it.
        /// </summary>
        /// <param name="sqlDataAdapter"><see cref="SqlDataAdapter"/> used to Update <see cref="DataSet"/>.</param>
        /// <param name="dataSet"><see cref="DataSet"/> to update.</param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public void Update(SqlDataAdapter sqlDataAdapter, DataSet dataSet, SqlFlag flag) {
            if (sqlDataAdapter == null) {
                throw new ArgumentNullException("sqlDataAdapter");
            }
            SqlCommand selectCommand = sqlDataAdapter.SelectCommand;
            if (selectCommand == null) {
                throw new ArgumentException("Given sqlDataAdapter must have SelectCommand set", "sqlDataAdapter");
            }
            EnsureOpenConnection(selectCommand);
            EnsureOpenTransaction(selectCommand, defaultIsolationLevel);
            if (selectCommand.CommandType == CommandType.StoredProcedure) {
                EnsureReturnValueParameter(selectCommand);
            }
            try {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder();
                commandBuilder.DataAdapter = sqlDataAdapter;
                sqlDataAdapter.Update(dataSet);
                CleanUp(selectCommand, flag);
            } catch (Exception e) {
                //if (log.IsWarnEnabled) log.Warn("Exception caugth: "+e.Message,e);
                if ((flag & SqlFlag.RollbackTxOnError) == SqlFlag.RollbackTxOnError) {
                    RollbackTx(selectCommand.Transaction);
                }
                if ((flag & SqlFlag.CloseConnectionOnError) == SqlFlag.CloseConnectionOnError) {
                    CloseConnection(selectCommand.Connection);
                }
                throw new SqlCommonException(selectCommand, e); // always throwing exception
            }
        }

        /// <summary>
        /// Executes <a cref="Exec(SqlCommand, SqlFlag)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="sqlCommand"><see cref="SqlCommand"/> to execute.</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public DataSet Exec(SqlCommand sqlCommand) {
            return Exec(sqlCommand, SqlFlag.CommitAndClose);
        }

        /// <summary>
        /// Executes <a cref="Exec(SqlCommand, SqlFlag)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="sql"><see cref="SqlCommand"/> to execute.</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public DataSet Exec(string sql) {
            return Exec(new SqlCommand(sql), SqlFlag.CommitAndClose);
        }

        /// <summary>
        /// Executes <a cref="Exec(SqlCommand, SqlFlag)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="sql"><see cref="SqlCommand"/> to execute.</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public int ExecNonQuery(string sql) {
            return ExecNonQuery(new SqlCommand(sql), SqlFlag.CommitAndClose);
        }

        /// <summary>
        /// Executes <a cref="Exec(SqlCommand, SqlFlag)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="cmd"><see cref="SqlCommand"/> to execute.</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public int ExecNonQuery(SqlCommand cmd) {
            return ExecNonQuery(cmd, SqlFlag.CommitAndClose);
        }

        /// <summary>
        /// See <see cref="Exec(SqlCommand,SqlFlag,IsolationLevel)"/>
        /// </summary>
        /// <param name="sqlCommand">A command to exetute</param>
        /// <param name="flag">Controls a connection and transaction operations</param>
        /// <returns>See <see cref="Exec(SqlCommand,SqlFlag,IsolationLevel)"/></returns>
        /// <remarks>A <see cref="DefaultIsolationLevel"/> is passed to 
        /// the <see cref="Exec(SqlCommand,SqlFlag,IsolationLevel)"/></remarks>
        public DataSet Exec(SqlCommand sqlCommand, SqlFlag flag) {
            return Exec(sqlCommand, flag, defaultIsolationLevel);
        }

        /// <summary>
        /// Executes a given <see cref="SqlCommand"/>.
        /// </summary>
        /// <param name="sqlCommand">A command to exetute</param>
        /// <param name="flag">Controls a connection and transaction operations</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public DataSet Exec(SqlCommand sqlCommand, SqlFlag flag, IsolationLevel isolationLevel) {
            if (sqlCommand == null) {
                throw new ArgumentNullException("sqlCommand");
            }
            SqlDataAdapter adapter = BuildAdapter(sqlCommand);
            return Fill(adapter, flag, isolationLevel);
        }

        public int ExecNonQuery(SqlCommand sqlCommand, SqlFlag flag) {
            if (sqlCommand == null) {
                throw new ArgumentNullException("sqlCommand");
            }
            EnsureOpenConnection(sqlCommand);
            EnsureOpenTransaction(sqlCommand);
            return (int) ExecCmd(sqlCommand, flag, delegate(SqlCommand cmd) { return cmd.ExecuteNonQuery(); });
        }

        /// <summary>
        /// A cover method for <see cref="ExecCommand"/>, adds a <c>RETURN_VALUE</c> <see cref="SqlParameter"/>
        /// then calls <see cref="ExecCommand"/>.
        /// </summary>
        /// <param name="sqlCmd">Command to execute</param>
        /// <returns>Dataset returned by <see cref="ExecCommand"/></returns>
        private static void EnsureReturnValueParameter(SqlCommand sqlCmd) {
            SqlParameterCollection sp = sqlCmd.Parameters;
            if (!sp.Contains("RETURN_VALUE")) {
                sp.Add("RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
            }
        }

        /// <summary>
        /// Executes a given <see cref="SqlCommand"/>.
        /// </summary>
        /// <param name="sqlCommand">Command to execute</param>
        /// <returns>A <see cref="DataSet"/> filled by a <see cref="SqlDataAdapter"/></returns>
        private static DataSet ExecCommand(SqlCommand sqlCommand) {
            SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
            DataSet ds = new DataSet();
            if (log.IsDebugEnabled) {
                //log.Debug("Executing: "+sqlCmd.CommandText);
                DateTime t = DateTime.Now;
                da.Fill(ds);
                log.Debug("Finished ( " + DateTime.Now.Subtract(t).ToString() + " ): " + sqlCommand.CommandText);
            } else {
                da.Fill(ds);
            }
            return ds;
        }

        public static Object Identity(SqlTransaction transaction, SqlFlag flag) {
            SqlCommand sqlCommand = new SqlCommand("Select @@IDENTITY");
            sqlCommand.Transaction = transaction;
            sqlCommand.Connection = transaction.Connection;
            return ExecuteScalarInternal(sqlCommand, flag);
        }

        public static Object Identity(SqlConnection connection) {
            SqlCommand sqlCommand = new SqlCommand("Select @@IDENTITY");
            //			sqlCommand.Transaction = transaction;
            sqlCommand.Connection = connection;
            return ExecuteScalarInternal(sqlCommand, SqlFlag.None);
        }

        public static Object ScopeIdentity(SqlTransaction transaction, SqlFlag flag) {
            SqlCommand sqlCommand = new SqlCommand("Select SCOPE_IDENTITY()");
            sqlCommand.Transaction = transaction;
            sqlCommand.Connection = transaction.Connection;
            return ExecuteScalarInternal(sqlCommand, flag);
        }

        public static Object ScopeIdentity(SqlConnection connection) {
            SqlCommand sqlCommand = new SqlCommand("Select SCOPE_IDENTITY()");
            //            sqlCommand.Transaction = connection.BeginTransaction();
            sqlCommand.Connection = connection;
            return ExecuteScalarInternal(sqlCommand, SqlFlag.None);
        }

        /// <summary>
        /// Executes <a cref="Exec(SqlCommand, SqlFlag)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="sql"><see cref="SqlCommand"/> to execute.</param>
        /// <returns>DataSet filled with data retuned by given SqlCommand</returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public object ExecuteScalar(string sql) {
            return ExecuteScalar(new SqlCommand(sql), SqlFlag.CommitAndClose);
        }

        /// <summary>
        /// Executes <see cref="ExecuteScalar(SqlCommand, SqlFlag)"/> with <see cref="SqlFlag.CommitAndClose"/>.
        /// </summary>
        /// <param name="sqlCommandcmd"><see cref="SqlCommand"/> to execute.</param>
        /// <returns>See <see cref="SqlCommand.ExecuteScalar()"/></returns>
        /// <exception cref="SqlCommonException">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public object ExecuteScalar(SqlCommand sqlCommandcmd) {
            return ExecuteScalar(sqlCommandcmd, SqlFlag.CommitAndClose);
        }

        /// <summary>
        /// Executes a given <see cref="SqlCommand"/> by <see cref="SqlCommand.ExecuteScalar"/>.
        /// </summary>
        /// <param name="sqlCommand">A command to exetute</param>
        /// <exception cref="Exception">If command execution results in exception, a detailed information about error is summarised and exception is thrown</exception>
        public object ExecuteScalar(SqlCommand sqlCommand, SqlFlag flag) {
            if (sqlCommand == null) {
                throw new ArgumentException("Given sqlDataAdapter must have SelectCommand set", "sqlDataAdapter");
            }
            EnsureOpenConnection(sqlCommand);
            EnsureOpenTransaction(sqlCommand);
            return ExecuteScalarInternal(sqlCommand, flag);
        }

        private static object ExecuteScalarInternal(SqlCommand sqlCommand, SqlFlag flag) {
            return ExecCmd(sqlCommand, flag, delegate(SqlCommand cmd) { return sqlCommand.ExecuteScalar(); });
        }

        private static object ExecCmd(SqlCommand sqlCommand, SqlFlag flag, ExecCmdWorker work) {
            if (sqlCommand.CommandType == CommandType.StoredProcedure) {
                EnsureReturnValueParameter(sqlCommand);
            }
            try {
                object o = work(sqlCommand);
                CleanUp(sqlCommand, flag);
                return o;
            } catch (Exception e) {
                //if (log.IsWarnEnabled) log.Warn("Exception caugth: "+e.Message,e);
                if ((flag & SqlFlag.RollbackTxOnError) == SqlFlag.RollbackTxOnError) {
                    RollbackTx(sqlCommand.Transaction);
                }
                if ((flag & SqlFlag.CloseConnectionOnError) == SqlFlag.CloseConnectionOnError) {
                    CloseConnection(sqlCommand.Connection);
                }
                throw new SqlCommonException(sqlCommand, e); // always throwing exception
            }
        }

        private static void CleanUp(SqlCommand sqlCommand, SqlFlag flag) {
            if ((flag & SqlFlag.CommitTransaction) == SqlFlag.CommitTransaction) {
                Guard.NotNull(sqlCommand.Transaction, "sqlCommand.Transaction");
                sqlCommand.Transaction.Commit();
            }
            if ((flag & SqlFlag.CloseConnection) == SqlFlag.CloseConnection) {
                CloseConnection(sqlCommand.Connection);
            }
        }

        private delegate object ExecCmdWorker(SqlCommand cmd);

        public string ConnectionString {
            get { return connectionString; }
            set { connectionString = value; }
        }

        #region Connection operations

        /// <summary>
        /// Ensures that <see cref="SqlCommand"/> have an opened <see cref="SqlConnection"/>.
        /// Check if SqlCommand have an open SqlConnection assigned, if not new <see cref="SqlConnection"/>
        /// is opened, existing <see cref="SqlConnection"/> is opened otherwise.
        /// </summary>
        private void EnsureOpenConnection(SqlCommand sqlCmd) {
            if (sqlCmd.Connection == null) {
                if (sqlCmd.Transaction != null && sqlCmd.Transaction.Connection != null) {
                    sqlCmd.Connection = sqlCmd.Transaction.Connection;
                    if (sqlCmd.Connection.State != ConnectionState.Open) {
                        sqlCmd.Connection.Open();
                    }
                } else {
                    SqlConnection sqlConnection = GetSqlConnection();
                    sqlCmd.Connection = sqlConnection;
                    sqlConnection.Open();
                }
            } else if (sqlCmd.Connection.State != ConnectionState.Open) {
                sqlCmd.Connection.Open();
            }
        }

        public SqlConnection GetSqlConnection() {
            return new SqlConnection(ConnectionString);
        }

        public static void OpenConnection(SqlCommand sqlCmd) {
            if (sqlCmd.Connection == null) {
                throw new ApplicationException("Cannot open command connection, the Connection property is not initialized");
            }
            if (sqlCmd.Connection.State != ConnectionState.Open) {
                sqlCmd.Connection.Open();
            }
        }

        /// <summary>
        /// Closes <see cref="SqlConnection"/>
        /// </summary>
        /// <param name="sqlConnection"></param>
        public static void CloseConnection(SqlConnection sqlConnection) {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed) {
                sqlConnection.Close();
            }
        }

        #endregion

        #region Transaction operations

        /// <summary>
        /// If a SqlCommand does not have a transaction new Transaction is started.
        /// IsolationLevel.ReadCommitted is set as a default;
        /// </summary>
        /// <param name="sqlCmd">A command to whitch transaction will be assigned</param>
        private void EnsureOpenTransaction(SqlCommand sqlCmd) {
            EnsureOpenTransaction(sqlCmd, defaultIsolationLevel);
        }

        /// <summary>
        /// If a SqlCommand does not have a transaction new Transaction is started.
        /// </summary>
        /// <param name="sqlCmd">A command to whitch transaction will be assigned</param>
        /// <param name="isolation">A transaction <see cref="IsolationLevel"/>.</param>
        private static void EnsureOpenTransaction(SqlCommand sqlCmd, IsolationLevel isolation) {
            if (sqlCmd.Transaction == null) {
                sqlCmd.Transaction = sqlCmd.Connection.BeginTransaction(isolation);
            } else {
                if (sqlCmd.Transaction.Connection == null) {
                    throw new InvalidOperationException("Current transaction have been closed");
                }
            }
        }

        /// <summary>
        /// Opens <see cref="OpenConnection">connection</see> if needed 
        /// and <see cref="EnsureOpenTransaction(SqlCommand,IsolationLevel)">begins transaction</see> id not set.
        /// </summary>
        /// <param name="sqlCmd"></param>
        public static void BeginTransaction(SqlCommand sqlCmd) {
            OpenConnection(sqlCmd);
            EnsureOpenTransaction(sqlCmd, IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Rollback a transation. 
        /// </summary>
        /// <param name="sqlTransaction">Transation to rollback</param>
        public static void RollbackTx(SqlTransaction sqlTransaction) {
            if (log.IsDebugEnabled) {
                log.Debug("Rolling back transaction");
            }
            if (sqlTransaction != null) {
                if (sqlTransaction.Connection == null) {
                    throw new ArgumentException("Given transaction has finished, connection was nulled", "sqlTransaction");
                }
                try {
                    sqlTransaction.Rollback();
                } catch (Exception e) {
                    if (log.IsWarnEnabled) {
                        log.Warn("Exception caugth druing transaction rollback:" + e.Message, e);
                    }
                    Console.Out.WriteLine("An exception " + e.Message + "\n\tof type " + e.GetType().ToString()
                                          + "\n\twas encountered while attempting to roll back the transaction:" + e.StackTrace);
                    throw new Exception("Caugth an expetion during SqlTransaction.Rollback: " + e.Message, e);
                }
            }
        }

        #endregion

        public IsolationLevel DefaultIsolationLevel {
            get { return defaultIsolationLevel; }
            set { defaultIsolationLevel = value; }
        }

        public static object GetValue(object value) {
            if (value == null || (value as string) == String.Empty) {
                return DBNull.Value;
            }
            return value;
        }

        public static string AddWildcards(string param) {
            if (param == null || param == String.Empty) {
                param = "*";
            } else if (!param.EndsWith("*") && !param.EndsWith("?")) {
                param = param + "*";
            }
            return param.Replace('*', '%').Replace('?', '_');
        }

        #region Intance sigleton

        private static Dictionary<string, SqlCommon> instances = new Dictionary<string, SqlCommon>();
        private static SqlCommon instance;

        public static Dictionary<string, SqlCommon> Instances {
            get { return instances; }
        }

        public static SqlCommon Default {
            get {
                if (instance == null) {
                    instance = New("Default");
                }
                return instance;
            }
            set { instance = value; }
        }

        #endregion
    }

    public enum SqlFlag {
        None = 0,
        CloseConnection = 1,
        CommitTransaction = 2,
        CloseConnectionOnError = 4,
        CommitAndClose = CloseConnection + CommitTransaction + CloseConnectionOnError + RollbackTxOnError,
        RollbackTxOnError = 8,
        RollbackAndCloseOnError = RollbackTxOnError + CloseConnectionOnError,
    }
}