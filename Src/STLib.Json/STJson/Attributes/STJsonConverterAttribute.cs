using System;

namespace STLib.Json
{
    public class STJsonConverterAttribute : Attribute
    {
        private static Type m_type_convert = typeof(STJsonConverter);
        public Type Type { get; private set; }

        public STJsonConverterAttribute(Type type) {
            if (!type.IsSubclassOf(m_type_convert)) {
                throw new ArgumentException("The type {" + type.FullName + "} is not sub class of {" + m_type_convert.FullName + "}", "type");
            }
            this.Type = type;
        }
    }
}
