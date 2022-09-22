using System;

namespace STLib.Json
{
    public class STJsonAttribute : Attribute
    {
        public STJsonSerilizaModel SerilizaModel { get; private set; }

        public STJsonAttribute(STJsonSerilizaModel serilizaModel) {
            this.SerilizaModel = serilizaModel;
        }
    }
}

