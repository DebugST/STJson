using System;

namespace STLib.Json
{
    public struct STJsonTypeMapInfo : IComparable
    {
        public int HashCode;
        public Type Type;
        public STJsonConverter Converter;

        public static STJsonTypeMapInfo Empty;

        public int CompareTo(object obj)
        {
            return this.HashCode - ((STJsonTypeMapInfo)obj).HashCode;
        }

        public static STJsonTypeMapInfo Create(Type type, STJsonConverter converter)
        {
            return new STJsonTypeMapInfo()
            {
                HashCode = type.GetHashCode(),
                Type = type,
                Converter = converter
            };
        }

        public static STJsonTypeMapInfo Create(int n_code, STJsonConverter converter)
        {
            return new STJsonTypeMapInfo()
            {
                HashCode = n_code,
                Converter = converter
            };
        }

        public override string ToString()
        {
            return this.Type.FullName;
        }
    }
}
