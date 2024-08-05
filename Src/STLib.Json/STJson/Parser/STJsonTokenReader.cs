using System;
using System.IO;
using System.Text;

using System.Collections.Generic;
using System.Collections;

namespace STLib.Json
{
    internal class STJsonTokenReader : IEnumerable<STJsonToken>, IDisposable
    {
        private TextReader m_reader;
        private int m_n_position;
        private int m_n_row;
        private int m_n_col;

        private STJsonToken m_last_token;
        private bool m_is_auto_close;

        public bool Disposed { get; private set; }

        public STJsonTokenReader(TextReader reader, bool is_auto_close)
        {
            m_reader = reader;
            m_is_auto_close = is_auto_close;
        }

        public IEnumerator<STJsonToken> GetEnumerator()
        {
            m_n_position = 0;
            m_n_row = 1;
            m_n_col = 1;

            int n_char = -1;
            var token = new STJsonToken();

            while ((n_char = m_reader.Peek()) != -1) {
                var ch = (char)n_char;
                int n_index = m_n_position, n_row = m_n_row, n_col = m_n_col;
                if (('0' <= ch && ch <= '9') || ch == '-' || ch == '+' || ch == '.') {
                    yield return m_last_token = this.GetNumber();
                    continue;
                }
                switch (ch) {
                    case '{':   // object start
                        this.ReadInt();
                        yield return m_last_token = new STJsonToken(n_index, n_row, n_col, "{", STJsonTokenType.ObjectStart);
                        continue;
                    case '[':   // array start
                        this.ReadInt();
                        yield return m_last_token = new STJsonToken(n_index, n_row, n_col, "[", STJsonTokenType.ArrayStart);
                        continue;
                    case ',':   // item splitor
                        this.ReadInt();
                        yield return m_last_token = new STJsonToken(n_index, n_row, n_col, ",", STJsonTokenType.ItemSplitor);
                        continue;
                    case ':':   // key value splitor
                        this.ReadInt();
                        yield return m_last_token = new STJsonToken(n_index, n_row, n_col, ":", STJsonTokenType.KVSplitor);
                        continue;
                    case ']':   // array end
                        this.ReadInt();
                        yield return m_last_token = new STJsonToken(n_index, n_row, n_col, "]", STJsonTokenType.ArrayEnd);
                        continue;
                    case '}':   // object end
                        this.ReadInt();
                        yield return m_last_token = new STJsonToken(n_index, n_row, n_col, "}", STJsonTokenType.ObjectEnd);
                        continue;
                    case ' ':   // space
                    case '\r':
                    case '\n':
                    case '\t':
                        this.ReadInt();
                        continue;
                    case '/':
                        this.GetComment();
                        continue;
                    case '\'':  // string
                    case '\"':
                        token = this.GetString();
                        if (m_last_token.Type == STJsonTokenType.String) {
                            m_last_token.Value += token.Value;
                        } else {
                            m_last_token = token;
                        }
                        while ((n_char = m_reader.Peek()) != -1) {
                            switch (n_char) {
                                case ' ':   // space
                                case '\r':
                                case '\n':
                                case '\t':
                                    this.ReadInt();
                                    continue;
                            }
                            break;
                        }
                        switch (m_reader.Peek()) {
                            case '\'':
                            case '\"':
                                continue;
                        }
                        yield return m_last_token;
                        continue;
                    default:
                        token = this.GetSymbol();
                        if (token.Value.Length == 0) {
                            throw new STJsonParseException(m_n_position, "Invalid char '" + ch + "'.");
                        }
                        yield return token;
                        continue;
                }
            }
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.Disposed) return;
            this.Disposed = true;
            if (m_is_auto_close) {
                m_reader.Dispose();
            }
        }

        private char ReadChar()
        {
            return (char)this.ReadInt();
        }

        private int ReadInt()
        {
            var ch = m_reader.Read();
            m_n_position++;
            m_n_col++;
            if (ch == '\n') {
                m_n_row++;
                m_n_col = 1;
            }
            return ch;
        }

        private STJsonToken GetNumber()
        {
            StringBuilder sb = new StringBuilder(12);
            int n_char = -1;
            int n_index = m_n_position, n_row = m_n_row, n_col = m_n_col;
            bool b_dot = false, b_e = false, b_hex = false;
            while ((n_char = m_reader.Peek()) != -1) {
                var ch = (char)n_char;
                switch (ch) {
                    case 'x':           // 0x123 0X123
                    case 'X':
                        b_hex = true;
                        sb.Append(this.ReadChar());
                        continue;
                    case '-':           // -123 +123
                    case '+':
                        sb.Append(this.ReadChar());
                        continue;
                    case 'e':           // 123E+2 123e2 123e-2
                    case 'E':
                        b_e = true;
                        sb.Append(this.ReadChar());
                        continue;
                    case '.':           // 123.213 .123 123.
                        b_dot = true;
                        sb.Append(this.ReadChar());
                        continue;
                }
                if ('0' <= ch && ch <= '9') {
                    sb.Append(this.ReadChar()); continue;
                }
                if ('a' <= ch && ch <= 'f') {
                    sb.Append(this.ReadChar()); continue;
                }
                if ('A' <= ch && ch <= 'F') {
                    sb.Append(this.ReadChar()); continue;
                }
                break;
            }
            return new STJsonToken()
            {
                Type = (b_dot || b_e) && !b_hex ? STJsonTokenType.Double : STJsonTokenType.Long,
                Index = n_index,
                Row = n_row,
                Col = n_col,
                Value = sb.ToString()
            };
        }

        private STJsonToken GetSymbol()
        {
            StringBuilder sb = new StringBuilder(12);
            int n_char = -1;
            int n_index = m_n_position, n_row = m_n_row, n_col = m_n_col;

            while ((n_char = m_reader.Peek()) != -1) {
                var ch = (char)n_char;
                if ('a' <= ch && ch <= 'z') {
                    sb.Append(this.ReadChar()); continue;
                }
                if ('A' <= ch && ch <= 'Z') {
                    sb.Append(this.ReadChar()); continue;
                }
                if ('0' <= ch && ch <= '9') {
                    sb.Append(this.ReadChar()); continue;
                }
                if (ch == '_') {
                    sb.Append(this.ReadChar()); continue;
                }
                break;
            }

            return new STJsonToken()
            {
                Type = STJsonTokenType.Symbol,
                Index = n_index,
                Row = n_row,
                Col = n_col,
                Value = sb.ToString()
            };
        }

        private STJsonToken GetString()
        {
            StringBuilder sb = new StringBuilder(512);
            int n_char = -1;
            int n_index = m_n_position, n_row = m_n_row, n_col = m_n_col;

            var ch_begin = this.ReadChar();
            var token = new STJsonToken()
            {
                Type = STJsonTokenType.String,
                Index = n_index,
                Row = n_row,
                Col = n_col
            };

            while ((n_char = this.ReadInt()) != -1) {
                var ch = (char)n_char;
                if (ch == ch_begin) {
                    token.Value = sb.ToString();
                    return token;
                }
                sb.Append(ch);
                if (ch == '\\') sb.Append(this.ReadChar());
            }
            throw new STJsonParseException(n_index, m_n_row, m_n_col, "Can not get a string. missing '\'' or '\"'.");
        }

        private STJsonToken GetComment()
        {
            // note: str_text[n_index] == '/'
            int n_index = m_n_position, n_row = m_n_row, n_col = m_n_col;
            var n_char = this.ReadInt();
            if ((n_char = this.ReadInt()) == -1) {
                throw new STJsonParseException(n_index, m_n_row, m_n_col, "Invalid char '/'.");
            }
            switch (n_char) {
                case '/': return this.GetCommentLine();
                case '*': return this.GetCommentBlock();
                default:
                    throw new STJsonParseException(n_index + 1, n_row, n_col + 1, "Invalid char '" + (char)n_char + "'.");
            }
        }

        private STJsonToken GetCommentLine()
        {
            // note: str_text[n_index:2] == '//'
            int n_char = -1;

            while ((n_char = this.ReadInt()) != -1) {
                if (n_char == '\n') {
                    break;
                }
            }
            return new STJsonToken()
            {
                Type = STJsonTokenType.Comment
            };
        }

        private STJsonToken GetCommentBlock()
        {
            // note: str_text[n_index:2] == '/*'
            int n_char_current = '\0', n_char_last = '\0';
            while ((n_char_current = this.ReadInt()) != -1) {
                if (n_char_current == '/' && n_char_last == '*') {
                    break;
                }
                n_char_last = n_char_current;
            }
            return new STJsonToken()
            {
                Type = STJsonTokenType.Comment
            };
        }

        // =============================================

        IEnumerator<STJsonToken> IEnumerable<STJsonToken>.GetEnumerator()
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
