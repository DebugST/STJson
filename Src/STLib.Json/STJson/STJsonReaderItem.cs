using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLib.Json
{
    public class STJsonReaderItem
    {
        public string Path { get; internal set; }
        public object PathArray { get; internal set; }
        public string Key { get; internal set; }
        public int Index { get; internal set; }
        public string Text { get; internal set; }
        public STJsonValueType ParentType { get; internal set; }
        public STJsonValueType ValueType { get; internal set; }

        private STJsonReader m_reader;
        private STJsonToken m_value_token;

        internal STJsonReaderItem(STJsonReader reader, STJsonToken token)
        {
            m_reader = reader;
            m_value_token = token;
        }

        public STJson GetSTJson()
        {
            switch (this.ValueType) {
                case STJsonValueType.String:
                    return STJson.FromObject(this.Text);
                case STJsonValueType.Double:
                    return STJson.FromObject(double.Parse(this.Text));
                case STJsonValueType.Long:
                    return STJson.FromObject(long.Parse(this.Text));
                case STJsonValueType.Boolean:
                    return STJson.FromObject(bool.Parse(this.Text));
                case STJsonValueType.Array:
                    return this.GetArray();
                case STJsonValueType.Object:
                    return this.GetObject();
            }
            return new STJson();
        }

        private STJson GetObject()
        {
            int n_counter = 1;
            List<STJsonToken> lst_token = new List<STJsonToken>() { m_value_token };
            foreach (var token in m_reader.TokenReader) {
                lst_token.Add(token);
                switch (token.Type) {
                    case STJsonTokenType.ObjectStart:
                        n_counter++;
                        continue;
                    case STJsonTokenType.ObjectEnd:
                        if (--n_counter == 0) {
                            break;
                        }
                        continue;
                    default: continue;
                }
                break;
            }
            m_reader.PopStack();
            return STJsonParser.Parse(lst_token);
        }

        private STJson GetArray()
        {
            int n_counter = 1;
            List<STJsonToken> lst_token = new List<STJsonToken>() { m_value_token };
            foreach (var token in m_reader.TokenReader) {
                lst_token.Add(token);
                switch (token.Type) {
                    case STJsonTokenType.ArrayStart:
                        n_counter++;
                        continue;
                    case STJsonTokenType.ArrayEnd:
                        if (--n_counter == 0) {
                            break;
                        }
                        continue;
                    default: continue;
                }
                break;
            }
            m_reader.PopStack();
            return STJsonParser.Parse(lst_token);
        }
    }
}
