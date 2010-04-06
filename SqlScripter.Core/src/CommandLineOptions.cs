using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core.Util;

namespace SqlScripter.Core.src
{
    public class CommandLineOptions
    {
        [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "verbose", ShortName = "v", Description = "Displays more information during execution")]
        public bool Verbose { get; set; }

        [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "databaseName", ShortName = "d", Description = "Specifies the anem of database to script")]
        public string DatabaseName { get; set; }

        private string serverName = ".";

        [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "serverName", ShortName = "s", Description = "Specifies the name of database server")]
        public string ServerName {
            get { return serverName; }
            set { serverName = value; }
        }

        [CommandLineArgument(CommandLineArgumentTypes.Exclusive, Name = "help", ShortName = "h", Description = "Prints this message")]
        public bool ShowHelp { get; set; }

        [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "generateProject", ShortName = "g", Description = "Generates a project file for given database")]
        public bool GenerateProject { get; set; }

        [DefaultCommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "ProjectFile", ShortName = "p", Description = "A scripting project file")]
        public string ProjectFile { get; set; }
    }
}
