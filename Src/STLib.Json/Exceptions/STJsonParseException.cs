﻿using System;

namespace STLib.Json
{
    public class STJsonParseException : Exception
    {
        public int Index { get; private set; }

        public STJsonParseException(int nIndex, string strError) : base(strError) {
            this.Index = nIndex;
        }

        public STJsonParseException(int nIndex, char ch)
            : base("Can not parse the string. Index: " + nIndex + ", char: [" + ch + "]") {
            this.Index = nIndex;
        }
    }
}

