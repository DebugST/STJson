namespace STLib.Json
{
    internal struct STJsonToken
    {
        public static readonly STJsonToken None = new STJsonToken()
        {
            Index = -1,
            Row = -1,
            Col = -1,
            Type = STJsonTokenType.None
        };

        public int Index;
        public int Row;
        public int Col;
        public string Value;
        public STJsonTokenType Type;

        public STJsonToken(int n_index, int n_row, int n_col, string str_value, STJsonTokenType type)
        {
            this.Index = n_index;
            this.Row = n_row;
            this.Col = n_col;
            this.Value = str_value;
            this.Type = type;
        }
    }
}
