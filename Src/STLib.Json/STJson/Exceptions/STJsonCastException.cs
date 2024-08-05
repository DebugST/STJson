using System;

namespace STLib.Json
{
    public class STJsonCastException : InvalidCastException
    {
        public STJsonCastException(string str_error) : base(str_error) { }
        public STJsonCastException(string str_error, Exception inner_exception) : base(str_error, inner_exception) { }
    }
}

