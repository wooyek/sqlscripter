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
// $Id: Project.cs 53 2009-08-14 16:56:42Z janusz.skonieczny $
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using SqlScripter.Core.ScripterProject;
using WooYek.Common.Doc;

namespace SqlScripter.ScripterProject {
    public class Project : DocumentBase<Project> {
        private static readonly ILog log = LogManager.GetLogger(typeof (Project));
        public static string DefaultFileName = "Project.scripter.xml";
        private string server;
        private string databaseName;
        private SelectionGroups selectionGroups = new SelectionGroups();
        private DatabaseModel inPlaceSchema;

        public Project() {
            Path = DefaultFileName;
        }

        public Project(string server, string databaseName) : this() {
            this.server = server;
            this.databaseName = databaseName;
        }

        public void RunScripting() {
            DateTime start = DateTime.Now;
            //ToDO: Introduce Dependecy injection use Ninject

            foreach (SelectionGroup group in selectionGroups) {
                ScripterBase.MakeEmptyDirectory(group.AllObjectsDirPath);
                foreach (ScriptFile file in group.Files) {
                    FileInfo fi = new FileInfo(file.Name);
                    ScripterBase.MakeEmptyDirectory(fi.DirectoryName);
                }
                if (HasInPlaceSchema) {
                    log.Info("RunScripting: Project has schema defined in place, scripting content only");
                    Thread t2 = new Thread(ContentThreadWorker);
                    t2.Name = group.Name + "-Content";
                    t2.Start(group);
                    continue;
                } 
                Thread t = new Thread(SchemaThreadWorker);
                t.Name = group.Name;
                t.Start(group);
                Thread contentThread = new Thread(ContentThreadWorker);
                contentThread.Name = group.Name + "-Content";
                contentThread.Start(group);
                Thread t3 = new Thread(SecurityThreadWorker);
                t3.Name = group.Name + "-Security";
                t3.Start(group);
            }
            //log.InfoFormat("Sripted in: {0:mm:ss:fff}", DateTime.Now.Subtract(start));
        }

        public bool HasInPlaceSchema {
            get { return this.InPlaceSchema != null; }
        }

        private void SchemaThreadWorker(object obj) {
            SmoDatabase database = new SmoDatabase(server, databaseName);
            SchemaScripter s = new SchemaScripter(database, (SelectionGroup)obj);
            s.RunScripting();
        }

        private void SecurityThreadWorker(object obj) {
            SmoDatabase database = new SmoDatabase(server, databaseName);
            SecurityScripter s = new SecurityScripter(database, (SelectionGroup)obj);
            s.RunScripting();
        }

        private void ContentThreadWorker(object obj) {
            IDatabase database;
            if (this.HasInPlaceSchema) {
                this.InPlaceSchema.ServerName = server;
                this.InPlaceSchema.DatabaseName = databaseName;
                database = this.InPlaceSchema;
            } else {
                database = new SmoDatabase(server, databaseName);
            }
            ContentScripter cs = new ContentScripter(database, (SelectionGroup)obj);
            cs.RunScripting();
        }

        private IEnumerable<ScriptFile> ScriptFiles {
            get {
                foreach (SelectionGroup item in selectionGroups) {
                    foreach (ScriptFile file in item.Files) {
                        yield return file;
                    }
                }
            }
        }

        public SelectionGroups SelectionGroups {
            get { return selectionGroups; }
            set { selectionGroups = value; }
        }

        [XmlAttribute()]
        public string Server {
            get { return server; }
            set { server = value; }
        }

        [XmlAttribute()]
        public string DatabaseName {
            get { return databaseName; }
            set { databaseName = value; }
        }

        public static Project Load() {
            if (File.Exists(DefaultFileName)) {
                DefaultFileName = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + DefaultFileName;
                log.InfoFormat("Loading default: {0}", DefaultFileName);
            }
            return Load(DefaultFileName);
        }

        new public static Project Load(string fileName) {
            if (!System.IO.Path.IsPathRooted(fileName)) {
                fileName = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + fileName;
            }
            if (!File.Exists(fileName)) {
                log.InfoFormat("File does not exist: {0}", fileName);
                return null;
            }
            return DocumentBase<Project>.Load(fileName);
        }

        public DatabaseModel InPlaceSchema {
            get { return inPlaceSchema; }
            set { inPlaceSchema = value; }
        }
    }

    public enum FileType {
        Schema,
        Content,
        Security
    }

    public enum SelectionType {
        /// <summary>
        /// Include those that are explicitly cofigured and mareked for scripting and those that 
        /// are not configured.
        /// </summary>
        ExplicitExclude,
        /// <summary>
        /// Include only those that are explicitly cofigured and mareked for scripting.
        /// </summary>
        ExplicitInclude,
        /// <summary>
        /// This item was selected to be scripted.
        /// </summary>
        Include,
        /// <summary>
        /// An item was excluded from scripting.
        /// </summary>
        Exclude
    }

    public enum Category {
        Unknown,
        Tables,
        Views,
        UserDefinedFunctions,
        StoredProcedures,
        Triggers
    }
}