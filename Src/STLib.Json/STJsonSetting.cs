using System.Collections.Generic;

namespace STLib.Json
{
    public class STJsonSetting
    {
        public static STJsonSetting Default = new STJsonSetting();

        public enum KeyMode
        {
            All, Include, Exclude
        }

        public bool EnumUseNumber { get; set; }
        public bool IgnoreAttribute { get; set; }
        public bool IgnoreNullValue { get; set; }
        public KeyMode KyeMode { get; set; }
        public HashSet<string> KeyList { get; private set; }

        public STJsonSetting() {
            this.KeyList = new HashSet<string>();
        }
    }
}
