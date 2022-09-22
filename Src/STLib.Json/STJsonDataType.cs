using System;
using System.Collections.Generic;

namespace STLib.Json
{
    public enum STJsonSerilizaModel
    {
        All, OnlyMarked, ExcudeMarked
    }

    public enum STJsonValueType
    {
        None, String, Boolean, Number, Datetime, Array, Object
    }

    internal class STJsonBasicDataType
    {
        public delegate object Converter(object value);

        public struct TypeMapInfo
        {
            public string Name;
            public Type Type;
            public STJsonValueType ValueType;
            public Converter Converter;

            public TypeMapInfo(string name, Type type, STJsonValueType valueType, Converter converter) {
                this.Name = name;
                this.Type = type;
                this.ValueType = valueType;
                this.Converter = converter;
            }
        }

        private static Type m_type_object = typeof(object);
        private static Dictionary<string, TypeMapInfo> m_dic_type_map = new Dictionary<string, TypeMapInfo>();

        static STJsonBasicDataType() {
            Type t = null;
            t = typeof(byte);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToByte(v); }));
            t = typeof(sbyte);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToSByte(v); }));
            t = typeof(short);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToInt16(v); }));
            t = typeof(ushort);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToUInt16(v); }));
            t = typeof(int);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToInt32(v); }));
            t = typeof(uint);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToUInt32(v); }));
            t = typeof(long);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToInt64(v); }));
            t = typeof(ulong);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToUInt64(v); }));
            t = typeof(float);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToSingle(v); }));
            t = typeof(double);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToDouble(v); }));
            t = typeof(decimal);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Number, (v) => { return Convert.ToDecimal(v); }));
            t = typeof(bool);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Boolean, (v) => { return Convert.ToBoolean(v); }));
            t = typeof(char);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.String, (v) => { return v.ToString(); }));
            t = typeof(string);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.String, (v) => { return v.ToString(); }));
            t = typeof(DateTime);
            m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Datetime, (v) => { return Convert.ToDateTime(v); }));
            //t = typeof(object);
            //m_dic_type_map.Add(t.FullName, new TypeMapInfo(t.FullName, t, STJsonValueType.Object, (v) => { return v; }));
        }

        public static bool Contains(string strTypeName) {
            return m_dic_type_map.ContainsKey(strTypeName);
        }

        public static TypeMapInfo Get(string strTypeName) {
            return m_dic_type_map[strTypeName];
        }
    }
}

