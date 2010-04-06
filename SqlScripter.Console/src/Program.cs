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
// $Id: Program.cs 53 2009-08-14 16:56:42Z janusz.skonieczny $
#endregion
using System;
using System.IO;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using NAnt.Core.Types;
using NAnt.Core.Util;
using Qualent.Logging;
using SqlScripter.Core.src;
using SqlScripter.ScripterProject;
using WooYek.Common.Logging;

namespace SqlScripter {
    internal class Program {
        private static readonly ILog log = LogManager.GetLogger(typeof (Program));

        [STAThread]
        private static void Main(string[] args) {
            Console.WriteLine("SQLSCripter " + typeof(Program).Assembly.GetName().Version);
            Console.WriteLine("Copyright (C) 2008 Janusz Skonieczny.");
            Console.WriteLine("All Rights Reserved.");
            Console.WriteLine("");

            XmlConfigurator.Configure();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_OnUnhandledException);
//            Application.ThreadException += Application_OnThreadException;
            try {
                CommandLineOptions options = new CommandLineOptions();
                CommandLineParser clp = new CommandLineParser(typeof (CommandLineOptions), true);
                clp.Parse(args, options);


                if(options.ShowHelp) {
                    Console.WriteLine(clp.Usage);
                    return;
                }

                if(options.Verbose) {
                    LoggingConfig.ReconfigureConsoleAppender(Level.Debug, "[%d{HH:mm:ss,fff}] %-5p [%-25t] %m%n");
                    LogUtil.LogInformation(log);
                }

                if (options.GenerateProject) {
                    if (string.IsNullOrEmpty(options.DatabaseName)) {
                        Console.Out.WriteLine("Please provide at least database name");
                        Console.Out.WriteLine("");
                        Console.WriteLine(clp.Usage);
                    }
                    ProjectUtils.GenerateDefault(options.ServerName, options.DatabaseName);
                    return;
                }

                Project project = null;
                if (string.IsNullOrEmpty(options.ProjectFile)) {
                    project = Project.Load();
                } else {
                    project = Project.Load(options.ProjectFile);
                }
                if (project != null) {
                    project.RunScripting();
                } else {
                    Console.Out.WriteLine("");
                    Console.WriteLine(clp.Usage);
                    return;
                }

            } catch (Exception ex) {
                log.Error("Main: failed", ex);
            }
        }

        private static void CurrentDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            if (e.ExceptionObject is Exception) {
                log.Error("Unhadled exception on appdomain", (Exception) e.ExceptionObject);
            } else {
                log.Error("Unhadled exception on appdomain: " + e.ExceptionObject);
            }
        }

        private static void Application_OnThreadException(object sender, ThreadExceptionEventArgs e) {
            log.Error("Application_OnThreadException: failed", e.Exception);
        }
    }
}