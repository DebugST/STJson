using System;

namespace STLib.Json
{
    public class STJsonPathException : Exception
    {
        public STJsonPathException(string strErr) : base(strErr) {
        }
    }
}

