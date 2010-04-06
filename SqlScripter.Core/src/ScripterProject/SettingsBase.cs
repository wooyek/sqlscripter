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
// $Id: SettingsBase.cs 66 2009-11-05 11:36:51Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Xml.Serialization;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using SqlScripter.Core.src.ScripterProject;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    public class SettingsBase {
        private static readonly ILog log = LogManager.GetLogger(typeof (SettingsBase));
        private string name;
        private Schemas schemas = new Schemas();
        private Elements elements = new Elements();
        private Users users = new Users();
        private SelectionType defaultSelectionType = SelectionType.ExplicitExclude;

        public SettingsBase() {}

        public SettingsBase(string name) {
            this.name = name;
        }

        public Elements Elements {
            get { return elements; }
            set { elements = value; }
        }

        public Schemas Schemas {
            get { return schemas; }
            set { schemas = value; }
        }

        public Users Users {
            get { return users; }
            set { users = value; }
        }

        [XmlAttribute()]
        public string Name {
            get { return name; }
            set { name = value; }
        }

        [XmlAttribute()]
        public SelectionType DefaultSelectionType {
            get { return defaultSelectionType; }
            set { defaultSelectionType = value; }
        }

/*
        public bool IsUserExcluded(string userName) {
            switch (defaultSelectionType) {
                case SelectionType.ExplicitExclude:
                    return Users.IsExcluded(userName, false);
                case SelectionType.ExplicitInclude:
                    return Users.IsExcluded(userName, true);
                case SelectionType.Include:
                    log.DebugFormat("{0,-35} was Included by default", userName);
                    return false;
                case SelectionType.Exclude:
                    log.DebugFormat("{0,-35} was Excluded by default", userName);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
*/

        public bool IsExcluded(Microsoft.SqlServer.Management.Smo.Schema schema) {
            SelectionType? selectionType = Schemas.IsExcluded(schema);
            selectionType = UpdateSchemaSelectionType(selectionType, schema.Name);
            if (selectionType == SelectionType.Exclude || selectionType == SelectionType.ExplicitInclude) {
                return true;
            }
            return false;
        }

        public bool IsExcluded(IScriptable scriptable) {
            if (SmoUtils.IsSystemObject(scriptable, true)) {
                log.DebugFormat("{0} is system object and will be ignored", scriptable);
                return true;
            }

            if (scriptable is Microsoft.SqlServer.Management.Smo.Schema) {
                return IsExcluded((Microsoft.SqlServer.Management.Smo.Schema) scriptable);
            }

            ScriptSchemaObjectBase ssob;
            string elementSchema = GetElementSchema(scriptable, out ssob);
            SelectionType? schemaSelectionType = Schemas.IsExcluded(elementSchema);
            schemaSelectionType = UpdateSchemaSelectionType(schemaSelectionType, elementSchema);

            string elementName;
            if (ssob != null) {
                elementName = Element.GetName(ssob);
            } else {
                elementName = Element.GetName((TableModel) scriptable);
            }

            switch ((SelectionType)schemaSelectionType) {
                case SelectionType.ExplicitExclude:
                case SelectionType.ExplicitInclude:
                    bool excludeMissing = schemaSelectionType == SelectionType.ExplicitInclude;
                    return Elements.IsExcluded(elementName, excludeMissing);
                case SelectionType.Include:
                    SelectionGroup.log.DebugFormat("{0,-35} was Included by schema or default", elementName);
                    return false;
                case SelectionType.Exclude:
                    SelectionGroup.log.DebugFormat("{0,-35} was Excluded by schema or default", elementName);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetElementSchema(IScriptable scriptable, out ScriptSchemaObjectBase ssob) {
            ssob = scriptable as ScriptSchemaObjectBase;
            TableModel tableModel = scriptable as TableModel;
            if(tableModel != null) {
                return tableModel.Schema;
            }
            if (ssob == null) {
                string s = string.Format("Cannot verify schema of {0}", scriptable.GetType());
                throw new InvalidOperationException(s);
            }

            return ssob.Schema;
        }

        public IList RemoveIgnored(ICollection c) {
            IList l = new ArrayList();
            foreach (IScriptable s in c) {
                if (s == null || IsExcluded(s)) {
                    continue;
                }
                l.Add(s);
            }
            return l;
        }

        private SelectionType? UpdateSchemaSelectionType(SelectionType? schemaSelectionType, string schemaName) {
            if (schemaSelectionType == null) {
                // Jeœli schematu brakuje pozwól komuœ innemu zdecydowaæ
                SelectionGroup.log.InfoFormat("Schema {0} is not configured, will use group defult {1}", schemaName, DefaultSelectionType);
                schemaSelectionType = DefaultSelectionType;
                Schemas.Add(new Schema(schemaName, DefaultSelectionType));
            }
            return schemaSelectionType;
        }
    }
}