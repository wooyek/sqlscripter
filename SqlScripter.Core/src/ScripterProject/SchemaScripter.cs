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
// $Id: SchemaScripter.cs 53 2009-08-14 16:56:42Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using Qualent.Util;
using WooYek.Collections.Generic;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    public class SchemaScripter : ScripterBase {
        private static readonly ILog log = LogManager.GetLogger(typeof (SchemaScripter));
        protected SmoDatabase database;

        public SchemaScripter(SmoDatabase database, SelectionGroup group) {
            this.database = database;
            this.group = group;
        }

        public void RunScripting() {
            ScriptAllObjects(group);
            FileType fileType = FileType.Schema;
            List<ScriptFile> files = RemoveIgnoredFiles(fileType);
            foreach (ScriptFile file in files) {
                ScriptSchema(group, file);
            }
        }

        private void ScriptAllObjects(SelectionGroup group) {
            allObjectsDirPath = group.AllObjectsDirPath;
            MakeEmptyDirectory(allObjectsDirPath);
            
            /* 
             * sync
            
            ScriptAllObjects(RemoveIgnored(GetDb().Views, group), allObjectsDirPath, "Views");
            ScriptAllObjects(RemoveIgnored(db.Tables, group), allObjectsDirPath, "Tables");
            ScriptAllObjects(RemoveIgnored(db.StoredProcedures, group), allObjectsDirPath, "StoredProcedures");
            ScriptAllObjects(RemoveIgnored(db.UserDefinedFunctions, group), allObjectsDirPath, "UserDefinedFunctions");
            ScriptAllObjects(RemoveIgnored(db.UserDefinedFunctions, group), allObjectsDirPath, "Triggers");
            
            */

            ScriptAllObjectsAsync(group, database.Views, "Views");
            ScriptAllObjectsAsync(group, database.Tables, "Tables");
            ScriptAllObjectsAsync(group, database.StoredProcedures, "StoredProcedures");
            ScriptAllObjectsAsync(group, database.UserDefinedFunctions, "UserDefinedFunctions");
            try {
                ScriptAllObjectsAsync(group, database.Triggers, "Triggers");
            } catch (Exception e) {
                log.Warn("ScriptAllObjects: Triggers failed - IGNORE it while working with SQL 2000", e);
            }
        }

        private void ScriptAllObjectsAsync(SelectionGroup group, ICollection items, string name) {
            Thread t = new Thread(ScriptingThreadWorker);
            t.Name = group.Name + "-" + name;
            t.Start(new ScritpingParameters(items, name, group));
        }

        private class ScritpingParameters {
            public ICollection items;
            public string name;
            private SelectionGroup group;

            public ScritpingParameters(ICollection items, string name, SelectionGroup group) {
                this.items = items;
                this.name = name;
                this.group = group;
            }
        }

        private void ScriptingThreadWorker(object obj) {
            ScritpingParameters p = (ScritpingParameters) obj;
            IList items = group.RemoveIgnored(p.items);
            log.InfoFormat("ScriptAllObjects: {1}.Count={0}", items.Count, p.name);
            SmoScriptingUtil.Script(items, "sql", allObjectsDirPath, version);
        }

        private void ScriptSchema(SelectionGroup group, ScriptFile file) {
            FileInfo fi = new FileInfo(file.Name);
            if (!Directory.Exists(fi.DirectoryName)) {
                throw new ApplicationException("Direcotry does not exist: " + fi.DirectoryName);
            }

            ScriptingOptions createOptions = SmoScriptingUtil.GetCreateOptions(version);
            createOptions.Permissions = false;

            using (StreamWriter writer = new StreamWriter(file.Name, false, Encoding.UTF8)) {
                try {
                    writer.WriteLine(file.Header);
                    WriteHeading("Create: Schemas", writer);
                    ICollection schemas = RemoveIgnored(database.Schemas, file, group);
                    SmoScriptingUtil.ScriptSchemas(schemas, writer, createOptions, false);
                } catch (Exception e) {
                    log.Warn("ScriptAllObjects: Schemas failed - IGNORE it while working with SQL 2000", e);
                }

                WriteHeading("Create: Tables", writer);
                ExcludeIndexes(createOptions);
                ICollection tables = RemoveIgnored(database.Tables, file, group);

                SmoScriptingUtil.Script(tables, writer, createOptions, true, "tables");

                WriteHeading("Create: Indexes", writer);
                IncludeIndexes(createOptions);
                foreach (Table table in tables) {
                    SmoScriptingUtil.Script(table.Indexes, writer, createOptions, true, "indexes");
                }

                WriteHeading("Create: ForeignKeys", writer);
                foreach (Table t in tables) {
                    SmoScriptingUtil.Script(t.ForeignKeys, writer, createOptions, false, "foreign keys");
                }

                WriteHeading("Create: UserDefinedFunctions", writer);
                ICollection userDefinedFunctions = RemoveIgnored(database.UserDefinedFunctions, file, group);
                SmoScriptingUtil.Script(userDefinedFunctions, writer, createOptions, true, "functions");

                WriteHeading("Create: Views", writer);
                IList views = RemoveIgnored(database.Views, file, group);
                views = database.SortViews(views);
                views = RemoveIgnored(views, file, group);
                SmoScriptingUtil.Script(views, writer, createOptions, true, "views");

                WriteHeading("Create: StoredProcedures", writer);
                ICollection procedures = RemoveIgnored(database.StoredProcedures, file, group);
                SmoScriptingUtil.Script(procedures, writer, createOptions, true, "stored procedures");
            }
        }

        private IList RemoveIgnored(ICollection items, ScriptFile file, SelectionGroup group) {
            IList items2 = group.RemoveIgnored(items);
            items2 = file.RemoveIgnored(items2);
            return items2;
        }

        private void IncludeIndexes(ScriptingOptions createOptions) {
            //TODO: test options switches
            createOptions.Indexes = true;
            createOptions.DriAll = true;
            createOptions.ClusteredIndexes = true;
            createOptions.NonClusteredIndexes = true;
        }

        private void ExcludeIndexes(ScriptingOptions createOptions) {
            //TODO: test options switches
            createOptions.Indexes = false;
            createOptions.ClusteredIndexes = false;
            createOptions.NonClusteredIndexes = false;
            createOptions.DriChecks = true;
        }
    }
}