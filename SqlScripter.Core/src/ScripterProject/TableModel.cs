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
// $Id: TableModel.cs 45 2009-02-24 08:38:18Z janusz.skonieczny $
#endregion
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using Microsoft.SqlServer.Management.Smo;

namespace SqlScripter.ScripterProject {
    public class TableModel : IScriptable {
        private string name;
        private string schema;
        private bool hasIdentityColumn = false;
        private bool hasTriggers = false;

        public TableModel() {}

        public TableModel(Table table) {
            name = table.Name;
            schema = table.Schema;
            foreach (Column c in table.Columns) {
                if (c.Identity) {
                    this.hasIdentityColumn = true;
                    break;
                }
            }
            hasTriggers = table.Triggers.Count > 0;
        }

        [XmlAttribute()]
        public string Name {
            get { return name; }
            set { name = value; }
        }

        [XmlAttribute()]
        public string Schema {
            get { return schema; }
            set { schema = value; }
        }

        [XmlAttribute()]
        public bool HasIdentityColumn {
            get { return hasIdentityColumn; }
            set { hasIdentityColumn = value; }
        }

        [XmlAttribute()]
        public bool HasTriggers {
            get { return hasTriggers; }
            set { hasTriggers = value; }
        }


        public StringCollection Script() {
            throw new System.NotImplementedException();
        }

        public StringCollection Script(ScriptingOptions scriptingOptions) {
            throw new System.NotImplementedException();
        }

        public bool IsSystemObject { get { return false; } }

        public override string ToString() {
            return string.Format("Name: {0}, Schema: {1}", name, schema);
        }
    }
}