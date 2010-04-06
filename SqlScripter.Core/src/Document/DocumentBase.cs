/* Copyright 2005-2007 Janusz Skonieczny
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
 * $Id: DocumentBase.cs 19 2009-01-16 10:53:14Z janusz.skonieczny $
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WooYek.Common.Doc {
    /// <summary>
    /// A base class for XML serialization based documents.
    /// </summary>
    public abstract class DocumentBase<T>
        where T : DocumentBase<T> {
        private string path;
        private bool secure = false;
        private Version documentSchemaVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public virtual void Store() {
            if (Path == null) {
                throw new InvalidOperationException("Path is not set");
            }
            Store(Path);
        }

        /// <summary>
        /// Stores current object as an XML file. See <see cref="XmlSerializer"/>. 
        /// </summary>
        /// <param name="filePath">File to which object will be serialized.</param>
        public virtual void Store(string filePath) {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists) {
                fi.Directory.Create();
            }
            Console.Out.WriteLine("Storing document in: " + filePath);
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
            try {
                Store(fileStream);
            } finally {
                fileStream.Close();
            }
            Path = filePath;
        }

        private void Store(Stream stream) {
            XmlSerializer serializer = new XmlSerializer(GetType());
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            try {
                serializer.Serialize(writer, this);
            } finally {
                writer.Flush();
            }
        }

        public virtual MemoryStream Store2Stream()
        {
            MemoryStream memoryStream = new MemoryStream();
            Store(memoryStream);
            return memoryStream;
        }

        /// <summary>
        /// Loades an object ot type T derived from <see cref="DocumentBase"/> from an XML file. See <see cref="XmlSerializer"/>. 
        /// </summary>
        /// <param name="filePath">File with serilized <see cref="Type"/></param>
        /// <returns>A instance of type specified by documentType parameter</returns>
        public static T Load(string filePath) {
            if (filePath == null) {
                throw new ArgumentNullException("filePath",
                                                "Document path must be give in order to load a document");
            }
            XmlTextReader reader = new XmlTextReader(filePath);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            try {
                object o = serializer.Deserialize(reader);
                T document = (T) o;
                document.Path = filePath;
                return document;
            } finally {
                reader.Close();
            }
        }
        /// <summary>
        /// Loades an object ot type T derived from <see cref="DocumentBase"/> from an XML. See <see cref="XmlSerializer"/>. 
        /// </summary>
        /// <param name="xmlDocument"><see cref="XmlDocument"/> with serilized <see cref="Type"/></param>
        /// <returns>A instance of type specified by documentType parameter</returns>
        public static T Load(XmlDocument xmlDocument) {
            if (xmlDocument == null) {
                throw new ArgumentNullException("xmlDocument");
            }
            XmlNodeReader reader = new XmlNodeReader(xmlDocument.DocumentElement);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            try {
                object o = serializer.Deserialize(reader);
                T document = (T) o;
                return document;
            } finally {
                reader.Close();
            }
        }

        [XmlIgnore]
        public virtual string Path {
            get { return path; }
            set { path = value; }
        }

        public virtual String DocumentSchemaVersion {
            get { return documentSchemaVersion.ToString(); }
            set {
                Version readDocVersion = new Version(value);
                if (readDocVersion.CompareTo(MinVersion) < 0) {
                    string msg =
                        "Ta wersja programu {0} nie wspiera odczutu dokumentów z wersji programu starszych ni¿ {1}";
                    throw new DocumentNotSupported(msg, documentSchemaVersion, MinVersion);
                }
            }
        }

        /// <summary>
        /// A minimum supported <see cref="Version"/> of the document that can be deserialized.
        /// </summary>
        [XmlIgnore]
        public virtual Version MinVersion {
            get { return new Version(0, 0, 0, 0); }
        }
    }


    public class DocumentNotSupported : ApplicationException {
        public DocumentNotSupported(string message) : base(message) {}
        public DocumentNotSupported(string message, params object[] args) : base(string.Format(message, args)) {}

        public DocumentNotSupported(string message, Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException) {}

//        public DocumentNotSupported(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class EqalityHelper<T> {
        public static bool Equals(List<T> list, List<T> other) {
            if (list.Count != other.Count) {
                return false;
            }
            foreach (T o in list) {
                if (!other.Contains(o)) {
                    return false;
                }
            }
            return true;
        }
    }
}