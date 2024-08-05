using System;

namespace STLib.Json
{
    public class STJsonAttribute : Attribute
    {
        public STJsonSerializeMode SerilizaMode { get; private set; }

        public STJsonAttribute(STJsonSerializeMode serilizaMode) {
            this.SerilizaMode = serilizaMode;
        }
    }
}

