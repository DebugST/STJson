using System;

namespace STLib.Json
{
    public class STJsonAttribute : Attribute
    {
        public STJsonSerilizaMode SerilizaMode { get; private set; }

        public STJsonAttribute(STJsonSerilizaMode serilizaMode) {
            this.SerilizaMode = serilizaMode;
        }
    }
}

