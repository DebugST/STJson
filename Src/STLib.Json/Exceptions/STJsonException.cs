﻿using System;

namespace STLib.Json
{
    public class STJsonException : Exception
    {
        public STJsonException(string strErr) : base(strErr) { }
    }
}
