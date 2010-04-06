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
 * $Id: AppConfigHelper.cs 5806 2007-11-26 13:05:22Z wooyek $
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using log4net;

namespace WooYek.Configuration {
    public class AppConfigHelper {
        private static ILog log = LogManager.GetLogger(typeof (AppConfigHelper));
        
        public static int? Get(string key, int? defaultValue) {
            string s = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(s)) {
                log.WarnFormat("The key {1} is not configured, using default: {0}", defaultValue, key);
            } else {
                int v;
                if (!int.TryParse(s, out v)) {
                    log.WarnFormat("The key {1} is not properly configured, using default: {0}", defaultValue, key);
                } else {
                    defaultValue = v;
                    log.InfoFormat("Get: {0}={1}", key, defaultValue);
                }
            }
            return defaultValue;
        }
        
        public static bool? Get(string key, bool? defaultValue) {
            string s = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(s)) {
                log.WarnFormat("The key {1} is not configured, using default: {0}", defaultValue, key);
            } else {
                bool v;
                if (!bool.TryParse(s, out v)) {
                    log.WarnFormat("The key {1} is not properly configured, using default: {0}", defaultValue, key);
                } else {
                    defaultValue = v;
                    log.InfoFormat("Get: {0}={1}", key, defaultValue);
                }
            }
            return defaultValue;
        }

        public static T GetEnum<T>(string key, T defaultValue) {
            string s = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(s)) {
                log.WarnFormat("The key {1} is not configured, using default: {0}", defaultValue, key);
                return defaultValue;
            } else {
                T v = (T) Enum.Parse(typeof (T), s);
                log.InfoFormat("Get: {0}={1}", key, defaultValue);
                return v;
            }
        }
        
        public static string Get(string key, string defaultValue) {
            string s = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(s)) {
                log.WarnFormat("The key {1} is not configured, using default: {0}", defaultValue, key);
                return defaultValue;
            }
            log.InfoFormat("Get: {0}={1}", key, s);
            return s;
        }

        public static DateTime Get(string key, DateTime defaultValue) {
            string s = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(s)) {
                log.WarnFormat("The key {1} is not configured, using default: {0}", defaultValue, key);
                return defaultValue;
            }
            DateTime result;
            if(!DateTime.TryParse(s, out result)) {
                log.WarnFormat("The key {0} is not properly configured, using default: {1} isntead of {3}", key, defaultValue, s);
                return defaultValue;
            }
            log.InfoFormat("Get: {0}={1}", key, s);
            return result; 
        }

        public static TimeSpan Get(string key, TimeSpan defaultValue) {
            string s = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(s)) {
                log.WarnFormat("The key {1} is not configured, using default: {0}", defaultValue, key);
                return defaultValue;
            }
            TimeSpan result;
            if (!TimeSpan.TryParse(s, out result)) {
                log.WarnFormat("The key {0} is not properly configured, using default: {1} isntead of {3}", key, defaultValue, s);
                return defaultValue;
            }
            log.InfoFormat("Get: {0}={1}", key, s);
            return result; 
        }
    }
}
