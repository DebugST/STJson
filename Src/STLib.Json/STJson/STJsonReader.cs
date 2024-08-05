using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Collections;

namespace STLib.Json
{
    public class STJsonReader : IEnumerable<STJsonReaderItem>, IDisposable
    {
        private class StackInfo
        {
            public char Symbol { get; set; }
            public int Level { get; set; }
            public string Key { get; set; }
            public int Index { get; set; }

            public StackInfo(char ch_symbol, int n_level)
            {
                this.Symbol = ch_symbol;
                this.Level = n_level;
            }
        }

        public bool Disposed { get; private set; }
        public bool IsAutoClose { get; private set; }

        internal STJsonTokenReader TokenReader { get { return m_token_reader; } }

        private bool m_b_started;
        private STJsonTokenReader m_token_reader;
        private Stack<object> m_stack_path = new Stack<object>();

        private StackInfo m_current_stack = null;
        private List<StackInfo> m_lst_stack_info = new List<StackInfo>(16);

        public STJsonReader(string str_file) : this(new StreamReader(str_file, Encoding.UTF8), true) { }

        public STJsonReader(TextReader reader) : this(reader, true) { }

        public STJsonReader(TextReader reader, bool is_auto_close)
        {
            this.IsAutoClose = is_auto_close;
            m_token_reader = new STJsonTokenReader(reader, is_auto_close);
        }

        public IEnumerator<STJsonReaderItem> GetEnumerator()
        {
            STJsonReaderItem item = null;
            while ((item = this.GetNextItem()) != null) {
                yield return item;
            }
            this.Dispose();
        }

        public STJsonReaderItem GetNextItem()
        {
            this.CheckStarted();
            STJsonReaderItem item = null;
            if (m_current_stack != null) {
                switch (m_current_stack.Symbol) {
                    case '{':   // '{'
                        item = this.GetNextObjectKV();
                        break;
                    default:    // '['
                        item = this.GetNextArrayItem();
                        break;
                }
            }
            if (item == null) {
                this.Dispose();
            }
            return item;
        }

        public void Dispose()
        {
            if (this.Disposed) {
                return;
            }
            this.Disposed = true;
            m_current_stack = null;
            m_stack_path.Clear();
            m_lst_stack_info.Clear();
            m_token_reader.Dispose();
        }

        // =====================================

        internal void PushStack(char ch_symbol, int n_level)
        {
            m_current_stack = new StackInfo(ch_symbol, n_level);
            m_lst_stack_info.Add(m_current_stack);
        }

        internal void PopStack()
        {
            m_current_stack = null;
            if (m_lst_stack_info.Count == 0) return;
            m_lst_stack_info.RemoveAt(m_lst_stack_info.Count - 1);
            if (m_lst_stack_info.Count == 0) return;
            m_current_stack = m_lst_stack_info[m_lst_stack_info.Count - 1];
        }

        // =====================================

        private void CheckStarted()
        {
            if (!m_b_started) {
                m_b_started = true;
                var token = this.GetNextFilteredToken();
                if (token.Type == STJsonTokenType.None) {
                    return;
                }
                switch (token.Type) {
                    case STJsonTokenType.ObjectStart:
                    case STJsonTokenType.ArrayStart:
                        break;
                    default:
                        throw new STJsonParseException(token);
                }
                this.PushStack(token.Value[0], 0);
            }
        }

        private STJsonToken GetNextFilteredToken()
        {
            foreach (var v in m_token_reader) {
                switch (v.Type) {
                    case STJsonTokenType.KVSplitor:     // :
                    case STJsonTokenType.ItemSplitor:   // ,
                        continue;
                    default:
                        return v;
                }
            }
            return STJsonToken.None;
        }

        private STJsonReaderItem GetNextObjectKV()
        {
            var token_key = this.GetNextFilteredToken();
            switch (token_key.Type) {
                case STJsonTokenType.None:
                    return null;
                case STJsonTokenType.ObjectEnd:
                    this.PopStack();
                    return this.GetNextItem();
                case STJsonTokenType.Symbol:
                case STJsonTokenType.String:
                    break;
                default:
                    throw new STJsonParseException(token_key);
            }
            m_current_stack.Key = token_key.Value;
            var token_val = this.GetNextFilteredToken();
            if (token_val.Type == STJsonTokenType.None) {
                return null;
                //throw new Exception("error");
            }
            var item = new STJsonReaderItem(this, token_val)
            {
                ParentType = STJsonValueType.Object,
                Key = token_key.Value,
                Text = token_val.Value
            };
            return this.CheckValueToken(item, token_val);
        }

        private STJsonReaderItem GetNextArrayItem()
        {
            var token = this.GetNextFilteredToken();
            switch (token.Type) {
                case STJsonTokenType.None:
                    return null;
                case STJsonTokenType.ArrayEnd:
                    this.PopStack();
                    return this.GetNextItem();
            }
            var item = new STJsonReaderItem(this, token)
            {
                ParentType = STJsonValueType.Array,
                Index = m_current_stack.Index++,
                Text = token.Value
            };
            return this.CheckValueToken(item, token);
        }

        private STJsonReaderItem CheckValueToken(STJsonReaderItem item, STJsonToken token)
        {
            this.SetItemPath(item);
            switch (token.Type) {
                case STJsonTokenType.ObjectStart:
                    item.ValueType = STJsonValueType.Object;
                    item.Text = "{...}";
                    this.PushStack('{', m_current_stack.Level + 1);
                    return item;
                case STJsonTokenType.ArrayStart:
                    item.ValueType = STJsonValueType.Array;
                    item.Text = "[...]";
                    this.PushStack('[', m_current_stack.Level + 1);
                    return item;
                case STJsonTokenType.Double:
                    item.ValueType = STJsonValueType.Double;
                    return item;
                case STJsonTokenType.Long:
                    item.ValueType = STJsonValueType.Long;
                    return item;
                case STJsonTokenType.String:
                    item.ValueType = STJsonValueType.String;
                    return item;
                case STJsonTokenType.Symbol:
                    switch (token.Value) {
                        case "true":
                        case "false":
                            item.ValueType = STJsonValueType.Boolean;
                            return item;
                        case "null":
                        case "undefined":
                            item.ValueType = STJsonValueType.Object;
                            return item;
                    }
                    break;
            }
            throw new STJsonParseException(token);
        }

        private void SetItemPath(STJsonReaderItem item)
        {
            int n_index = 0;
            var str_path = string.Empty;
            object[] obj_path = new object[m_lst_stack_info.Count];
            foreach (var v in m_lst_stack_info) {
                switch (v.Symbol) {
                    case '{':
                        if (n_index != 0) str_path += '.';
                        obj_path[n_index++] = v.Key;
                        if (v.Key.IndexOf('.') != -1) {
                            str_path += "'" + v.Key.Replace("'", "\\'") + "'";
                        } else {
                            str_path += v.Key;
                        }
                        break;
                    case '[':
                        obj_path[n_index++] = v.Index - 1; // m_current_index++;
                        str_path += "[" + (v.Index - 1) + ']';
                        break;
                }
            }
            item.Path = str_path;
            item.PathArray = obj_path;
        }

        // ===================================================

        IEnumerator<STJsonReaderItem> IEnumerable<STJsonReaderItem>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
        }
    }
}
