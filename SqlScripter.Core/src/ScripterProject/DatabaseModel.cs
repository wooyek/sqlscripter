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
// $Id: DatabaseModel.cs 59 2009-10-06 09:37:52Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using log4net;
using Microsoft.SqlServer.Management.Smo;

namespace SqlScripter.ScripterProject {
    public class DatabaseModel : IDatabase {
        private static readonly ILog log = LogManager.GetLogger(typeof (DatabaseModel));
        private List<TableModel> tables = new List<TableModel>();
        private string serverName;
        private string databaseName;

        public DatabaseModel() {}
        public DatabaseModel(string serverName, string databaseName) {
            this.serverName = serverName;
            this.databaseName = databaseName;
        }

        public List<TableModel> Tables {
            get { return tables; }
            set { tables = value; }
        }

        public void AddTables(IList tables) {
            foreach (Table table in tables) {
                TableModel model = new TableModel(table);
                log.InfoFormat("AddTables: {0}", model);
                this.tables.Add(model);
            }
        }

        #region Implementation of IDatabase

        public IList SortTables(IList tablesToSort) {
            // assuming tables are sorted
            // table colletion in this model is sorted when inited.
            return tablesToSort;
        }

        private int GetTableIndexInTablesList(Table t) {
            int ind = 0;

            while (ind < tables.Count && !NormalizeTableName(Tables[ind].Name).Equals(NormalizeTableName(t.Name))){
                ind++;
            }

            if (ind == tables.Count) {
                throw new NotImplementedException();
            }
            return ind;
        }

        private string NormalizeTableName(string name) {
            int dotIndex = name.IndexOf('.');
            return (name.Substring(dotIndex + 1));
        }

        public IList SortViews(IList views) {
            throw new System.NotSupportedException();
        }

        [XmlAttribute()]
        public string ServerName {
            get { return serverName; }
            set { serverName = value; }
        }

        [XmlAttribute()]
        public string DatabaseName {
            get { return databaseName; }
            set { databaseName = value; }
        }

        #region Implementation of IDatabase

        public string GetConnectionString() {
            return String.Format("data source={0};initial catalog={1}; integrated security=sspi", serverName, databaseName);
        }

        #endregion
        [XmlIgnore]
        ICollection IDatabase.Tables { get { return tables;}
        }
        [XmlIgnore]
        public ICollection Views {
            get { throw new System.NotSupportedException(); }
        }
        [XmlIgnore]
        public ICollection StoredProcedures {
            get { throw new System.NotSupportedException(); }
        }
        [XmlIgnore]
        public ICollection UserDefinedFunctions {
            get { throw new System.NotSupportedException(); }
        }
        [XmlIgnore]
        public ICollection Triggers {
            get { throw new System.NotSupportedException(); }
        }
        [XmlIgnore]
        public ICollection Schemas {
            get { throw new System.NotSupportedException(); }
        }

        #endregion
    }
}