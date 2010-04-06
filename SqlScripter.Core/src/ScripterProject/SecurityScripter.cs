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
// $Id: SecurityScripter.cs 66 2009-11-05 11:36:51Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using SqlScripter.ScripterProject;
using WooYek.Common.Database;
using WooYek.Smo;

namespace SqlScripter.Core.ScripterProject {
    public class SecurityScripter : ScripterBase {
        private static readonly ILog log = LogManager.GetLogger(typeof (SecurityScripter));
        protected SmoDatabase database;

        public SecurityScripter(SmoDatabase database, SelectionGroup group) {
            this.group = group;
            this.database = database;
        }

        public void RunScripting() {
            foreach (ScriptFile file in group.Files) {
                switch (file.Type) {
                    case FileType.Content:
                    case FileType.Schema:
                        continue;
                    case FileType.Security:
                        Script(group, file);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Script(SelectionGroup group, ScriptFile file) {
            //            Debugger.Break();
            log.InfoFormat("Script: Generating security {0}", file.Name);
            FileInfo fi = new FileInfo(file.Name); 
            if (!Directory.Exists(fi.DirectoryName)) {
                throw new ApplicationException("Direcotry does not exist: " + fi.DirectoryName);
            }
            using (StreamWriter writer = new StreamWriter(file.Name, false, Encoding.UTF8)) {
                writer.WriteLine(file.Header);

                ScriptingOptions options = SmoScriptingUtil.GetSecurityOptions(version);

                WriteHeading("Permissions: Tables", writer);
                IList tables = GetTables(file, group, database);
                SmoScriptingUtil.ScriptPersmissions(tables, writer, true, "tables");

                WriteHeading("Permissions: Views", writer);
                IList views = group.RemoveIgnored(database.Views);
                views = group.RemoveIgnored(views);
                SmoScriptingUtil.ScriptPersmissions(views, writer, true, "views");

                //TODO: check if it can be done simplier just with setting scriptingOptions
                //temp. replaced with another method
              
//                WriteHead("Permissions: Stored procedures", writer);
//                ICollection procedures = group.RemoveIgnored(db.StoredProcedures);
//                SmoScriptingUtil.Script(procedures, writer, options, false);
                
                
                WriteHeading("Permissions: Stored procedures", writer);
//                Debugger.Break();
                ICollection procedures = group.RemoveIgnored(database.StoredProcedures);
                SmoScriptingUtil.ScriptPersmissions(procedures, writer, true, "stored procedures");

                WriteHeading("Permissions: User defined functions", writer);
                ICollection udf = group.RemoveIgnored(database.UserDefinedFunctions);
                SmoScriptingUtil.Script(udf, writer, options, true, "functions");

                try {
                    WriteHeading("Permissions: Triggers", writer);
                    ICollection triggers = group.RemoveIgnored(database.Triggers);
                    SmoScriptingUtil.Script(triggers, writer, options, true, "triggers");
                } catch (Exception e) {
                    log.Warn("Script: Triggers failed - IGNORE it while working with SQL 2000", e);
                }
            }
            
        }
    }
}