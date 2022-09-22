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
|Array|Array|int[],int[,],int[][]|
|ICollection|Array|List<int>,List<object>|
|IDectionary|Object|Dictionary<int,string>|
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

```
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
// attach to a exists project, but stu is struct.
STJson.Deserialize(json, stu); 
Console.WriteLine(stu.Name);
```

Output

```
Cooking
Andy
```

# Use Attribute

```cs
//str = STJson.Serialize(stu, ignoreAttribute: false);
str = STJson.Serialize(stu, false);
Console.WriteLine(str);
/*ignoreAttribute default value is true*/
```

Output

```
{"Hobby":["Cooking","Sports"]}
```