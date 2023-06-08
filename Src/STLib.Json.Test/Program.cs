using System.Data;

namespace STLib.Json.Test
{
    internal class Program
    {
        [STJson(STJsonSerilizaMode.All)]
        public class Student
        {
            [STJsonProperty("test_name")]
            public string Name;
            [STJsonProperty]
            public int Age;
            public Gender Gender;
            [STJsonProperty]                        // optional
            public List<string> Hobby;
        }

        public enum Gender
        {
            Male, Female
        }

        static void Main(string[] args) {
            // Console.WriteLine(STJsonPath.GetBuildInFunctionList().ToString(4));

            DataTable dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("age", typeof(int));
            dt.Columns.Add("is_boy", typeof(bool));
            for (int i = 0; i < 10; i++) {
                DataRow dr = dt.NewRow();
                dr["name"] = "test";
                dr["age"] = 10;
                dr["is_boy"] = i % 2 == 0;
                dt.Rows.Add(dr);
            }

            var str = STJson.Serialize(dt, 4);
            Console.WriteLine(str);

            var stu = new Student() {
                Name = "Tom",
                Age = 20,
                Hobby = new List<string>() { "sing", "dance" }
            };
            object obj = STJsonTestObject.CreateTestObject();
            Console.WriteLine(STJson.Serialize(obj, 4));

            str = STJson.Serialize(stu, 4);
            Console.WriteLine(str);
            stu = STJson.Deserialize<Student>(str);

            Console.WriteLine(STJson.Serialize(new System.Drawing.Rectangle(10, 10, 100, 100)));

            Console.WriteLine(STJson.FromObject(stu).Select("..").ToString(4));

            //str = STJson.Serialize(obj, 4);
            //var sw = new System.Diagnostics.Stopwatch();
            //int nCount = 10000;

            //while (true) {
            //    Console.ReadKey();
            //    Console.WriteLine("===");

            //    sw.Restart();
            //    for (int i = 0; i < nCount; i++) {

            //        var aaa = STJson.Deserialize<TestObject>(str);
            //    }
            //    sw.Stop();
            //    Console.WriteLine(sw.ElapsedMilliseconds);
            //}
        }
    }
}