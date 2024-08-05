using System;

namespace STLib.Json
{
    public class STJsonException : Exception
    {
        public STJsonException(string str_error) : base(str_error) { }
    }
}
