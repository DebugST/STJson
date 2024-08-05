using System;

namespace STLib.Json
{
    public class STJsonPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public STJsonPropertyAttribute() {
        }

        public STJsonPropertyAttribute(string strName) {
            this.Name = strName;
        }
    }
}

