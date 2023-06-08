using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLib.Json.Test
{
    public enum TestObjectEnum { Enum1, Enum2, Enum3 }

    [STJson(STJsonSerilizaMode.All)]
    public class TestObject
    {
        private string m_private_string = "private_string_obj";
        public static string Name { get { return "TestObject"; } }
        public string STR { get; set; }
        public int INT { get; set; }
        public double DB { get; set; }
        public decimal DEC { get; set; }
        public TestObjectEnum Enum { get; set; }
        public DateTime TIME { get; set; }
        public TestObject_1 OBJ { get; set; }
        [STJsonProperty]
        public TestObject_1[] ARR_OBJ { get; set; }

        public void Function() { Console.WriteLine(m_private_string); }
    }

    public class TestObject_1
    {
        private string m_private_string = "private_string_obj_1";
        public static string Name { get { return "TestObject_1"; } }
        public int[] ARR_INT { get; set; }
        public List<int> LST_INT { get; set; }
        public List<object> LST_OBJ { get; set; }
        public Dictionary<string, int> DIC_STR_INT { get; set; }
        public Dictionary<string, object> DIC_STR_OBJ { get; set; }
        public int[] ARR_INT_READONLY { get; private set; }
        public int[][] ARR_ARR_INT { get; set; }
        public int[,] ARRARR_INT { get; set; }
        public int[,,] ARRARRARR_INT { get; set; }
        public List<int> LST_INT_READONLY { get; private set; }
        public int ARR_INT_READONLY_LENGTH { get { return ARR_INT_READONLY.Length; } }

        public TestObject_1() {
            ARRARR_INT = new int[2, 3];
            ARRARRARR_INT = new int[2, 3, 4];
            ARR_ARR_INT = new int[5][];
            for (int i = 0; i < ARR_ARR_INT.Length; i++) {
                ARR_ARR_INT[i] = new int[i];
            }
            ARR_INT_READONLY = new int[] { 101, 202 };
            LST_INT_READONLY = new List<int>() { 100, 200 };
        }

        public void Function() { Console.WriteLine(m_private_string); }
    }

    public class STJsonTestObject
    {
        public static TestObject CreateTestObject() {
            return new TestObject() {
                STR = "string",
                INT = 10,
                DB = -0.123,
                DEC = 0.123M,
                TIME = DateTime.Now,
                OBJ = new TestObject_1() {
                    ARR_INT = new int[] { 1, 2, 3 },
                    LST_INT = new List<int>() { 11, 12, 13 },
                    LST_OBJ = new List<object>() { true, false, 123, "string", new { DY_STR = "str" }, -0.0005 },
                    DIC_STR_INT = new Dictionary<string, int>() {
                        {"key_1", 10},
                        {"key_2", 10}
                    },
                    DIC_STR_OBJ = new Dictionary<string, object>(){
                        {"key_1", true},
                        {"key_2", false},
                        {"key_3", null},
                        {"key_4", "string"},
                        {"key_5", 1234},
                        {"key_6", new {DY_INT = 100}},
                    }
                },
                ARR_OBJ = new TestObject_1[]{
                    null, new TestObject_1(){ARR_INT = new int[]{60, 70}}, null
                }
            };
        }
    }
}
