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
 * $Id: SqlCommonException.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
 */

using System;
using System.Data.SqlClient;
using System.Text;

namespace WooYek.Common.Database {
	public class SqlCommonException : Exception {
		private SqlException innerSqlException;

		public SqlCommonException(SqlCommand sqlCommand,SqlException exception) : this(getDetailedExceptionMessage(sqlCommand,exception),exception) {
			this.innerSqlException = exception;
		}
        public SqlCommonException(SqlCommand sqlCmd, string message) : base(getDetailedExceptionMessage(sqlCmd, message)) {}

	    public SqlCommonException(string message, SqlException exception) : base(message,exception) {
			this.innerSqlException = exception;
		}
		public SqlCommonException(SqlCommand sqlCommand,Exception exception) : this(getDetailedExceptionMessage(sqlCommand,exception),exception) {
		}
		public SqlCommonException(string message, Exception exception) : base(message,exception) {
		}

		public SqlException InnerSqlException {
			get { return innerSqlException; }
		}

		#region Information helper methods

		/// <summary>
		/// Constructs a detailed exception message, for use within <c>catch</c> blocks.
		/// </summary>
		/// <param name="sqlCmd">A command that resulted in error</param>
		/// <param name="e">A caugth <see cref="Exception"/>, if it is na instance <see cref="SqlException"/> 
		/// a <see cref="getSQLExceptionDetails"/> will be called, <see cref="Exception.Message"/> will be used otherwise.</param>
		/// <returns>A exception information (containin multiple lines) with command parameters and exception details.</returns>
		public static String getDetailedExceptionMessage(SqlCommand sqlCmd, Exception e) {
			StringBuilder message = new StringBuilder("Execution (");
		    message.Append(sqlCmd.CommandText);
			if (sqlCmd.Connection != null) {
			    message.Append(") on (");
		        message.Append(sqlCmd.Connection.ConnectionString);
		    }
            message.Append(") threw : ");
			if (e is SqlException) {
				message.Append("\n\t").Append(getSQLExceptionDetails((SqlException) e));
			} else {
				message.Append(e.Message.ToString());
			}
			foreach (SqlParameter p in sqlCmd.Parameters) {
				message.Append("\n\t" + p.ParameterName).Append(" = ");
				if (!Convert.IsDBNull(p.Value) && p.Value != null) {
					message.Append(p.Value.ToString());
				} else {
					message.Append("null");
				}
			}
			return message.ToString();
		}
	    
	    public static String getDetailedExceptionMessage(SqlCommand sqlCmd, string emessage) {
			StringBuilder message = new StringBuilder("Execution (");
			message.Append(sqlCmd.CommandText).Append(") on (");
			message.Append(sqlCmd.Connection.ConnectionString).Append(") threw : ").Append(emessage);
			foreach (SqlParameter p in sqlCmd.Parameters) {
				message.Append("\n\t" + p.ParameterName).Append(" = ");
				if (!Convert.IsDBNull(p.Value) && p.Value != null) {
					message.Append(p.Value.ToString());
				} else {
					message.Append("null");
				}
			}
			return message.ToString();
		}
        
        public static String GetCmdDetails(SqlCommand sqlCmd) {
			StringBuilder message = new StringBuilder();
			message.Append(sqlCmd.CommandText).Append("\n on \n");
            message.Append(sqlCmd.Connection.ConnectionString).Append("\n with params:");
			foreach (SqlParameter p in sqlCmd.Parameters) {
				message.Append("\n\t" + p.ParameterName).Append(" = ");
				if (!Convert.IsDBNull(p.Value) && p.Value != null) {
					message.Append(p.Value.ToString());
				} else {
					message.Append("null");
				}
			}
			return message.ToString();
		}
		
		/// <summary>
		/// Constructs a string with detailed information given by <see cref="SqlException"/>.
		/// </summary>
		/// <param name="sqle">Information source</param>
		/// <returns>See <see cref="SqlException"/> properties for list of details</returns>
		public static string getSQLExceptionDetails(SqlException sqle) {
			StringBuilder details = new StringBuilder();
			details.Append("Message: " + sqle.Message);
			details.Append("\n\t Number:    " + sqle.Number.ToString());
			details.Append("\n\t Procedure: " + sqle.Procedure);
			details.Append("\n\t Server:    " + sqle.Server);
			details.Append("\n\t Source:    " + sqle.Source);
			details.Append("\n\t State:     " + sqle.State.ToString());
			details.Append("\n\t Severity:  " + sqle.Class.ToString());
			details.Append("\n\t LineNumber:" + sqle.LineNumber.ToString());
			return details.ToString();
		}

		#endregion
	}
}