using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLib.Json
{
    public class STJsonWriterException : STJsonException
    {
        public STJsonWriterException(string str_error) : base(str_error) { }
    }
}
