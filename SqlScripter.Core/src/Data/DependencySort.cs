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
// $Id: DependencySort.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
#endregion
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;

namespace Qualent.Data {
    public class DependencySort {
        /// <summary>
        /// A heleper mthod that sorts named dependecies.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="table"></param>
        /// <returns></returns>
        public static StringCollection Sort(DataTable table){
            if (!table.Columns.Contains("Priority")) {
                throw new ArgumentException("Source table must have an int column named Priority");
            }
            if (!table.Columns.Contains("depends")) {
                throw new ArgumentException("Source table must have a string column named depends, containing dependency of name");
            }
            if (!table.Columns.Contains("name")) {
                throw new ArgumentException("Source table must have a string column named name, containing name of object that can depend on others");
            }
            foreach (DataRow row in table.Rows) {
                if (Convert.IsDBNull(row["depends"])) {
                    Console.Out.WriteLine("Undependent: " + row["name"]);
                    row["Priority"] = 1;
                }
            }

            int priority = 1;
            DataRow[] left = table.Select("Priority = 0");
            DataRow[] possibleDepnedencies = table.Select("Priority = 0");
            do {
                priority++;
                foreach (DataRow row in left) {
                    string dependency = row["depends"].ToString();
                    //System.Console.Out.WriteLine("# " + row["depends"] );
                    bool found = IsTableInCollection(dependency, possibleDepnedencies);
                    if (!found) {
                        row["Priority"] = priority;
                        CheckOtherDependencies(row, table);
                    }
                }
                left = table.Select("Priority = 0");
                possibleDepnedencies = table.Select("Priority = 0");
            } while (left.Length > 0);
            DataRow[] rows = table.Select("", "Priority ASC");
            
            StringCollection names = new StringCollection();
            foreach (DataRow row in rows) {
                string name = row["name"].ToString();
                if (!names.Contains(name)) {
                    names.Add(name);
                }
            }
            return names;
        }

        private static void CheckOtherDependencies(DataRow row, DataTable table) {
            DataRow[] leftOvers = table.Select("Priority = 0");
            string tableName = row["name"].ToString();
            if (!DoesHaveOtherDependencies(tableName, leftOvers)) {
                Console.Out.WriteLine("Dependency done: " + row["Priority"] + " " + row["name"]);
            }
        }

        private static bool DoesHaveOtherDependencies(string tableName, DataRow[] leftOvers) {
            bool stillHaveDeps = false;
            foreach (DataRow dr in leftOvers) {
                if (dr["name"].Equals(tableName)) {
                    stillHaveDeps = true;
                }
            }
            return stillHaveDeps;
        }

        private static bool IsTableInCollection(string tableName, DataRow[] collection) {
            foreach (DataRow table in collection) {
                string name = table["name"].ToString();
                if (tableName.Equals(name)) {
                    //                            System.Console.Out.WriteLine("## "+row["name"] + " depneds on " + name);
                    return true;
                }
            }
            return false;
        }
    }
}
