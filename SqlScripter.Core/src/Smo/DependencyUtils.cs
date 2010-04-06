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
// $Id: DependencyUtils.cs 54 2009-08-17 17:15:42Z janusz.skonieczny $
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using WooYek.Collections.Generic;
using Qualent.Util;

namespace WooYek.Smo {
    public class DependencyUtils {
        private static readonly ILog log = LogManager.GetLogger(typeof (DependencyUtils));

        public static IList Sort(Database database, IList items) {
            return Sort(database, items, null);
        }

        public static IList Sort(Database database, IList items, string typeToSort) {
            Guard.NotNull(items, "items");
            log.InfoFormat("Sort: Sorting {0} {1} based on dependecies", items.Count, typeToSort);
            if(items.Count == 0) {
                log.DebugFormat("Sort: Noting to sort");
                return items;
            }

            List<SqlSmoObject> smoObjects = new List<SqlSmoObject>();
            IEnumerable<SqlSmoObject> enumerable = new CastingEnumerable<SqlSmoObject>(items);
            smoObjects.AddRange(enumerable);

            DependencyWalker dw = new DependencyWalker(database.Parent);
            SqlSmoObject[] smoObjectsArray = smoObjects.ToArray();
            DependencyTree dependencies = dw.DiscoverDependencies(smoObjectsArray, true);

            // Orginally DependencyTree was nested, don't know why. But it's casuing StackOveflow.
            // It stays commented till I confirm that not really needed.

//            DependencyTree dt; 
//            try {
//                dt = new DependencyTree(dependencies);
//            } catch (Exception e) {
//                throw new Exception("Sort: new DependencyTree on dependencies failed", e);
//            }

            smoObjects.Clear();

            foreach (DependencyCollectionNode d in dw.WalkDependencies(dependencies)) {
                if (typeToSort != null && d.Urn.Type != typeToSort) {
                    continue;
                }
//                log.DebugFormat("Sort: d.Urn={0}", d.Urn);
                string name = d.Urn.GetNameForType(d.Urn.Type);
                string schema = d.Urn.GetAttribute("Schema");
//                log.DebugFormat("Sort: schema={1} nameForType={0}", name, schema);
                SqlSmoObject o = Get(name, schema, typeToSort, database);
//                log.DebugFormat("Sort: view={0}", o);
                smoObjects.Add(o);
            }
            return smoObjects;
        }

        private static SqlSmoObject Get(string name, string schema, string type, Database database) {
            switch (type) {
                case "View":
                    return database.Views[name, schema];
                case "Table":
                    return database.Tables[name, schema];
                default:
                    throw new NotImplementedException("");
            }
        }
    }
}