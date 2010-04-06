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
// $Id: SmoDatabase.cs 67 2009-11-05 17:01:44Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using Qualent.Util;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    public class SmoDatabase : IDatabase {
        private static readonly ILog log = LogManager.GetLogger(typeof (SmoDatabase));
        private readonly string serverName;
        private readonly string databaseName;

        public SmoDatabase(string serverName, string databaseName) {
            this.serverName = serverName;
            this.databaseName = databaseName;
            Database database = GetDb();
            Guard.NotNull(database, "database", "Cannot get database for {0}\\{1}", serverName, databaseName);
        }

        public Database GetDb() {
            lock (typeof(SmoDatabase)) {
                return GetDatabase(serverName, databaseName);                
            }
        }

        public string GetConnectionString() {
            return String.Format("data source={0};initial catalog={1}; integrated security=sspi", serverName, databaseName);
        }


        public static Database GetDatabase(string serverName, string databaseName) {
            Server server = new Server(serverName);
            Database database = server.Databases[databaseName];
            Guard.NotNull(database, "database");

            //tuning pi performance 
            server.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject", "Schema");
            server.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject", "Schema");
            server.SetDefaultInitFields(typeof(View), "IsSystemObject", "Schema");
            server.SetDefaultInitFields(typeof(Table), "IsSystemObject", "Schema");
            server.SetDefaultInitFields(typeof(Index), "IsSystemObject", "Schema");
            server.SetDefaultInitFields(typeof(ForeignKey), "IsSystemObject", "Schema");

            return database;
        }

        public ICollection Tables {
            get {
                lock (log) {
                    TableCollection collection = GetDb().Tables;
                    collection.Refresh(true);
                    return collection;
                }
            }
        }

        public ICollection Views {
            get {
                lock (log) {
                    ViewCollection c = GetDb().Views;
                    c.Refresh(true);
                    return c;
                }
            }
        }

        public ICollection StoredProcedures {
            get {
                lock (log) {
                    StoredProcedureCollection c = GetDb().StoredProcedures;
                    c.Refresh(true);
                    return c;
                }
            }
        }

        public ICollection UserDefinedFunctions {
            get { return GetDb().UserDefinedFunctions; }
        }

        public ICollection Triggers {
            get { return GetDb().Triggers; }
        }

        public ICollection Schemas {
            get { return GetDb().Schemas; }
        }

        public IList SortTables(IList tables) {
            return DependencyUtils.Sort(GetDb(), tables, "Table");
        }

        public IList SortViews(IList views) {
            return DependencyUtils.Sort(GetDb(), views, "View");
        }

        public static IList GetList(TableCollection c) {
            IList l = new ArrayList();
            foreach (IScriptable s in c) {
                l.Add(s);
            }
            return l;
        }
    }
}