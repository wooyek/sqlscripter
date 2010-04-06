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
// $Id: ProjectUtils.cs 59 2009-10-06 09:37:52Z janusz.skonieczny $
#endregion
using System.Collections;
using System.Collections.Generic;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    public class ProjectUtils {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProjectUtils));
        
        public static void GenerateDefault(string serverName, string dbName) {
            log.InfoFormat("Generating project file for {0} {1}", serverName, dbName);
            Database db = SmoUtils.GetDb(serverName, dbName);
            Project p = new Project(serverName, dbName);

            IList tables = DependencyUtils.Sort(db, SmoDatabase.GetList(db.Tables), "Table");
            p.InPlaceSchema = new DatabaseModel();
            p.InPlaceSchema.AddTables(tables);

            SelectionGroup group = p.SelectionGroups.Add("AllInOneGroup");
            group.Schemas.Add(db.Schemas);
            group.Elements.Add(tables, Category.Tables);
            group.Elements.Add(db.Views);
            group.Elements.Add(db.UserDefinedFunctions);
            group.Elements.Add(db.StoredProcedures);
            group.Elements.Add(db.Triggers);

            group.Files.Add(string.Format("{0}\\{0}.Schema.New.sql", dbName), FileType.Schema);
            group.Files.Add(string.Format("{0}\\{0}.Content.New.sql", dbName), FileType.Content);
            group.Files.Add(string.Format("{0}\\{0}.Security.New.sql", dbName), FileType.Security);


            p.Store();
        }
    }
}