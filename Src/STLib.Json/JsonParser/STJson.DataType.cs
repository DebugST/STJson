using System;

namespace STLib.Json
{
    public enum STJsonSerilizaMode
    {
        All, Include, Exclude
    }

    public enum STJsonValueType
    {
        Undefined, Long, Double, String, Boolean, Array, Object, Datetime
    }

    public struct TypeMapInfo : IComparable
    {
        public int HashCode;
        public Type Type;
        public STJsonConverter Converter;

        public static TypeMapInfo Empty;

        public int CompareTo(object obj) {
            return this.HashCode - ((TypeMapInfo)obj).HashCode;
        }

        public static TypeMapInfo Create(Type t, STJsonConverter converter) {
            return new TypeMapInfo() {
                HashCode = t.GetHashCode(),
                Type = t,
                Converter = converter
            };
        }

        public static TypeMapInfo Create(int nCode, STJsonConverter converter) {
            return new TypeMapInfo() {
                HashCode = nCode,
                Converter = converter
            };
        }

        public override string ToString() {
            return this.Type.FullName;
        }
    }
}

