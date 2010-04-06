using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using log4net;
using WooYek.Configuration;

namespace SqlScripter.Core.ScripterProject {
    public class ScriptWriter : IScriptWriter {
        private static readonly ILog log = LogManager.GetLogger(typeof (ScriptWriter));
        private StreamWriter backbone;
        private string filePathNoExtension;
        private string fileExtension;
        private int splitPosition = 50 * 1024 * 1024;
        private int fileIdex = 0;
        private string head;
        private string foot;

        public ScriptWriter(string path) {
            this.filePathNoExtension = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path);
            this.fileExtension = new FileInfo(path).Extension;
            ResetBacbone(path);
            splitPosition = (int)AppConfigHelper.Get("SqlScripter.SplitPosition", splitPosition);
        }

        private void ResetBacbone(string path) {
            if (backbone != null) {
                backbone.Dispose();
            }
            backbone = new StreamWriter(path, false, Encoding.UTF8);
        }

        public void SplitFileIfNeeded() {
            long position = backbone.BaseStream.Position;
            if (position < splitPosition) {
                return;
            }
            log.InfoFormat("SplitFileIfNeeded: Splitting file {0} after {1} position", filePathNoExtension, position);
            WriteLine(foot);
            ResetBacbone(GetNextFileName());
            WriteHead(head);
        }

        public string GetNextFileName() {
            fileIdex++;
            return this.filePathNoExtension + "." + (fileIdex).ToString("D3") + this.fileExtension;
        }

        public void WriteHead(string text) {
            this.FileHeader = text;
            Write(text);
        }

        public string FileHeader {
            set { this.head = value; }
            get { return this.head; }
        }

        public string FileFooter {
            set { this.foot = value; }
            get { return this.foot; }
        }

        public StreamWriter BaseWriter {
            get { return backbone; }
            set { backbone = value; }
        }

        public object GetLifetimeService() {
            return backbone.GetLifetimeService();
        }

        public object InitializeLifetimeService() {
            return backbone.InitializeLifetimeService();
        }

        public ObjRef CreateObjRef(Type requestedType) {
            return backbone.CreateObjRef(requestedType);
        }

        public void Dispose() {
            backbone.Dispose();
        }

        public void Write(bool value) {
            backbone.Write(value);
        }

        public void Write(int value) {
            backbone.Write(value);
        }

        public void Write(uint value) {
            backbone.Write(value);
        }

        public void Write(long value) {
            backbone.Write(value);
        }

        public void Write(ulong value) {
            backbone.Write(value);
        }

        public void Write(float value) {
            backbone.Write(value);
        }

        public void Write(double value) {
            backbone.Write(value);
        }

        public void Write(decimal value) {
            backbone.Write(value);
        }

        public void Write(object value) {
            backbone.Write(value);
        }

        public void Write(string format, object arg0) {
            backbone.Write(format, arg0);
        }

        public void Write(string format, object arg0, object arg1) {
            backbone.Write(format, arg0, arg1);
        }

        public void Write(string format, object arg0, object arg1, object arg2) {
            backbone.Write(format, arg0, arg1, arg2);
        }

        public void Write(string format, params object[] arg) {
            backbone.Write(format, arg);
        }

        public void WriteLine() {
            backbone.WriteLine();
        }

        public void WriteLine(char value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(char[] buffer) {
            backbone.WriteLine(buffer);
        }

        public void WriteLine(char[] buffer, int index, int count) {
            backbone.WriteLine(buffer, index, count);
        }

        public void WriteLine(bool value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(int value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(uint value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(long value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(ulong value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(float value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(double value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(decimal value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(string value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(object value) {
            backbone.WriteLine(value);
        }

        public void WriteLine(string format, object arg0) {
            backbone.WriteLine(format, arg0);
        }

        public void WriteLine(string format, object arg0, object arg1) {
            backbone.WriteLine(format, arg0, arg1);
        }

        public void WriteLine(string format, object arg0, object arg1, object arg2) {
            backbone.WriteLine(format, arg0, arg1, arg2);
        }

        public void WriteLine(string format, params object[] arg) {
            backbone.WriteLine(format, arg);
        }

        public IFormatProvider FormatProvider {
            get { return backbone.FormatProvider; }
        }

        public string NewLine {
            get { return backbone.NewLine; }
            set { backbone.NewLine = value; }
        }

        public void Close() {
            backbone.Close();
        }

        public void Flush() {
            backbone.Flush();
        }

        public void Write(char value) {
            backbone.Write(value);
        }

        public void Write(char[] buffer) {
            backbone.Write(buffer);
        }

        public void Write(char[] buffer, int index, int count) {
            backbone.Write(buffer, index, count);
        }

        public void Write(string value) {
            backbone.Write(value);
        }

        public bool AutoFlush {
            get { return backbone.AutoFlush; }
            set { backbone.AutoFlush = value; }
        }

        public Stream BaseStream {
            get { return backbone.BaseStream; }
        }

        public Encoding Encoding {
            get { return backbone.Encoding; }
        }
    }
}