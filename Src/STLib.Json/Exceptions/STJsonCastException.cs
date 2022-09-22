using System;

namespace STLib.Json
{
    public class STJsonCastException : InvalidCastException
    {
        public STJsonCastException(string strError) : base(strError) { }
        public STJsonCastException(string strError, Exception innerException) : base(strError, innerException) { }
        //public int Index { get; private set; }

        //public STJsonCastException(int nIndex, string strError) : base(strError) {
        //    this.Index = nIndex;
        //}
    }
}

