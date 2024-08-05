using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace STLib.Json
{
    public delegate void STJsonWriterCallback();
    public delegate void STJsonWriterStartCallback(STJsonWriter writer);

    public class STJsonWriter : IDisposable
    {
        private enum LevelType { None, Object, Array }
        private class StackInfo
        {
            public int ItemCount { get; set; }
            public int Level { get; private set; }
            public LevelType LeveType { get; private set; }
            public string Space { get; private set; }

            public StackInfo(int n_level, LevelType type, string str_space)
            {
                this.Level = n_level;
                this.LeveType = type;
                this.Space = str_space;
            }
        }

        //====================================================

        private bool m_b_start;
        private Stack<StackInfo> m_stack = new Stack<StackInfo>();
        private StackInfo m_current_stack = null;

        private TextWriter m_writer;

        public bool Disposed { get; private set; }
        public bool IsAutoClose { get; private set; }
        public int SpaceCount { get; private set; }

        public int Level
        {
            get {
                return m_current_stack == null ? 0 : m_current_stack.Level;
            }
        }

        public STJsonValueType ValueType
        {
            get {
                if (m_current_stack == null) return STJsonValueType.Undefined;
                switch (m_current_stack.LeveType) {
                    case LevelType.Array:
                        return STJsonValueType.Array;
                    case LevelType.Object:
                        return STJsonValueType.Object;
                }
                return STJsonValueType.Undefined;
            }
        }

        public STJsonWriter(string str_file) : this(new StreamWriter(str_file, false, Encoding.UTF8), true, 0) { }

        public STJsonWriter(string str_file, int n_space_count) : this(new StreamWriter(str_file, false, Encoding.UTF8), true, n_space_count) { }

        public STJsonWriter(TextWriter writer) : this(writer, true, 0) { }

        public STJsonWriter(TextWriter writer, int n_space_count) : this(writer, true, n_space_count) { }

        public STJsonWriter(TextWriter writer, bool is_auto_close, int n_space_count)
        {
            m_writer = writer;
            this.IsAutoClose = is_auto_close;
            this.SpaceCount = n_space_count;
            if (this.SpaceCount < 0) {
                this.SpaceCount = 0;
            }
            this.PushStack(0, LevelType.None);
        }

        public void StartWithObject(STJsonWriterStartCallback callback)
        {
            if (m_b_start) throw new STJsonWriterException("It's already started.");
            m_b_start = true;
            this.CheckDisposedOrStarted();
            this.CreateObject(callback, null);
        }

        public void StartWithArray(STJsonWriterStartCallback callback)
        {
            if (m_b_start) throw new STJsonWriterException("It's already started.");
            m_b_start = true;
            this.CheckDisposedOrStarted();
            this.CreateArray(callback, null);
        }

        public STJsonWriter Append(object value)
        {
            this.CheckDisposedOrStarted();
            if (m_current_stack.LeveType != LevelType.Array) {         // check current level
                throw new STJsonWriterException("Current level is not a array. The current operation is only available at the array level.");
            }
            if (m_current_stack.ItemCount > 0) {                        // not the first item need add (,)
                m_writer.Write(',');
            }
            if (this.SpaceCount != 0) {
                m_writer.Write("\r\n" + m_current_stack.Space);
                ObjectToString.Get(m_writer, m_current_stack.Level, this.SpaceCount, value, STJsonSetting.Default);
            } else {
                m_writer.Write(STJson.Serialize(value));
            }
            m_current_stack.ItemCount++;
            return this;
        }

        public STJsonWriter SetItem(string str_key, object value)
        {
            this.CheckDisposedOrStarted();
            if (m_current_stack.LeveType != LevelType.Object) {
                throw new STJsonWriterException("Current level is not a object. The current operation is only available at the object level.");
            }
            if (m_current_stack.ItemCount++ > 0) {
                m_writer.Write(',');
            }
            var str_text = "\"" + STJson.Escape(str_key) + "\":";
            if (this.SpaceCount != 0) {
                m_writer.Write("\r\n" + m_current_stack.Space + str_text + " ");
                ObjectToString.Get(m_writer, m_current_stack.Level, this.SpaceCount, value, STJsonSetting.Default);
            } else {
                m_writer.Write(str_text + STJson.Serialize(value));
            }
            return this;
        }

        public STJsonWriter SetObject(string str_key, STJsonWriterCallback callback)
        {
            return this.SetObject(str_key, true, callback);
        }

        public STJsonWriter SetObject(string str_key, bool b_express, STJsonWriterCallback callback)
        {
            if (!b_express) return this;
            this.CheckDisposedOrStarted();
            this.SetKey(str_key).CreateObject(null, callback);
            return this;
        }

        public STJsonWriter SetArray(string str_key, STJsonWriterCallback callback)
        {
            return this.SetArray(str_key, true, callback);
        }

        public STJsonWriter SetArray(string str_key, bool b_express, STJsonWriterCallback callback)
        {
            if (!b_express) return this;
            this.CheckDisposedOrStarted();
            this.SetKey(str_key).CreateArray(null, callback);
            return this;
        }

        public STJsonWriter CreateObject(STJsonWriterCallback callback)
        {
            this.CheckDisposedOrStarted();
            if (m_current_stack.LeveType != LevelType.Array) {
                throw new STJsonWriterException("Current level is not a array. The current operation is only available at the array level.");
            }
            if (m_current_stack.ItemCount++ > 0) {
                m_writer.Write(',');
            }
            if (this.SpaceCount != 0) {
                m_writer.Write("\r\n" + m_current_stack.Space);
            }

            return this.CreateObject(null, callback);
        }

        public STJsonWriter CreateArray(STJsonWriterCallback callback)
        {
            this.CheckDisposedOrStarted();
            if (m_current_stack.LeveType != LevelType.Array) {
                throw new STJsonWriterException("Current level is not a array. The current operation is only available at the array level.");
            }
            if (m_current_stack.ItemCount++ > 0) {
                m_writer.Write(',');
            }
            if (this.SpaceCount != 0) {
                m_writer.Write("\r\n" + m_current_stack.Space);
            }

            return this.CreateArray(null, callback);
        }

        public void Dispose()
        {
            if (this.Disposed) return;
            this.Disposed = true;
            m_stack.Clear();
            //if (m_writer == null) return;
            m_writer.Flush();
            if (this.IsAutoClose) m_writer.Dispose();
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        //==================================

        private void CheckDisposedOrStarted()
        {
            if (this.Disposed) {
                throw new ObjectDisposedException(nameof(STJsonWriter));
            }
            if (!m_b_start) {
                throw new STJsonWriterException("Cannot complete the current operation. Please call StartWithObject/Array() first.");
            }
        }

        private STJsonWriter CreateObject(STJsonWriterStartCallback start_callback, STJsonWriterCallback callback)
        {

            this.PushStack(m_current_stack.Level + 1, LevelType.Object);

            m_writer.Write('{');

            if (start_callback != null) {
                start_callback(this);
            } else {
                callback();
            }
            int n_item_count = m_current_stack.ItemCount;

            this.PopStack();

            if (this.SpaceCount != 0 && n_item_count != 0) {
                m_writer.Write("\r\n" + m_current_stack.Space);
            }
            m_writer.Write('}');
            return this;
        }

        private STJsonWriter CreateArray(STJsonWriterStartCallback start_callback, STJsonWriterCallback callback)
        {
            this.PushStack(m_current_stack.Level + 1, LevelType.Array);

            m_writer.Write('[');

            if (start_callback != null) {
                start_callback(this);
            } else {
                callback();
            }
            int n_item_count = m_current_stack.ItemCount;

            this.PopStack();

            if (this.SpaceCount != 0 && n_item_count != 0) {
                m_writer.Write("\r\n" + m_current_stack.Space);
            }
            m_writer.Write(']');
            return this;
        }

        private void PushStack(int n_level, LevelType type)
        {
            var str_space = "".PadRight(n_level * this.SpaceCount);
            m_current_stack = new StackInfo(n_level, type, str_space);
            m_stack.Push(m_current_stack);
        }

        private void PopStack()
        {
            m_current_stack = null;
            if (m_stack.Count == 0) return;
            m_stack.Pop();
            if (m_stack.Count == 0) return;
            m_current_stack = m_stack.Peek();
        }

        private STJsonWriter SetKey(string str_key)
        {
            if (m_current_stack.LeveType != LevelType.Object) {
                throw new STJsonWriterException("Current level is not a object. The current operation is only available at the object level.");
            }
            if (m_current_stack.ItemCount++ > 0) {
                m_writer.Write(',');
            }
            var str_text = "\"" + STJson.Escape(str_key) + "\":";
            if (this.SpaceCount != 0) {
                m_writer.Write("\r\n" + m_current_stack.Space + str_text + " ");
            } else {
                m_writer.Write(str_text);
            }
            return this;
        }
    }
}
