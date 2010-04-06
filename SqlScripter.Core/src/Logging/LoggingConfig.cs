/* Copyright 2003-2007 Janusz Skonieczny
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Created by: WooYek on 13:20:43 2003-01-30
 *
 * Last changes made by:
 * $Id: LoggingConfig.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Qualent.Util;

namespace WooYek.Common.Logging {
    public class LoggingConfig {
        private static bool loggingConfigured;
        public static int RollingFileBackups = 3;
        public static int RollingFileMaxSize = 2000000;
        public static bool RollingAppendToFile = false;
        private static string logsDirectory;

        public static void ConfigureLogging(string levelName, string logsDir) {
            if (loggingConfigured) {
                return;
            }
            if (!string.IsNullOrEmpty(levelName)) {
                switch (levelName.ToUpper()) {
                    case "DEBUG":
                        ConfigureLogging(Level.Debug, logsDir);
                        break;
                    case "INFO":
                        ConfigureLogging(Level.Info, logsDir);
                        break;
                    case "WARN":
                        ConfigureLogging(Level.Warn, logsDir);
                        break;
                    case "ERROR":
                        ConfigureLogging(Level.Error, logsDir);
                        break;
                    case "FATAL":
                        ConfigureLogging(Level.Fatal, logsDir);
                        break;
                    case "OFF":
                        ConfigureLogging(Level.Off, logsDir);
                        break;
                }
            }
        }

        public static void ConfigureLoggingWithEntryAssembly(Level level) {
            if (loggingConfigured) {
                return;
            }
#if !Smartphone && !PocketPC
            Assembly assembly = Assembly.GetEntryAssembly();
#else
            Assembly assembly = Assembly.GetCallingAssembly();
#endif
            ConfigureLogging(level, assembly);
        }

        public static DirectoryInfo GetAssemblyDir(Assembly assembly) {
#if !Smartphone && !PocketPC
            Guard.NotNull(assembly, "assembly");
            FileInfo fi = new FileInfo(assembly.Location);
            return fi.Directory;
#else
            Uri uri = new Uri(assembly.GetName().CodeBase);
            FileInfo fi = new FileInfo(uri.LocalPath);
            return fi.Directory;
#endif
        }

        public static void ConfigureLoggingWithExecutingAssembly(Level level) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            ConfigureLogging(level, assembly);
        }

        public static void ConfigureLogging(Level level, Assembly assembly) {
            DirectoryInfo di = GetAssemblyDir(assembly);
            string logName = GetLogName(assembly);
            ConfigureLogging(level, di.FullName, logName);
        }

        public static string GetLogName(Assembly assembly) {
            string assemblyName = assembly.GetName().Name;
            return assemblyName + ".log";
        }

        public static void ConfigureLogging(Level level, string logsDir) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string logName = GetLogName(assembly);
            ConfigureLogging(level, logsDir, logName);
        }

        public static void ConfigureLogging(Level level, string logsDir, string logName) {
            
            if (loggingConfigured) {
                return;
            }
            logsDirectory = logsDir;
            IAppender a1 = GetConsoleAppender(level);

            string file = logsDir + Path.DirectorySeparatorChar + logName ;
            IAppender a2 = GetRollingFileAppender(level, file, RollingFileBackups, RollingFileMaxSize);
#if !Smartphone && !Log4netCF

            string friendlyName = AppDomain.CurrentDomain.FriendlyName;
            IAppender a3 = GetEventLogAppender(Level.Warn, "Application", friendlyName);
            ConfigureAppenders(a1, a2, a3);
#else
            ConfigureAppenders(a1, a2);
//            ConfigureAppenders(a1);
#endif
        }

        public static void ConfigureAppenders(params IAppender[] appenders) {
            IBasicRepositoryConfigurator repository = (IBasicRepositoryConfigurator) LogManager.GetRepository();
            for (int i = 0; i < appenders.Length; i++) {
                IAppender appender = appenders[i];
                repository.Configure(appender);
            }
            loggingConfigured = true;
        }

#if !Smartphone && !Log4netCF

        public static EventLogAppender GetEventLogAppender(Level level, string logName, string applicationName) {
            EventLogAppender eventLogAppender = new EventLogAppender();
            eventLogAppender.LogName = logName;
            eventLogAppender.SecurityContext = NullSecurityContext.Instance;
            eventLogAppender.ApplicationName = applicationName;
            eventLogAppender.Layout = new PatternLayout(@"Time:   %d{yyyy-MM-dd HH:mm:ss,fff}
Level:  %-5p
Thread: %t
Logger: %c
Message:
%m");
            eventLogAppender.Threshold = level;
            return eventLogAppender;
        }
#endif

        public static ConsoleAppender GetConsoleAppender(Level level) {
            ConsoleAppender consoleAppender = new ConsoleAppender();
            consoleAppender.Name = "Console";
            consoleAppender.Layout = new PatternLayout("[%d{HH:mm:ss,fff}] %-5p [%t] %c{2} - %m%n");
            consoleAppender.ActivateOptions();
            consoleAppender.Threshold = level;
            
            LoggerMatchFilter filter = new LoggerMatchFilter();
            filter.AcceptOnMatch = false;
            filter.LoggerToMatch = "NHibernate";
            consoleAppender.AddFilter(filter);

            return consoleAppender;
        }

        public static RollingFileAppender GetRollingFileAppender(Level level, string file, int rollBackups,
                                                                 int maxFileSize) {
            RollingFileAppender rollingFileAppender = new RollingFileAppender();
            rollingFileAppender.Name = "RollingFile";
            System.Console.Out.WriteLine("RollingFile = " + file);
            rollingFileAppender.File = file;
            rollingFileAppender.AppendToFile = RollingAppendToFile;
            rollingFileAppender.MaxSizeRollBackups = rollBackups;
            rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            rollingFileAppender.MaxFileSize = maxFileSize;
            rollingFileAppender.Layout = new PatternLayout("[%d{yyyy-MM-dd HH:mm:ss,fff}] %-5p [%t] %c{2} - %m%n");
            rollingFileAppender.ActivateOptions();
            rollingFileAppender.Threshold = level;
            LoggerMatchFilter filter = new LoggerMatchFilter();
            filter.AcceptOnMatch = false;
            filter.LoggerToMatch = "NHibernate";
            rollingFileAppender.AddFilter(filter);
            return rollingFileAppender;
        }

#if !Smartphone && !Log4netCF
        public static void AddMapping(EventLogAppender eventLogAppender, Level level,
                                      EventLogEntryType eventLogEntryType) {
            EventLogAppender.Level2EventLogEntryType mapping = new EventLogAppender.Level2EventLogEntryType();
            mapping.Level = level;
            mapping.EventLogEntryType = eventLogEntryType;
            eventLogAppender.AddMapping(mapping);
        }
#endif

        public static void ConfigByAssemblyConfigFile() {
            XmlConfigurator.Configure();
        }

        public static string LogsDirectory {
            get { return logsDirectory; }
        }

        public static void ReconfigureConsoleAppender(Level threshold, string conversionPattern) {
            ConsoleAppender appender = GetConsoleAppender(threshold);
            IAppender[] appenders = LogManager.GetRepository().GetAppenders();
            foreach (IAppender a in appenders) {
                if (a.Name == "Console") {
                    ((Hierarchy) LogManager.GetRepository()).Root.RemoveAppender("Console");
                    ((IBasicRepositoryConfigurator)LogManager.GetRepository()).Configure(appender);
                    a.Close();
                    break;
                }
            }
        }

    }
}