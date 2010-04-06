using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using log4net;
using Microsoft.SqlServer.Management.Smo;
using SqlScripter.ScripterProject;

namespace SqlScripter.Core.src.ScripterProject {
    public class User  {
        private static readonly ILog log = LogManager.GetLogger(typeof (User));
        private SelectionType selectionType = SelectionType.ExplicitExclude;
        private string name;

        public User() {}

        public User(string name, SelectionType selectionType) {
            this.name = name;
            this.selectionType = selectionType;
        }

        public User(Microsoft.SqlServer.Management.Smo.User User) {
            name = User.Name;
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
    }

    public class Users : List<User> {
        private static readonly ILog log = LogManager.GetLogger(typeof(Users));
        public Users() { }

        public Users(UserCollection Users) {
            Update(Users);
        }

        public void Update(UserCollection Users) {
            foreach (Microsoft.SqlServer.Management.Smo.User User in Users) {
                if (this[User.Name] == null) {
                    this.Add(User);
                }
            }
        }

        public void Add(SortedListCollectionBase elements) {
            foreach (Microsoft.SqlServer.Management.Smo.User User in elements) {
                Add(User);
            }
        }

        public void Add(Microsoft.SqlServer.Management.Smo.User User) {
            Add(new User(User));
        }

        public User this[string userName] {
            get {
                foreach (User User in this) {
                    if (User.Name.Equals(userName)) {
                        return User;
                    }
                }
                return null;
            }
        }

        public SelectionType? IsExcluded(Microsoft.SqlServer.Management.Smo.User User) {
            return IsExcluded(User.Name);
        }

        public bool IsExcluded(string userName, bool excludeMissing) {
            SelectionType? selectionType = IsExcluded(userName);
                    if (selectionType == SelectionType.Exclude) {
                        log.DebugFormat("{0,-35} Excluded explicitly", userName);
                        return true;
                    }

            if (selectionType == SelectionType.Include) {
                log.DebugFormat("{0,-35} Included explicitly", userName);
                return false;
            }
            if (excludeMissing) {
                log.DebugFormat("{0,-35} is missing and was Excluded by elements collection", userName);
            } else {
                log.DebugFormat("{0,-35} is missing and was Included by elements collection", userName);
            }
            return excludeMissing;
        }

        public SelectionType? IsExcluded(string userName) {
            User user = this[userName];
            if (user != null) {
                return user.SelectionType;
            }
            return null;
        }
    }
}
