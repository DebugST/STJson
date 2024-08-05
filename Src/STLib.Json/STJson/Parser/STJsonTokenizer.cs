using System;
using System.Text;
using System.Collections.Generic;

namespace STLib.Json
{
    internal class STJsonTokenizer
    {
        public string Text { get => m_str_json; }

        private string m_str_json;

        private int m_n_position;
        private int m_n_row;
        private int m_n_col;

        public STJsonTokenizer(string str_json)
        {
            m_str_json = str_json;
        }

        public List<STJsonToken> GetTokens()
        {
            m_n_position = 0;
            m_n_row = 1;
            m_n_col = 1;

            var token = new STJsonToken();
            List<STJsonToken> lst = new List<STJsonToken>();

            while (m_n_position < m_str_json.Length) {
                var ch = m_str_json[m_n_position];
                if (('0' <= ch && ch <= '9') || ch == '-' || ch == '+' || ch == '.') {
                    token = this.GetNumber();
                    lst.Add(token);
                    continue;
                }
                switch (ch) {
                    case '{':   // object start
                        //stack_region.Push('{');
                        lst.Add(new STJsonToken(m_n_position++, m_n_row, m_n_col++, "{", STJsonTokenType.ObjectStart));
                        continue;
                    case '[':   // array start
                        //stack_region.Push('[');
                        lst.Add(new STJsonToken(m_n_position++, m_n_row, m_n_col++, "[", STJsonTokenType.ArrayStart));
                        continue;
                    case ',':   // item splitor
                        lst.Add(new STJsonToken(m_n_position++, m_n_row, m_n_col++, ",", STJsonTokenType.ItemSplitor));
                        continue;
                    case ':':   // key value splitor
                        lst.Add(new STJsonToken(m_n_position++, m_n_row, m_n_col++, ":", STJsonTokenType.KVSplitor));
                        continue;
                    case ']':   // array end
                        lst.Add(new STJsonToken(m_n_position++, m_n_row, m_n_col++, "]", STJsonTokenType.ArrayEnd));
                        continue;
                    case '}':   // object end
                        lst.Add(new STJsonToken(m_n_position++, m_n_row, m_n_col++, "}", STJsonTokenType.ObjectEnd));
                        continue;
                    case ' ':   // space
                    case '\r':
                    case '\n':
                    case '\t':
                        m_n_position++;
                        m_n_col++;
                        if (ch == '\n') {
                            m_n_row++;
                            m_n_col = 1;
                        }
                        continue;
                    case '/':
                        token = this.GetComment();
                        continue;
                    case '\'':  // string
                    case '\"':
                        token = this.GetString();
                        if (lst.Count > 0 && lst[lst.Count - 1].Type == STJsonTokenType.String) {
                            var temp = lst[lst.Count - 1];
                            temp.Value += token.Value;
                            lst[lst.Count - 1] = temp;
                        } else {
                            lst.Add(token);
                        }
                        continue;
                    default:
                        token = this.GetSymbol();
                        if (token.Value.Length == 0) {
                            throw new STJsonParseException(m_n_position, "Invalid char '" + ch + "'.");
                        }
                        lst.Add(token);
                        break;
                }
            }
            return lst;
        }

        private STJsonToken GetNumber()
        {
            int n_index = m_n_position, n_len = -1;
            bool b_dot = false, b_e = false, b_hex = false;
            for (int i = n_index; i < m_str_json.Length; i++) {
                var ch = m_str_json[i];
                switch (ch) {
                    case 'x':           // 0x123 0X123
                    case 'X':
                        b_hex = true;
                        continue;
                    case '-':           // -123 +123
                    case '+':
                        continue;
                    case 'e':           // 123E+2 123e2 123e-2
                    case 'E':
                        b_e = true;
                        continue;
                    case '.':           // 123.213 .123 123.
                        b_dot = true;
                        continue;
                }
                if ('0' <= ch && ch <= '9') continue;
                if ('a' <= ch && ch <= 'f') continue;
                if ('A' <= ch && ch <= 'F') continue;
                n_len = i - n_index;
                break;
            }
            if (n_len == -1) n_len = m_str_json.Length - n_index;
            m_n_position += n_len;
            m_n_col += n_len;
            return new STJsonToken()
            {
                Index = n_index,
                Type = (b_dot || b_e) && !b_hex ? STJsonTokenType.Double : STJsonTokenType.Long,
                Value = m_str_json.Substring(n_index, n_len)
            };
        }

        private STJsonToken GetSymbol()
        {
            int n_col = m_n_col;
            int n_index = m_n_position, n_len = -1;
            for (var i = n_index; i < m_str_json.Length; i++) {
                var ch = m_str_json[i];
                if ('a' <= ch && ch <= 'z') continue;
                if ('A' <= ch && ch <= 'Z') continue;
                if ('0' <= ch && ch <= '9') continue;
                if (ch == '_') continue;
                n_len = i - n_index;
                break;
            }
            if (n_len == -1) n_len = m_str_json.Length - n_index;
            m_n_position += n_len;
            m_n_col += n_len;
            return new STJsonToken()
            {
                Type = STJsonTokenType.Symbol,
                Index = n_index,
                Row = m_n_row,
                Col = n_col,
                Value = m_str_json.Substring(n_index, n_len)
            };
        }

        private STJsonToken GetString()
        {
            int n_index = m_n_position;
            var ch_begin = m_str_json[n_index];
            var token = new STJsonToken()
            {
                Index = n_index,
                Row = m_n_row,
                Col = m_n_col++,
                Type = STJsonTokenType.String
            };
            for (int i = n_index + 1; i < m_str_json.Length; i++) {
                var ch = m_str_json[i];
                m_n_col++;
                if (ch == ch_begin) {
                    token.Value = m_str_json.Substring(n_index + 1, i - n_index - 1);
                    m_n_position += i - n_index + 1;
                    return token;
                }
                switch (ch) {
                    case '\\':
                        i++;
                        continue;
                    case '\n':
                        m_n_row++;
                        m_n_col = 1;
                        continue;
                }
            }
            throw new STJsonParseException(n_index, m_n_row, m_n_col, "Can not get a string. missing '\'' or '\"'.");
        }

        private STJsonToken GetComment()
        {
            // note: str_text[n_index] == '/'
            var n_index = m_n_position + 1;
            if (n_index >= m_str_json.Length) {
                throw new STJsonParseException(m_n_position, m_n_row, m_n_col, "Invalid char '/'.");
            }
            var ch = m_str_json[n_index];
            switch (ch) {
                case '/': return this.GetCommentLine();
                case '*': return this.GetCommentBlock();
                default:
                    throw new STJsonParseException(n_index, m_n_row, m_n_col, "Invalid char '" + ch + "'.");
            }
        }

        private STJsonToken GetCommentLine()
        {
            // note: str_text[n_index:2] == '//'
            int n_index = m_n_position, n_len = -1;
            m_n_col += 2;
            for (int i = n_index + 2; i < m_str_json.Length; i++) {
                m_n_col++;
                if (m_str_json[i] == '\n') {
                    n_len = i - n_index + 1;
                    m_n_row++;
                    m_n_col = 1;
                    break;
                }
            }
            if (n_len == -1) n_len = m_str_json.Length - n_index;
            m_n_position += n_len;
            return new STJsonToken()
            {
                Type = STJsonTokenType.Comment
            };
        }

        private STJsonToken GetCommentBlock()
        {
            // note: str_text[n_index:2] == '/*'
            m_n_col += 2;
            int n_index = m_n_position, n_len = -1;
            char ch_current = '\0', ch_last = '\0';
            for (int i = n_index + 2; i < m_str_json.Length; i++) {
                ch_current = m_str_json[i];
                m_n_col++;
                if (ch_current == '\n') {
                    m_n_row++;
                    m_n_col = 1;
                } else if (ch_current == '/' && ch_last == '*') {
                    n_len = i - n_index + 1;
                    break;
                }
                ch_last = ch_current;
            }
            if (n_len == -1) n_len = m_str_json.Length - n_index;
            m_n_position += n_len;
            return new STJsonToken()
            {
                Type = STJsonTokenType.Comment
            };
        }
    }
}

