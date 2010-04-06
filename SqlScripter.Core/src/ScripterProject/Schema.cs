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
// $Id: Schema.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
#endregion
using System.Collections.Generic;
using System.Xml.Serialization;
using log4net;
using Microsoft.SqlServer.Management.Smo;

namespace SqlScripter.ScripterProject {
    public class Schema  {
        private static readonly ILog log = LogManager.GetLogger(typeof (Schema));
        private SelectionType selectionType = SelectionType.ExplicitExclude;
        private string name;

        public Schema() {}

        public Schema(string name, SelectionType selectionType) {
            this.name = name;
            this.selectionType = selectionType;
        }

        public Schema(Microsoft.SqlServer.Management.Smo.Schema schema) {
            name = schema.Name;
        }

        [XmlAttribute()]
        public SelectionType SelectionType {
            get { return selectionType; }
            set { selectionType = value; }
        }

        [XmlAttribute()]
        public string Name {
            get { return name; }
            set { name = value; }
        }
    }

    public class Schemas : List<Schema> {
        private static readonly ILog log = LogManager.GetLogger(typeof (Schemas));
        public Schemas() {}

        public Schemas(SchemaCollection schemas) {
            Update(schemas);
        }

        public void Update(SchemaCollection schemas) {
            foreach (Microsoft.SqlServer.Management.Smo.Schema schema in schemas) {
                if (this[schema.Name] == null) {
                    this.Add(schema);
                }
            }
        }

        public void Add(SortedListCollectionBase elements) {
            foreach (Microsoft.SqlServer.Management.Smo.Schema schema in elements) {
                Add(schema);
            }
        }

        public void Add(Microsoft.SqlServer.Management.Smo.Schema schema) {
            Add(new Schema(schema));
        }

        public Schema this[string schemaName] {
            get {
                foreach (Schema schema in this) {
                    if (schema.Name.Equals(schemaName)) {
                        return schema;
                    }
                }
                return null;
            }
        }

/*        /// <summary>
        /// Determines how ot proceed with this object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public SelectionType? IsExcluded(ScriptSchemaObjectBase o) {
            string schemaName = o.Schema;
            return IsExcluded(schemaName);
        }

        /// <summary>
        /// Determines how ot proceed with this object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public SelectionType? IsExcluded(TableModel o) {
            string schemaName = o.Schema;
            return IsExcluded(schemaName);
        }*/



        public SelectionType? IsExcluded(Microsoft.SqlServer.Management.Smo.Schema schema) {
            return IsExcluded(schema.Name);
        }

        public SelectionType? IsExcluded(string schemaName) {
            Schema schema = this[schemaName];
            if (schema != null) {
                return schema.SelectionType;
            }
            return null;
        }
    }
}