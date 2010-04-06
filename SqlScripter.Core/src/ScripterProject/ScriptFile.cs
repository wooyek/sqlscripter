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
// $Id: ScriptFile.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
#endregion
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SqlScripter.ScripterProject {
    /// <summary>
    /// Represents a single SqlScript file.
    /// </summary>
    public class ScriptFile : SettingsBase {
        private FileType type;
        private string header = @"-- ===========================================================
--  $Id: ScriptFile.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
-- ===========================================================
-- !! Uwaga !!
-- Skrypt musi byc uruchomiony w kontekœcie dobrej bazy danych, œwiadomie wybranej przez u¿ytkownika
-- Nie wolno zmieniaæ kontekstu bazy danych w samym skrypcie, np. przez komende USE [nazwabazy]
";

        public ScriptFile() {}

        public ScriptFile(string fileName, FileType type) : base(fileName) {
            this.type = type;
        }

        [XmlAttribute()]
        public FileType Type {
            get { return type; }
            set { type = value; }
        }

        public string Header {
            get { return header; }
            set { header = value; }
        }
    }

    public class ScriptFiles : List<ScriptFile> {
        public ScriptFile Add(string fileName, FileType fileType) {
            ScriptFile sf = new ScriptFile(fileName, fileType);
            Add(sf);
            return sf;
        }
    }
}