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
// $Id: SelectionGroup.cs 53 2009-08-14 16:56:42Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using SqlScripter.ScripterProject;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    public class SelectionGroup : SettingsBase {
        private static readonly ILog log = LogManager.GetLogger(typeof (SelectionGroup));
        private ScriptFiles files = new ScriptFiles();
        private string allObjectsDirPath;

        public SelectionGroup(string name) : base(name) {}
        public SelectionGroup() {}

        public ScriptFiles Files {
            get { return files; }
            set { files = value; }
        }

        [XmlAttribute]
        public string AllObjectsDirPath {
            get {
                if (!string.IsNullOrEmpty(allObjectsDirPath)) {
                    allObjectsDirPath = allObjectsDirPath.Trim();
                }
                if (string.IsNullOrEmpty(allObjectsDirPath)) {
                    allObjectsDirPath = "allObjects." + Name + ".new";
                }
                return allObjectsDirPath;
            }
            set { allObjectsDirPath = value; }
        }
    }

    public class SelectionGroups : List<SelectionGroup> {
        public SelectionGroup Add(string name) {
            SelectionGroup item = new SelectionGroup(name);
            Add(item);
            return item;
        }
    }
}