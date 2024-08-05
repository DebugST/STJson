using System;

namespace STLib.Json
{
    public class STJsonPathParseException : Exception
    {
        public int Index { get; private set; }

        public STJsonPathParseException(int nIndex, string strError) : base(strError) {
            this.Index = nIndex;
        }

        public STJsonPathParseException(int nIndex, char ch)
            : base("Can not parse the string. Index: " + nIndex + ", char: [" + ch + "]") {
            this.Index = nIndex;
        }
    }
}

