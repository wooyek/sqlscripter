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
// $Id: SmoUtils.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
#endregion
using System;
using System.Collections.Specialized;
using System.Data;
using System.Reflection;
using Microsoft.SqlServer.Management.Smo;
using Qualent.Data;
using Qualent.Util;
using WooYek.Common.Database;

namespace WooYek.Smo {
    public class SmoUtils {
        public static Database GetDb(string serverName, string dbName) {
            //init pi objects
            Server server = new Server(serverName);
            Database db = server.Databases[dbName];
            Guard.NotNull(db, "db");

            //tuning pi performance 
            server.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");
            server.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject");
            server.SetDefaultInitFields(typeof(View), "IsSystemObject");
            server.SetDefaultInitFields(typeof(Table), "IsSystemObject");
            server.SetDefaultInitFields(typeof(Column), "Identity");

            return db;
        }

        public static bool IsSystemObject(Object o) {
            return IsSystemObject(o, true);
        }

        /// <summary>
        /// Chcecks if an obejct is a system object.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreNoIsSystemObjectProperty">If an a given object will not have a property "IsSystemObject" 
        /// then an exception will be thrown if this is false. If property is missing and this is true, an abject will be assumed 
        /// as not a system object.</param>
        /// <returns></returns>
        public static bool IsSystemObject(Object o, bool ignoreNoIsSystemObjectProperty) {
            Guard.NotNull(o, "o");
            Type type = o.GetType();
            PropertyInfo isSystemObjectProperty = type.GetProperty("IsSystemObject");
            PropertyInfo schemaProperty = type.GetProperty("Schema");
            if (schemaProperty != null) {
                object schema = schemaProperty.GetValue(o, null);
                if ("sys".Equals(schema)) {
                    return true;
                }
            }
            if (isSystemObjectProperty != null) {
                if ((bool)isSystemObjectProperty.GetValue(o, null)) {
                    return true;
                }
            } else {
                if (!ignoreNoIsSystemObjectProperty) {
                    string s = string.Format("Cannot check if \"{0}\" is system object, there is no IsSystemObject property on type.", o);
                    throw new InvalidOperationException(s);
                }
            }
            if (o is NamedSmoObject && "sysdiagrams".Equals(((NamedSmoObject)o).Name)) {
                return true;
            }
            if (o is Index && "sysdiagrams".Equals(((NamedSmoObject)((Index)o).Parent).Name)) {
                return true;
            }
            if (o is Schema) {
                string schemaName = ((Schema) o).Name;
                switch(schemaName) {
                    case "sys":
                    case "INFORMATION_SCHEMA":
                    case "guest":
                    case "dbo":
                    case "db_securityadmin":
                    case "db_owner":
                    case "db_denydatawriter":
                    case "db_denydatareader":
                    case "db_ddladmin":
                    case "db_datawriter":
                    case "db_datareader":
                    case "db_backupoperator":
                    case "db_accessadmin":
                        return true;
                }
            }
            return false;
        }

        public static StringCollection GetTablesWithDependenciesSorted(string serverName, string dbName) {
            string cmd =
                @"SELECT  0 as Priority, dep.name as name, dep.id, 
       case when dep.name = depc.name then null else depc.name end as depends
   FROM sysobjects dep
       LEFT OUTER JOIN sysforeignkeys fk on dep.id = fk.fkeyid
       LEFT OUTER JOIN sysobjects depc on fk.rkeyid = depc.id
   WHERE 
   dep.xtype = 'U' AND dep.name <> 'dtproperties' 
";
            SqlCommon sql = SqlCommon.New(dbName, serverName);
            DataTable table = sql.Exec(cmd).Tables[0];
            return DependencySort.Sort(table);
        }
    }
}