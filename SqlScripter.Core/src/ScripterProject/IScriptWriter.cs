using System;
using System.IO;
using System.Runtime.Remoting;
using System.Text;

namespace SqlScripter.Core.ScripterProject {
    public interface IScriptWriter {
        void Write(char value);
        void Write(char[] buffer);
        void Write(char[] buffer, int index, int count);
        void Write(string value);
        void Write(bool value);
        void Write(int value);
        void Write(uint value);
        void Write(long value);
        void Write(ulong value);
        void Write(float value);
        void Write(double value);
        void Write(decimal value);
        void Write(object value);
        void Write(string format, object arg0);
        void Write(string format, object arg0, object arg1);
        void Write(string format, object arg0, object arg1, object arg2);
        void Write(string format, params object[] arg);
        void WriteLine();
        void WriteLine(char value);
        void WriteLine(char[] buffer);
        void WriteLine(char[] buffer, int index, int count);
        void WriteLine(bool value);
        void WriteLine(int value);
        void WriteLine(uint value);
        void WriteLine(long value);
        void WriteLine(ulong value);
        void WriteLine(float value);
        void WriteLine(double value);
        void WriteLine(decimal value);
        void WriteLine(string value);
        void WriteLine(object value);
        void WriteLine(string format, object arg0);
        void WriteLine(string format, object arg0, object arg1);
        void WriteLine(string format, object arg0, object arg1, object arg2);
        void WriteLine(string format, params object[] arg);
        void Close();
        void Flush();
        bool AutoFlush { get; set; }
        Stream BaseStream { get; }
        Encoding Encoding { get; }
        IFormatProvider FormatProvider { get; }
        string NewLine { get; set; }
        StreamWriter BaseWriter { get; set; }
        void Dispose();
        object GetLifetimeService();
        object InitializeLifetimeService();
        ObjRef CreateObjRef(Type requestedType);
        void WriteHead(string text);
        string FileFooter { set; }
        string FileHeader { set; get; }
        void SplitFileIfNeeded();
    }
}