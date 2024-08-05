using System;

namespace STLib.Json
{
    public class STJsonParseException : STJsonException
    {
        public int Index { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }

        internal STJsonParseException(STJsonToken token)
            : base("Invalid token: '" + token.Value + "'. " + string.Format(" [index:{0}, row:{1}, col:{2}]", token.Index, token.Row, token.Col))
        {
            this.Index = token.Index;
            this.Row = token.Row;
            this.Col = token.Col;
        }

        internal STJsonParseException(int n_index, int n_row, int n_col, string str_error)
            : base(str_error + " " + string.Format(" [index:{0}, row:{1}, col:{2}]", n_index, n_row, n_col))
        {
        }

        internal STJsonParseException(int n_index, string str_error) : base(str_error)
        {
            this.Index = n_index;
        }
    }
}

