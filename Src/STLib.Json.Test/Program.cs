using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STLib.Json;

namespace STLib.Json.Test
{
    public class UserInfo
    {
        public string Name { get; set; }
        public string Github { get; set; }
        public string[] Language { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            //var obj_test = STJsonTestObject.CreateTestObject();
            var type = typeof(UserInfo);
            var obj_test = new UserInfo()
            {
                Name = "DebugST",
                Github = "https://github.com/DebugST",
                Language = new string[] { "C#", "JS", "..." },
                Address = new Address()
                {
                    Country = "China",
                    Province = "GuangDong",
                    City = "ShenZhen"
                }
            };
            string str_json, str_temp;
            Console.WriteLine("========================================");
            Console.WriteLine(STJson.Serialize(obj_test));
            Console.WriteLine("========================================");

            Console.WriteLine("[CHECK_IS_SAME]");

            str_json = JsonConvert.SerializeObject(obj_test);
            str_temp = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<UserInfo>(str_json));
            Console.WriteLine("Newtonsoft : " + (str_temp == str_json));

            str_json = STJson.Serialize(obj_test);
            str_temp = STJson.Serialize(STJson.Deserialize<UserInfo>(str_json));
            Console.WriteLine("STJson     : " + (str_temp == str_json));

            var sw = new System.Diagnostics.Stopwatch();

            int n_counter = 50000;
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("========================================");
            Console.WriteLine("[Deserialize To Linq] - " + n_counter);
            sw.Reset();
            sw.Start();
            for (int i = 0; i < n_counter; i++) {
                JsonConvert.DeserializeObject<JObject>(str_json);
            }
            sw.Stop();
            Console.WriteLine("Newstonoft : " + sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < n_counter; i++) {
                STJson.Deserialize(str_json);
            }
            sw.Stop();
            Console.WriteLine("STJson     : " + sw.ElapsedMilliseconds);
            // ====================================================================================================
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("========================================");
            Console.WriteLine("[Deserialize To object] - " + n_counter);
            sw.Reset();
            sw.Start();
            for (int i = 0; i < n_counter; i++) {
                JsonConvert.DeserializeObject<UserInfo>(str_json);
            }
            sw.Stop();
            Console.WriteLine("Newstonoft : " + sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < n_counter; i++) {
                STJson.Deserialize<UserInfo>(str_json);
            }
            sw.Stop();
            Console.WriteLine("STJson     : " + sw.ElapsedMilliseconds);
            // ====================================================================================================
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("========================================");
            Console.WriteLine("[Serialize] - " + n_counter);
            sw.Reset();
            sw.Start();
            for (int i = 0; i < n_counter; i++) {
                JsonConvert.SerializeObject(obj_test);
            }
            sw.Stop();
            Console.WriteLine("Newstonoft : " + sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < n_counter; i++) {
                STJson.Serialize(obj_test);
            }
            sw.Stop();
            Console.WriteLine("STJson     : " + sw.ElapsedMilliseconds);
            Console.WriteLine("========================================");
            Console.WriteLine("==END==");
            Console.ReadKey();
        }
    }
}