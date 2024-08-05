[简体中文](./README_CN.md) [English](./README.md)

# Introduction

`STJson` is a `Json` parsing library based on the `MIT` open source protocol . The library is purely native implementation does not rely on any library , so it is very light and convenient , and powerful .

HOME: [https://DebugST.github.io/STJson](https://DebugST.github.io/STJson)

NuGet: [https://www.nuget.org/packages/STLib.Json](https://www.nuget.org/packages/STLib.Json)

Unlike other `Json` libraries, `STJson` not has objects similar to `JObject` and `JArray`. Just `STJson`. which can be either `Object` or `Array`. `STJson` has two indexers: `STJson[int]` and `STJson[string]`. The type of the current `Json` object can be determined by `STJson.ValueType`.

```cs
var json_1 = new STJson();
Console.WriteLine("[json_1] - " + json_1.IsNullValue + " - " + json_1.ValueType);

var json_2 = STJson.New();
json_2.SetItem("key", "value");
Console.WriteLine("[json_2] - " + json_2.IsNullValue + " - " + json_2.ValueType);

var json_3 = new STJson();
json_3.Append(1, 2, 3);
Console.WriteLine("[json_3] - " + json_3.IsNullValue + " - " + json_3.ValueType);

var json_4 = new STJson();
json_4.SetValue(DateTime.Now);
Console.WriteLine("[json_4] - " + json_4.IsNullValue + " - " + json_4.ValueType);

var json_5 = STJson.CreateArray();          // made by static function
Console.WriteLine("[json_5] - " + json_5.IsNullValue + " - " + json_5.ValueType);

var json_6 = STJson.CreateObject();         // made by static function
Console.WriteLine("[json_6] - " + json_6.IsNullValue + " - " + json_6.ValueType);

var json_7 = STJson.FromObject(12);         // made by static function
Console.WriteLine("[json_3] - " + json_7.IsNullValue + " - " + json_7.ValueType);
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
[json_1] - True - Undefined
[json_2] - False - Object
[json_3] - False - Array
[json_4] - False - Datetime
[json_5] - False - Array
[json_6] - False - Object
[json_7] - False - Long
```

# STJson

`STJson` is an intermediate data type that is a bridge between `string` and `object` and is very convenient to use, e.g:

```cs
var st_json = new STJson()
    .SetItem("number", 0)               // The function returns itself so it can be operated continuously.
    .SetItem("boolean", true)
    .SetItem("string", "this is string")
    .SetItem("datetime", DateTime.Now)
    .SetItem("array_1", STJson.CreateArray(123, true, "string"))
    .SetItem("array_2", STJson.FromObject(new object[] { 123, true, "string" }))
    .SetItem("object", new { key = "this is a object" })
    .SetItem("null", obj: null);
st_json.SetKey("key").SetValue("this is a test");
Console.WriteLine(st_json.ToString(4)); // 4 -> indentation space count
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
{
    "number": 0,
    "boolean": true,
    "string": "this is string",
    "datetime": "2023-04-22T21:12:30.6109410+08:00",
    "array_1": [
        123, true, "string"
    ],
    "array_2": [
        123, true, "string"
    ],
    "object": {
        "key": "this is a object"
    },
    "null": null,
    "key": "this is a test"
}
```

# Serialize

By the above example maybe you already know how to convert an object to `string`, by `STJson.FromObject(object).ToString(+n)`, but is it possible that it actually doesn't need to be so troublesome? For example: `STJson.Serialize(+n)` will do?

In fact `STJson.Serialize(+n)` would be more efficient because it converts the object directly to a string, rather than converting it to `STJson` and then to a string.

```cs
Console.WriteLine(STJson.Serialize(new { key = "this is test" }));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
{"key":"this is test"}
```

Of course you can have a more friendly output format:

```cs
Console.WriteLine(STJson.Serialize(new { key = "this is test" }, 4));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
{
    "key": "this is test"
}
```

In fact formatting the output is done by calling the static function `STJson.Format(+n)`. If you don't like the author's built-in formatting style, you can write a formatting function of your own. Or go modify the source file `STJson.Statics.cs:Format(string,int)`.

# Deserialize

The code doesn't actually convert `string` to `object` directly. It has to parse the string to make sure it's a properly formatted `Json` before it does that. But by the time this is done, you'll already have an `STJson` object. Finally, `STJson` is converted to `object` again.

So you will see the following files in the source code `STLib.Json.Converter`:

`ObjectToSTJson.cs` `ObjectToString.cs` `STJsonToObject.cs` `StringToSTJson.cs`

There is no `StringToObject.cs` file inside, and the source code of `STJson.Deserialize(+n)` is as follows:

```cs
public static T Deserialize<T>(string strJson, +n) {
    var json = StringToSTJson.Get(strJson, +n);
    return STJsonToObject.Get<T>(json, +n);
}
```

How to convert strings to objects, I believe the author does not need to explain the reader should know how to handle, but here it is worth explaining that `STJson` can be attached to objects to achieve local updates.

```cs
public class TestClass {
    public int X;
    public int Y;
}

TestClass tc = new TestClass() {
    X = 10,
    Y = 20
};
STJson json_test = new STJson().SetItem("Y", 100);
STJson.Deserialize(json_test, tc);
Console.WriteLine(STJson.Serialize(tc));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
 {"X":10,"Y":100}
```

# STJsonPath

Test data: `test.json`：

```cs
[{
    "name": "Tom", "age": 16, "gender": 0,
    "hobby": [
        "cooking", "sing"
    ]
},{
    "name": "Tony", "age": 16, "gender": 0,
    "hobby": [
        "game", "dance"
    ]
},{
    "name": "Andy", "age": 20, "gender": 1,
    "hobby": [
        "draw", "sing"
    ]
},{
    "name": "Kun", "age": 26, "gender": 1,
    "hobby": [
        "sing", "dance", "rap", "basketball"
    ]
}]
// Load it：
var json_src = STJson.Deserialize(System.IO.File.ReadAllText("./test.json"));
```
An `STJsonPath` can be constructed by:
```cs
// var jp = new STJsonPath("$[0]name");
// var jp = new STJsonPath("$[0].name");
var jp = new STJsonPath("[0]'name'"); // All of the above can be used, but $ is not required.
Console.WriteLine(jp.Select(json_src));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
["Tom"]
```
Of course `STJsonPath` is already integrated in the extension functions in `STJson` and can be used directly by the following:
```cs
// var jp = new STJsonPath("[0].name");
// Console.WriteLine(json_src.Select(jp));
Console.WriteLine(json_src.Select("[0].name")); // 内部动态构建 STJsonPath
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
["Tom"]
```

Normal mode (default mode):
```cs
Console.WriteLine(json_src.Select("..name", STJsonPathSelectMode.ItemOnly).ToString(4));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
[
    "Tom", "Tony", "Andy", "Kun"
]
```
Path mode：
```cs
Console.WriteLine(json_src.Select("..name", STJsonPathSelectMode.ItemWithPath).ToString(4));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
[
    {
        "path": [
            0, "name"
        ],
        "item": "Tom"
    }, {
        "path": [
            1, "name"
        ],
        "item": "Tony"
    }, {
        "path": [
            2, "name"
        ],
        "item": "Andy"
    }, {
        "path": [
            3, "name"
        ],
        "item": "Kun"
    }
]
```
Keep Structure：
```cs
Console.WriteLine(json_src.Select("..name", STJsonPathSelectMode.KeepStructure).ToString(4));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
[
    {
        "name": "Tom"
    }, {
        "name": "Tony"
    }, {
        "name": "Andy"
    }, {
        "name": "Kun"
    }
]
```

Of course the above is just an introduction to the basic usage, there are many rich features are not introduced to. If you have enough time you can read the tutorial directly at

CN: [https://DebugST.github.io/STJson/tutorial_cn.html](https://DebugST.github.io/STJson/tutorial_cn.html)

EN: [https://DebugST.github.io/STJson/tutorial_en.html](https://DebugST.github.io/STJson/tutorial_en.html)

# Contact Author

* TG: DebugST
* QQ: 2212233137
* Mail: 2212233137@qq.com