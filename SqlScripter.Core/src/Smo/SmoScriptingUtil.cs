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
// $Id: SmoScriptingUtil.cs 66 2009-11-05 11:36:51Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using log4net;
using Microsoft.SqlServer.Management.Smo;

namespace WooYek.Smo {
    internal class SmoScriptingUtil {
        private static readonly ILog log = LogManager.GetLogger(typeof (SmoScriptingUtil));
        public static void Write(StringCollection stringCollection, TextWriter writer) {
            foreach (string s in stringCollection) {
                Write(s, writer);
            }
        }

        private static void Write(string s, TextWriter writer) {
            writer.WriteLine(s);
            writer.WriteLine("GO");
            writer.WriteLine("");
        }

        public static void Script(IScriptable scriptable, string fileName, ScriptingOptions options) {
            StringCollection stringCollection = Script(scriptable, options);
            if (stringCollection == null) {
                return;
            }
            TextWriter writer = new StreamWriter(fileName, true, Encoding.UTF8);
            try {
                Write(stringCollection, writer);
                writer.Flush();
            } finally {
                writer.Close();
            }
        }

        public static StringCollection Script(IScriptable scriptable, ScriptingOptions options) {
            if (IsIgnored(scriptable)) {
                log.DebugFormat("Script: {0} ignored", scriptable);
                return null;
            }
            log.InfoFormat("Script: {0}", scriptable);
            StringCollection collection = scriptable.Script(options);
            return Cleanse(collection);
        }

        /// <summary>
        /// SQL Server scripting Hack, fidl out how to do this with option settings
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static StringCollection Cleanse(StringCollection collection) {
            // Alternative method for removing CREATE from stored procedure security
//            if (!options.Default && collection.Count > 0) {
//                collection[0] = null;
//            }
            for (int i = 0; i < collection.Count; i++) {
                string s = (string) collection[i];
                if (string.IsNullOrEmpty(s)) {
                    continue;
                }
                s = s.Replace("PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, ", "");
                s = s.Replace(", ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON", "");
                s = s.Replace("DEFAULT ((0))", "DEFAULT (0)");
                s = s.Replace("DEFAULT ((1))", "DEFAULT (1)");
            
                collection[i] = s;
            }
            return collection;
        }

        public static bool IsIgnored(IScriptable scriptable) {
            return SmoUtils.IsSystemObject(scriptable, true);
        }

        public static ScriptingOptions GetCreateOptions(SqlServerVersion version) {
            ScriptingOptions options = new ScriptingOptions();
            SetCommonOptions(options, version);
            options.ScriptDrops = false;
            options.IncludeIfNotExists = false;
            options.NoFileGroup = true;
            return options;
        }

        ///problems occured with scripting "create" statement
        ///while scripting only ! permisions on stored procedures
        public static ScriptingOptions GetSecurityOptions(SqlServerVersion version) {
            ScriptingOptions options = new ScriptingOptions();
            options.TargetServerVersion = version;
            options.AllowSystemObjects = false;
            options.Default = false;
            options.DdlHeaderOnly = true;
            options.DdlBodyOnly = false;
            options.PrimaryObject = false;
            options.Permissions = true;
            options.NoIdentities = true;
            options.NoAssemblies = true;
            
            return options;
        }

        /// <summary>
        /// Common scripting options settings
        /// </summary>
        public static void SetCommonOptions(ScriptingOptions options, SqlServerVersion version) {
            //TODO: test option switches, dump unnecessery, consider oving to pplication settings 
            options.DriDefaults = true;
            options.TargetServerVersion = version;
            options.SchemaQualifyForeignKeysReferences = true;
            options.ExtendedProperties = false;
            options.Indexes = true;
            options.ClusteredIndexes = true;
            options.NonClusteredIndexes = true;
            options.Permissions = true;
            options.Triggers = true;
            options.Default = true;
            options.AllowSystemObjects = false;
            options.DriIncludeSystemNames = true;
            
//            options.DriAll = true;
//            options.DriAllConstraints = true;
//            options.DriChecks = true;
//            options.DriAllKeys = true;
//            options.DriClustered = true;
//            options.DriForeignKeys = true;
//            options.DriIncludeSystemNames = true;
//            options.DriIndexes = true;
//            options.DriNonClustered = true;
//            options.DriPrimaryKey = true;
//            options.DriUniqueKeys = true;
//            options.WithDependencies = true;
//            options.DriAll = true;
//            options.DriAllConstraints = true;
//            options.DriChecks = true;
//            options.DriAllKeys = true;
//            options.DriClustered = true;
//            options.DriForeignKeys = true;
//            options.DriIncludeSystemNames = true;
//            options.DriIndexes = true;
//            options.DriNonClustered = true;
//            options.DriPrimaryKey = true;
//            options.DriUniqueKeys = true;
//            options.DriDefaults = true;
//            options.TargetServerVersion = version;
//            options.SchemaQualifyForeignKeysReferences = true;
//            options.ExtendedProperties = false;
//            options.Indexes = true;
//            options.ClusteredIndexes = true;
//            options.NonClusteredIndexes = true;
//            options.Permissions = true;
//            options.Triggers = true;
//            options.Default = true;
//            options.WithDependencies = true;
//            options.AllowSystemObjects = false;

            
        }

        public static void Script(ICollection collection, string extensionPart, string folder, SqlServerVersion version) {

            foreach (IScriptable scriptable in collection) {
                ScriptSchemaObjectBase o = (ScriptSchemaObjectBase) scriptable;
                string fileName = folder + @"\" + (o.Schema + "." + o.Name) + "." + extensionPart;
                //                Script(scriptable, fileName, GetDropOptions());
                ScriptingOptions options = GetCreateOptions(version);
                options.DriAll = true; //included for with-split scripting creation
                Script(scriptable, fileName, options);
            }
        }

        /// <summary>
        /// Scripting collection of Iscriptable's 
        /// ver. for scripting to single file
        /// </summary>
        /// <param name="collection"></param>
        public static void Script(ICollection collection, StreamWriter writer, ScriptingOptions options, bool printObjectsCount, string what) {
            if (printObjectsCount) {
                log.InfoFormat("Script: {0} in {1} collection", collection.Count, what);
            }
            foreach (IScriptable scriptable in collection) {
                Script(scriptable, writer, options);
            }
        }
        
        /// <summary>
        /// Scripting collection of Iscriptable's 
        /// ver. for scripting to single file
        /// </summary>
        /// <param name="collection"></param>
        public static void ScriptSchemas(ICollection collection, StreamWriter writer, ScriptingOptions options, bool printObjectsCount) {
            if (printObjectsCount) {
                log.InfoFormat("Script: {0} in schemas collection ", collection.Count);
            }
            foreach (Microsoft.SqlServer.Management.Smo.Schema schema in collection) {
                string script = @"CREATE SCHEMA [{0}] AUTHORIZATION [dbo]";
                script = string.Format(script, schema.Name);
                Write(script, writer);
            }
        }

        //temporary solution 
        //TODO: refactor
        public static void ScriptPersmissions(ICollection collection, StreamWriter writer, bool printObjectsCount, string what) {
            if (printObjectsCount) {
                log.InfoFormat("Script: {0} in {1} collection", collection.Count, what);
            }
            foreach (IObjectPermission o in collection) {
                    ObjectPermissionInfo[] opinfos = o.EnumObjectPermissions();
                    foreach(ObjectPermissionInfo opi in opinfos) {
                        string permissionType = opi.PermissionType.ToString();
                        string schema = opi.Grantor;
                        ScriptSchemaObjectBase ssob = o as ScriptSchemaObjectBase;
                        if (ssob != null) {
                            schema = ssob.Schema;
                        }
                        string name = opi.ObjectName;
                        string grantee = opi.Grantee;
                        
                        //TODO: remove this ugly temporary hack to limit perms scripting to a specyfic user
                        if (!grantee.Equals(schema+"User")) {
                            continue;
                        }

                        string script = (string.Format("GRANT {0} ON [{1}].[{2}] TO [{3}]", permissionType, schema, name, grantee));

                        Write(script, writer);
                    }
            }
        }

        
        public static void Script(IScriptable scriptable, TextWriter writer, ScriptingOptions options) {
            StringCollection stringCollection = Script(scriptable, options);
            if (stringCollection == null) {
                return;
            }
            Write(stringCollection, writer);
        }
    }
}