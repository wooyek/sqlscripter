/* Copyright 2008 Janusz Skonieczny
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
 * $Id: Guard.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
 */

using System;
using System.Data;
using System.Runtime.Serialization;

namespace Qualent.Util {
    public partial class Guard {
        public static void ArgumentNotNull(object argument, string argumentName) {
            if (argument == null) {
                throw new ArgumentNullException(argumentName);
            }
        }

        [Obsolete("Use StringNotEmpty")]
        public static void StringNotNull(object var, string varName) {
            if (string.IsNullOrEmpty((string)var)){
                throw new InvalidOperationException(varName + " is null or empty string");
            }
        }

        public static void StringNotEmpty(object var, string varName) {
            StringNotEmpty(var, varName, null);
        }

        public static void StringNotEmpty(object var, string varName, string message) {
            if (string.IsNullOrEmpty((string)var)){
                throw new InvalidOperationException(varName + " is null or empty string. "+message);
            }
        }

        public static void NotNull(object var, string varName) {
            if (var == null) {
                throw new InvalidOperationException(varName + " is null");
            }
        }
        public static void NotNull(object var, string varName, string additionalMessage) {
            if (var == null) {
                throw new InvalidOperationException(varName + " is null. "+additionalMessage);
            }
        }
        public static void NotNull(object var, string varName, string additionalMessage, params object[] args) {
            if (var == null) {
                throw new InvalidOperationException(varName + " is null. "+String.Format(additionalMessage,args));
            }
        }

        public static void ArrayNotEmpty(Array array, string argumentName) {
            if (array == null) {
                throw new ArgumentNullException(argumentName);
            }
            if (array.Length < 1) {
                throw new ArgumentException("Array must have at least one value", argumentName);
            }
        }

        public static void NotDBNull(DataRow dr, string columnName) {
            if (Convert.IsDBNull(dr[columnName])) {
                throw new InvalidOperationException(columnName + " is DBNull");
            }
        }
        public static void NotEmpty(string argument, string paramName) {
            if (argument == null) {
                throw new ArgumentNullException(paramName);
            }
            if (argument == String.Empty) {
                throw new ArgumentEmptyException(paramName, "Argument '{" + paramName + "}' was String.Empty");
            }
        }
        public static void NotEmpty(string argument, string paramName, string exceptionMessage) {
            if (argument == null) {
                throw new ArgumentNullException(paramName, exceptionMessage);
            }
            if (argument == String.Empty) {
                throw new ArgumentEmptyException(paramName, exceptionMessage);
            }
        }
        public static void NotZero(double var, string varName) {
            if (var == 0) {
                throw new ArgumentOutOfRangeException(varName + " is zero.");
            }
        }

        public static void NotZero(double var, string varName, string additionalMessage) {
            if (var == 0) {
                throw new ArgumentOutOfRangeException(varName + " is zero. " + additionalMessage);
            }
        }


    }
    public class ArgumentEmptyException : ArgumentException {
        public ArgumentEmptyException(string paramName, string message) : base(message, paramName) { }
    }
    public class AssertionException : ApplicationException {
        public AssertionException(string message) : base(message) {}
        public AssertionException() {}
        public AssertionException(SerializationInfo info, StreamingContext context) : base(info, context) {}
        public AssertionException(string message, Exception innerException) : base(message, innerException) {}
    }
}