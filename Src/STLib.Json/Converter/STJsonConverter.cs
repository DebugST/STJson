using System;

namespace STLib.Json
{
    public abstract class STJsonConverter
    {
        public virtual STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
            bProcessed = false;
            return null;
        }
        public virtual string ObjectToString(Type t, object obj, ref bool bProcessed) {
            bProcessed = false;
            return null;
        }
        public virtual object JsonToObject(Type t, STJson json, ref bool bProcessed) {
            bProcessed = false;
            return null;
        }
    }
}
