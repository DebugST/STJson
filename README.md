# STJson
A Json serialization operation calling library. Convert between objects and Json.

# Support DataType

|.Net|Json|e.g|
|:---|:---|:---|
|byte|Number|byte num = 0|
|sbyte|Number|sbyte num = 0|
|short|Number|short num = 0|
|ushort|Number|ushort num = 0|
|int|Number|int num = 0|
|uint|Number|uint num = 0|
|long|Number|long num = 0|
|ulong|Number|ulong num = 0|
|float|Number|float num = 0|
|double|Number|double num = 0|
|decimal|Number|decimal num = 0|
|bool|Boolean|bool b = true|
|char|String|char ch = 0|
|string|String|string str = 0|
|DateTime|String|DateTime dt = DateTime.Now|
|enum|Number|XXX.XXX|
|Array|Array|int[],int[,],int[][]|
|ICollection|Array|List,HashSet|
|IDectionary|Object|Dictionary|
|Other object|Object|Recursively reflect all fields.|

# Simple object

```cs
[STJson(STJsonSerilizaModel.OnlyMarked)]    // optional
public class Student
{
    public string Name;
    public int Age;
    public Gender Gender;
    [STJsonProperty]                        // optional
    public List<string> Hobby;
}

public enum Gender
{
    Male, Female
}
```

# Convert [string and object]

```cs
var stu = new Student() {
    Name = "Tom",
    Age = 100,
    Gender = Gender.Male,
    Hobby = new List<string>() { "Game", "Sing" }
};
//string str = STJson.Serialize(stu);
//str = STJson.Format(str);
//str = STJson.Format(str, 4);  // 4 -> the format space count.
string str = STJson.Serialize(stu, 4);
Console.WriteLine(str);
STJson json = STJson.Deserialize(str);
//Console.WriteLine("[STJson]  - " + json["Hobby"][0]);            // return STJson
//Console.WriteLine("[STJson]  - " + json["Hobby"][0].Value);      // return object
//Console.WriteLine("[STJson]  - " + json["Hobby"][0].GetValue()); // return string
Console.WriteLine("[STJson]  - " + json["Hobby"][0].GetValue<string>());
stu = STJson.Deserialize<Student>(str);
Console.WriteLine("[Student] - " + stu.Hobby[0]);
```

Output

```json
{
    "Name": "Tom",
    "Age": 100,
    "Gender": 0,
    "Hobby": [
        "Game", "Sing"
    ]
}
[STJson]  - Game
[Student] - Game
```

# Convert [STJson and object]

```cs
var json = new STJson();
json.SetKey("Name").SetValue("Andy");
json.SetItem("Age", 200);
json.SetItem("Gender", 1);
json.SetItem("Hobby", STJson.CreateArray(
    STJson.FromValue("Cooking"),
    STJson.FromValue("Sports")
    ));
Console.WriteLine(json.ToString(4));
var stu = STJson.Deserialize<Student>(json);
Console.WriteLine(stu.Hobby[0]);

json = new STJson();
json.SetItem("Name", "Jack");
// attach to a exists project.
STJson.Deserialize(json, stu); 
Console.WriteLine(stu.Name);
```

Output

```
{
    "Name": "Andy",
    "Age": 200,
    "Gender": 1,
    "Hobby": [
        "Cooking", "Sports"
    ]
}
Cooking
Jack
```

# Use Attribute

```cs
//str = STJson.Serialize(stu, ignoreAttribute: false);
str = STJson.Serialize(stu, false);
Console.WriteLine(str);
/*ignoreAttribute default value is true*/
```

Output

```json
{"Hobby":["Cooking","Sports"]}
```

# A complex object.
---
```cs
public enum TestObjectEnum { Enum1, Enum2, Enum3 }

[STJson(STJsonSerilizaModel.All)]
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

public class STJsonTest
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
```

Code

```cs
var obj = STJsonTest.CreateTestObject();

var str_a = STJson.Serialize(obj);
var aaa = STJson.Deserialize<TestObject>(str_a);
var str_b = STJson.Serialize(aaa);
var bool_a = str_a == str_b;
Console.WriteLine(bool_a);
Console.WriteLine(STJson.Format(str_a, 4));
```

Output

```json
true
{
	"STR": "string",
	"INT": 10,
	"DB": -0.123,
	"DEC": 0.123,
	"Enum": 0,
	"TIME": "2022-09-21T12:24:32.1520390+08:00",
	"OBJ": {
		"ARR_INT": [1, 2, 3],
		"LST_INT": [11, 12, 13],
		"LST_OBJ": [true, false, 123, "string", {
			"DY_STR": "str"
		}, -0.0005],
		"DIC_STR_INT": {
			"key_1": 10,
			"key_2": 10
		},
		"DIC_STR_OBJ": {
			"key_1": true,
			"key_2": false,
			"key_3": null,
			"key_4": "string",
			"key_5": 1234,
			"key_6": {
				"DY_INT": 100
			}
		},
		"ARR_INT_READONLY": [101, 202],
		"ARR_ARR_INT": [
			[],
			[0],
			[0, 0],
			[0, 0, 0],
			[0, 0, 0, 0]
		],
		"ARRARR_INT": [
			[0, 0, 0],
			[0, 0, 0]
		],
		"ARRARRARR_INT": [
			[
				[0, 0, 0, 0],
				[0, 0, 0, 0],
				[0, 0, 0, 0]
			],
			[
				[0, 0, 0, 0],
				[0, 0, 0, 0],
				[0, 0, 0, 0]
			]
		],
		"LST_INT_READONLY": [100, 200],
		"ARR_INT_READONLY_LENGTH": 2
	},
	"ARR_OBJ": [null, {
		"ARR_INT": [60, 70],
		"LST_INT": null,
		"LST_OBJ": null,
		"DIC_STR_INT": null,
		"DIC_STR_OBJ": null,
		"ARR_INT_READONLY": [101, 202],
		"ARR_ARR_INT": [
			[],
			[0],
			[0, 0],
			[0, 0, 0],
			[0, 0, 0, 0]
		],
		"ARRARR_INT": [
			[0, 0, 0],
			[0, 0, 0]
		],
		"ARRARRARR_INT": [
			[
				[0, 0, 0, 0],
				[0, 0, 0, 0],
				[0, 0, 0, 0]
			],
			[
				[0, 0, 0, 0],
				[0, 0, 0, 0],
				[0, 0, 0, 0]
			]
		],
		"LST_INT_READONLY": [100, 200],
		"ARR_INT_READONLY_LENGTH": 2
	}, null]
}
```