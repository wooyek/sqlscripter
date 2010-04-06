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
// $Id: ScripterBase.cs 70 2010-02-01 10:21:54Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using Qualent.Util;
using WooYek.Smo;

namespace SqlScripter.ScripterProject {
    public class ScripterBase {
        private static readonly ILog log = LogManager.GetLogger(typeof (ScripterBase));
        protected static SqlServerVersion version = SqlServerVersion.Version100;
        protected string allObjectsDirPath;
        protected SelectionGroup group;

        protected List<ScriptFile> RemoveIgnoredFiles(FileType fileType) {
            List<ScriptFile> files = new List<ScriptFile>();
            foreach (ScriptFile file in group.Files) {
                if (file.Type == fileType) {
                    files.Add(file);
                } else if (file.Type == FileType.Content || file.Type == FileType.Security) {
                    continue;
                } else {
                    throw new ArgumentOutOfRangeException();
                }
            }
            return files;
        }

        public static void WriteHeading(string s, TextWriter writer) {
            writer.WriteLine(
                @"-- ===========================================================
--  {0}
-- ===========================================================",
                s);
        }

        /// <summary>
        /// Returns tables in dependecies ordered collection.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        protected static IList GetTables(ScriptFile file, SelectionGroup group, IDatabase database) {
            IList tables = group.RemoveIgnored(database.Tables);
            tables = file.RemoveIgnored(tables);
            tables = database.SortTables(tables); 
            // Dependencies will be inserted, assuming that user is knows what
            // she's doing by ignoring parent objects
            tables = group.RemoveIgnored(tables);
            tables = file.RemoveIgnored(tables);
            return tables;
        }

        /// <summary>
        /// Creating(with clearing) script folder tree
        /// </summary>
        public static void MakeEmptyDirectory(string path) {
            log.DebugFormat("MakeEmptyDirectory: {0}", path);
            lock (log) {
                if (Directory.Exists(path)) {
                    Directory.Delete(path, true);
                }                
            } 
            Directory.CreateDirectory(path);
        }
    }
}