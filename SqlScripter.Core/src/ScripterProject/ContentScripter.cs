#region Header
// Copyright 2005-2008 Janusz Skonieczny
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// Last changes made by:
// $Id: ContentScripter.cs 61 2009-10-12 11:12:13Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using Qualent.Util;
using SqlScripter.ScripterProject;
using WooYek.Common.Database;
using WooYek.Configuration;
using WooYek.Smo;

namespace SqlScripter.Core.ScripterProject {
    public class ContentScripter : ScripterBase {
        private static readonly ILog log = LogManager.GetLogger(typeof (ContentScripter));
        private readonly IDatabase database;
        private readonly int batchStatementsCount;
        private readonly int maxTextLenght;

        public ContentScripter(IDatabase database, SelectionGroup group) {
            this.group = group;
            this.database = database;
            batchStatementsCount = (int)AppConfigHelper.Get("SqlScripter.BatchStatementsCount", 50);
            maxTextLenght = (int)AppConfigHelper.Get("SqlScripter.MaxTextLenght", 50);
        }

        public void RunScripting() {
            foreach (ScriptFile file in group.Files) {
                switch (file.Type) {
                    case FileType.Content:
                        Script(group, file);
                        break;
                    case FileType.Schema:
                    case FileType.Security:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Script(SelectionGroup group, ScriptFile file) {
            FileInfo fi = new FileInfo(file.Name);
            if (!Directory.Exists(fi.DirectoryName)) {
                throw new ApplicationException("Direcotry does not exist: " + fi.DirectoryName);
            }
//            Debugger.Break();
            log.InfoFormat("Script: Generating content {0}", file.Name);
            IList tables = GetTables(file, group);

            IScriptWriter writer = new ScriptWriter(file.Name);
            try {
                writer.WriteLine(file.Header);

                // Write delete statements for each table
                WriteHeading("Delete content first", writer.BaseWriter);
                foreach (Object table in tables) {
                    string where = GetWhere(GetTableName(table), file, group);
                    writer.WriteLine("DELETE " + GetSafeTableName(table)+ " "+where);
                }
                // Script content for each table
                foreach (Object table in tables) {
                    string contentQuery = GetContentQuery(GetTableName(table), file, group);
                    Script(table, writer, contentQuery);
                }
            } finally {
                writer.Dispose();
            }
        }

        private static string GetWhere(string tableName, ScriptFile file, SelectionGroup group) {
            string contentQuery = GetContentQuery(tableName, file, group);
            if (contentQuery == null) {
                return null;
            }
            int indexOf = contentQuery.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
            if(indexOf < 0 ) {
                return null;
            }
            return contentQuery.Substring(indexOf);
        }

        private static string GetContentQuery(string tableName, ScriptFile file, SelectionGroup group) {
            // First try take element from file descrituin as most important
            Element element = file.Elements[tableName];
            if(element == null) {
                // if missing try to take it from group as more general setting 
                element = group.Elements[tableName];
            }
            if (element == null) {
                log.DebugFormat("Script: No {0} element to take ContentQuery from", tableName);
                return null;
            }
            return element.ContentQuery;
        }

        private IList GetTables(ScriptFile file, SelectionGroup group) {
            IList tables = group.RemoveIgnored(database.Tables);
            tables = file.RemoveIgnored(tables);
            tables = database.SortTables(tables);
            // Dependencies will be inserted, assuming that user is knows what
            // she's doing by ignoring parent objects
            tables = group.RemoveIgnored(tables);
            tables = file.RemoveIgnored(tables);
            return tables;
        }

        private void Script(Object table, IScriptWriter writer, string contentQuery) {
            string tableName = GetSafeTableName(table);
            if(string.IsNullOrEmpty(contentQuery)) {
                contentQuery = "Select * from " + tableName;
                log.InfoFormat("Script: Scripting content {0} with Default query={1}", tableName, contentQuery);
            } else {
		        if (contentQuery.StartsWith("WHERE")) {
		             contentQuery = "Select * from {0} "+ contentQuery;
		        } 
                contentQuery = string.Format(contentQuery, tableName);
                log.InfoFormat("Script: Scripting content {0} with Custom  query={1}", tableName, contentQuery);
            }
            SqlCommand cmd = new SqlCommand(contentQuery);
            string connectionString = database.GetConnectionString();
//            log.InfoFormat("Script: connectionString={0}", connectionString);
            using (cmd.Connection = new SqlConnection(connectionString)) {
                cmd.Connection.Open();
                try {
                    ScriptContent(table, ref writer, cmd, tableName);
                } catch (Exception e) {
                    log.Warn("Script: Scripting content failed for query: " + contentQuery, e);
                }
                
            }
        }

        private void ScriptContent(object table, ref IScriptWriter writer, SqlCommand cmd, string tableName) {
            using (SqlDataReader reader = cmd.ExecuteReader()) {
                if(!reader.Read()) {
                    log.InfoFormat("Script: {0} has no data", tableName);
                    return;
                }
                bool hasTriggers = HasTriggers(table);
                bool setIdentityInsert = HasIdentityColumn(table);

                string head = GetTableHead(tableName, hasTriggers, setIdentityInsert);
                string foot = GetTableFoot(tableName, hasTriggers, setIdentityInsert);
                writer.WriteHead(head);
                writer.FileFooter = foot;

                int toGoCounter = 0;
                string insertHeader = GetInsertHeader(reader, tableName);
                do {
                    writer.WriteLine(insertHeader);
                    writer.Write("    VALUES (");
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (i > 0) {
                            writer.Write(",");
                        }
                        if (reader.IsDBNull(i)) {
                            writer.Write("NULL");
                            continue;
                        }
                        try {
                            WriteValue(reader, i, writer);
                        } catch (InvalidCastException ex) {
                            throw new Exception(string.Format("WriteValue failed for column {0}:{1}:{2}", i, reader.GetName(i), reader.GetValue(i).GetType().Name), ex);
                        } catch (Exception ex) {
                            throw new Exception("WriteValue failed for column " + i + ":" + reader.GetName(i),ex);
                        }
                    }
                    writer.WriteLine(")");
                    toGoCounter++;
                    if(toGoCounter >= batchStatementsCount) {
                        writer.WriteLine("GO");
                        toGoCounter = 0;
                    }
                    writer.SplitFileIfNeeded();
                } while (reader.Read());
                writer.Write(foot);
            }
        }

        private static string GetTableHead(string tableName, bool hasTriggers, bool setIdentityInsert) {
            StringWriter writer = new StringWriter();
            WriteHeading(tableName, writer);
            writer.WriteLine();
            if (hasTriggers) {
                writer.WriteLine("ALTER TABLE " + tableName + " DISABLE TRIGGER ALL");
            }
            if (setIdentityInsert) {
                writer.WriteLine("SET IDENTITY_INSERT " + tableName + " ON");
            }
            if (setIdentityInsert || hasTriggers) {
                writer.WriteLine();
            }
            return writer.ToString();
        }

        private static string GetTableFoot(string tableName, bool hasTriggers, bool setIdentityInsert) {
            StringWriter writer = new StringWriter();
            if (setIdentityInsert || hasTriggers) {
                writer.WriteLine();
            }
            if (setIdentityInsert) {
                writer.WriteLine("SET IDENTITY_INSERT " + tableName + " OFF");
            }
            if (hasTriggers) {
                writer.WriteLine("ALTER TABLE " + tableName + " ENABLE TRIGGER ALL");
            }
            writer.WriteLine();
            return writer.ToString();
        }

        private static bool HasIdentityColumn(object table) {
            bool setIdentityInsert = false;
            Table smoTable = table as Table;
            if(smoTable != null) {
                foreach (Column column in smoTable.Columns) {
                    if (column.Identity) {
                        setIdentityInsert = true;
                    }
                }    
            } else {
                return ((TableModel) table).HasIdentityColumn;
            }
            return setIdentityInsert;
        }

        private static bool HasTriggers(object table) {
            Table smoTable = table as Table;
            if (smoTable != null) {
                return smoTable.Triggers.Count > 0;
            } 
            return ((TableModel)table).HasTriggers;
        }


        private static string GetSafeTableName(object table) {
            Table smoTable = table as Table;
            if (smoTable != null) {
                return string.Format("{0}.[{1}]", smoTable.Schema, smoTable.Name);
            } else {
                TableModel tableModel = table as TableModel;
                return string.Format("{0}.[{1}]", tableModel.Schema, tableModel.Name);
            }
        } 
        private static string GetTableName(object table) {
            Table smoTable = table as Table;
            if (smoTable != null) {
                return string.Format("{0}.{1}", smoTable.Schema, smoTable.Name);
            } else {
                TableModel tableModel = table as TableModel;
                return string.Format("{0}.{1}", tableModel.Schema, tableModel.Name);
            }
        }

        private void WriteValue(SqlDataReader reader, int i, IScriptWriter writer) {
            string dataTypeName = reader.GetDataTypeName(i);
            switch (dataTypeName) {
                case "nvarchar":
                case "varchar":
                case "char":
                case "nchar":
                case "ntext":
                case "text":
                    writer.Write("'");
                    string text = reader.GetString(i).Replace("\'", "\'\'");
                    if (text.Length > maxTextLenght) {
                        text = text.Substring(0, maxTextLenght);
                    }
                    writer.Write(text);
                    writer.Write("'");
                    return;
                case "uniqueidentifier":
                    writer.Write("'");
                    writer.Write(reader.GetGuid(i).ToString("D"));
                    writer.Write("'");
                    return;
                case "datetime":
                    writer.Write("'");//CONVERT (datetime, '");
                    writer.Write(reader.GetDateTime(i).ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    writer.Write("'");//', 121)");
                    return;
                case "bit":
                    writer.Write(reader.GetBoolean(i) ? 1 : 0);
                    return;
                case "int":
                case "smallint":
                case "tinyint":
                case "bigint":
                    writer.Write(reader.GetValue(i));
                    return;
                case "float":
                    writer.Write(reader.GetDouble(i).ToString().Replace(',','.'));
                    return;
                case "money":
                    writer.Write(reader.GetDecimal(i).ToString("#.0000").Replace(',', '.'));
                    return;
                case "binary":
                case "image":
                    throw new NotSupportedException("Type " + dataTypeName + " is not upported");
                defaut:
                    throw new NotImplementedException("Type " + dataTypeName + " is not implemented");
            }
        }

        private string GetInsertHeader(SqlDataReader reader, string tableName) {
            string s = string.Format("INSERT INTO {0} (", tableName);
            StringBuilder sb = new StringBuilder(s);
            for (int i = 0; i < reader.FieldCount; i++) {
                if (i > 0) {
                    sb.Append(",");
                }
                sb.Append("[").Append(reader.GetName(i)).Append("]");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}