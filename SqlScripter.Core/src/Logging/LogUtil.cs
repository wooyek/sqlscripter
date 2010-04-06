using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;

namespace Qualent.Logging {
    public class LogUtil {
        /// <summary>
        /// Calls <see cref="LogInformation(ILog,Assembly)"/> with <see cref="Assembly.GetEntryAssembly"/>
        /// </summary>
        /// <param name="log"></param>
        public static void LogInformation(ILog log) {
            LogInformation(log, Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Logs Environment and Assembly informaction.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="assembly"></param>
        public static void LogInformation(ILog log, Assembly assembly) {
            log.InfoFormat("Environment.OSVersion:        {0}", Environment.OSVersion);
            log.InfoFormat("Environment.Version:          {0}", Environment.Version);
            log.InfoFormat("Environment.CommandLine:      {0}", Environment.CommandLine);
            log.InfoFormat("Environment.CurrentDirectory: {0}", Environment.CurrentDirectory);
            log.InfoFormat("Environment.MachineName:      {0}", Environment.MachineName);
            log.InfoFormat("Environment.ProcessorCount:   {0}", Environment.ProcessorCount);
            log.InfoFormat("Environment.SystemDirectory:  {0}", Environment.SystemDirectory);
            log.InfoFormat("Environment.UserDomainName:   {0}", Environment.UserDomainName);
            log.InfoFormat("Environment.UserName:         {0}", Environment.UserName);
            log.InfoFormat("Environment.UserInteractive:  {0}", Environment.UserInteractive);
            log.InfoFormat("Assembly: {0}", assembly.FullName);
            log.InfoFormat("Version:  {0}", assembly.GetName().Version);
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0) {
                log.InfoFormat("Description:   {0}", ((AssemblyDescriptionAttribute)attributes[0]).Description);
            }
            object[] attributes2 = assembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            if (attributes2 != null && attributes2.Length > 0) {
                log.InfoFormat("Configuration: {0}", ((AssemblyConfigurationAttribute)attributes2[0]).Configuration);
            }
        }
    }
}
