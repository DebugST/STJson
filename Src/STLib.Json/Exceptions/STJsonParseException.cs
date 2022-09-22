using System;

namespace STLib.Json
{
    public class STJsonParseException : Exception
    {
        public int Index { get; private set; }

        public STJsonParseException(int nIndex, string strError) : base(strError) {
            this.Index = nIndex;
        }
    }
}

