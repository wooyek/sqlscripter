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
// $Id: Elements.cs 59 2009-10-06 09:37:52Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    public class Elements : List<Element> {
        private static readonly ILog log = LogManager.GetLogger(typeof (Elements));
        public void Add(SortedListCollectionBase elements) {
            Category category = GetCategory(elements);
            Add(elements,category);
        }

        public Element this[string elementName] {
            get {
                foreach (Element element in this) {
                    if(elementName.Equals(element.Name)) {
                        return element;
                    }
                }
                return null;
            }
        }

        private Category GetCategory(SortedListCollectionBase elements) {
            string name = elements.GetType().Name;
            switch (name) {
                case "UserDefinedFunctionCollection": 
                    return Category.UserDefinedFunctions;
                case "StoredProcedureCollection":
                    return Category.StoredProcedures;
                case "ViewCollection":
                    return Category.Views;
                case "TableCollection":
                    return Category.Tables;
                case "DatabaseDdlTriggerCollection":
                    return Category.Triggers;
                default:
                    throw new ArgumentOutOfRangeException("Cannot determine category for " + name);
            }
        }

        public void Add(ICollection elements, Category category) {
            foreach (NamedSmoObject nso in elements) {
                if (SmoUtils.IsSystemObject(nso, true)) {
                    continue;
                }
                Add(nso, category);
            }
        }
        
        public void Add(NamedSmoObject nso, Category category) {
            ScriptSchemaObjectBase ssob = nso as ScriptSchemaObjectBase;
            if (ssob != null) {
                log.DebugFormat("Add: ssob={0}", ssob);    
                Add(new Element(ssob, category));
            } else {
                log.DebugFormat("Add: nso={0}", nso);
                Add(new Element(nso, category));
            }
        }

/*        public bool IsExcluded(ScriptSchemaObjectBase ssob, bool excludeMissing) {
            string ssobName = Element.GetName(ssob);
            return IsExcluded(ssobName, excludeMissing);
        }

        public bool IsExcluded(TableModel ssob, bool excludeMissing) {
            string ssobName = Element.GetName(ssob);
            return IsExcluded(ssobName, excludeMissing);
        }*/

        public bool IsExcluded(string objectNameWithScehma, bool excludeMissing) {
            foreach (Element element in this) {
                if (element.Name.Equals(objectNameWithScehma)) {
                    if(element.SelectionType == SelectionType.Exclude) {
                        log.DebugFormat("{0,-35} Excluded explicitly", objectNameWithScehma);
                        return true;
                    } else {
                        log.DebugFormat("{0,-35} Included explicitly", objectNameWithScehma);
                        return false;
                    }
                }
            }
            if(excludeMissing) {
                log.DebugFormat("{0,-35} is missing and was Excluded by elements collection", objectNameWithScehma);
            } else {
                log.DebugFormat("{0,-35} is missing and was Included by elements collection", objectNameWithScehma);
            }
            return excludeMissing;
        }
    }
}