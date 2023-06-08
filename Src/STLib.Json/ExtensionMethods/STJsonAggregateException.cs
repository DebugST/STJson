using System;

namespace STLib.Json
{
    public class STJsonAggregateException : Exception
    {
        public STJsonAggregateException(string strErr) : base(strErr)
        {
        }
    }
}

