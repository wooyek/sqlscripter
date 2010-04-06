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
// $Id: Element.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
#endregion
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    /// <summary>
    /// Representas a sigle database schema element, eg. Table.
    /// </summary>
    public class Element {
        private static readonly ILog log = LogManager.GetLogger(typeof (Element));
        private int weight;
        private Category category = Category.Unknown;
        private SelectionType selectionType = SelectionType.ExplicitExclude;
        private string name;
        private string contentQuery = "";

        public Element() {}
        public Element(ScriptSchemaObjectBase nso, Category category) : this(GetName(nso), category) { }
        public Element(NamedSmoObject nso, Category category) : this(nso.Name, category) { }
        public Element(string name, Category category) : this(name, category, SelectionType.Include) {}
        public Element(string name, Category category, SelectionType selectionType) {
            this.name = name;
            this.category = category;
            this.selectionType = selectionType;
        }

        public static string GetName(ScriptSchemaObjectBase nso) {
            return nso.Schema + "." + nso.Name;
        }

        public static string GetName(TableModel nso) {
            return nso.Schema + "." + nso.Name;
        }

        [XmlAttribute()]
        public int Weight {
            get { return weight; }
            set { weight = value; }
        }

        [XmlAttribute()]
        public Category Category {
            get { return category; }
            set { category = value; }
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

        [XmlAttribute()]
        public string ContentQuery {
            get { return contentQuery; }
            set { contentQuery = value; }
        }
    }
}