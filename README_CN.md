[简体中文](./README_CN.md) [English](./README.md)

# 简介

`STJson`是一款基于`MIT`开源协议的`Json`解析库。该库纯原生实现不依赖任何库，所以非常轻量便捷，且功能强大。

完整教程:

HOME: [https://DebugST.github.io/STJson](https://DebugST.github.io/STJson)

CN: [https://DebugST.github.io/STJson/tutorial_cn.html](https://DebugST.github.io/STJson/tutorial_cn.html)

EN: [https://DebugST.github.io/STJson/tutorial_en.html](https://DebugST.github.io/STJson/tutorial_en.html)

`STJson`不同与其他`Json`库拥有类似与`JObject`和`JArray`的对象。在`STJson`中仅一个`STJson`对象，它既可以是`Object`，也可以是`Array`。`STJson`拥有两个索引器：`STJson[int]`和`STJson[string]`。可通过`STJson.ValueType`确定当前`Json`对象的类型。

```cs
var json_1 = new STJson();
Console.WriteLine("[json_1] - " + json_1.IsNullObject + " - " + json_1.ValueType);

var json_2 = STJson.New();
json_2.SetItem("key", "value");
Console.WriteLine("[json_2] - " + json_2.IsNullObject + " - " + json_2.ValueType);

var json_3 = new STJson();
json_3.Append(1, 2, 3);
Console.WriteLine("[json_3] - " + json_3.IsNullObject + " - " + json_3.ValueType);

var json_4 = new STJson();
json_4.SetValue(DateTime.Now);
Console.WriteLine("[json_4] - " + json_4.IsNullObject + " - " + json_4.ValueType);

var json_5 = STJson.CreateArray();          // made by static function
Console.WriteLine("[json_5] - " + json_5.IsNullObject + " - " + json_5.ValueType);

var json_6 = STJson.CreateObject();         // made by static function
Console.WriteLine("[json_6] - " + json_6.IsNullObject + " - " + json_6.ValueType);

var json_7 = STJson.FromObject(12);         // made by static function
Console.WriteLine("[json_3] - " + json_7.IsNullObject + " - " + json_7.ValueType);
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

`STJson`是一个中间数据类型，它是`string`与`object`之间的桥梁，使用非常便捷，比如：

```cs
var st_json = new STJson()
    .SetItem("number", 0)               // 函数返回自身 所以可以连续操作
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

# 序列化

通过上面的例子或许你已经知道怎么将一个对象转换为`string`，通过`STJson.FromObject(object).ToString(+n)`即可，但是有没有可能，其实不用这么麻烦的？比如：`STJson.Serialize(+n)`就可以了？？？

事实上`STJson.Serialize(+n)`的效率会更好，因为它是直接将对象转换为字符串，而不是转换成`STJson`再转换成字符串。

```cs
Console.WriteLine(STJson.Serialize(new { key = "this is test" }));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
{"key":"this is test"}
```

当然你可以有个更友好的输出格式：

```cs
Console.WriteLine(STJson.Serialize(new { key = "this is test" }, 4));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
{
    "key": "this is test"
}
```

事实上格式化输出是通过调用静态函数`STJson.Format(+n)`完成的。如果你觉得不喜欢作者内置的格式化风格，完全可以自己写一个格式化的函数。或者去修改源码文件`STJson.Statics.cs:Format(string,int)`。

# 反序列化

事实上代码并不会直接将`string`转换为`object`。因为在那之前必须先对字符串进行解析，确保它是一个正确格式的`Json`。但是做完这个过程的时候已经得到一个`STJson`对象了。最后将`STJson`再转换为`object`。

所以你会在源代码`STLib.Json.Converter`中看到如下文件：

`ObjectToSTJson.cs` `ObjectToString.cs` `STJsonToObject.cs` `StringToSTJson.cs`

里面并没有`StringToObject.cs`文件，而`STJson.Deserialize(+n)`的源码如下：

```cs
public static T Deserialize<T>(string strJson, +n) {
    var json = StringToSTJson.Get(strJson, +n);
    return STJsonToObject.Get<T>(json, +n);
}
```

如何将字符串转换为对象，相信作者不用说明读者也应该知道如何处理，但是这里值得说明的是，`STJson`可以附加到对象中，实现局部更新。

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

测试数据`test.json`：

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
// 将其加载到程序中：
var json_src = STJson.Deserialize(System.IO.File.ReadAllText("./test.json"));
```
通过以下方式可以构建一个`STJsonPath`：
```cs
// var jp = new STJsonPath("$[0]name");
// var jp = new STJsonPath("$[0].name");
var jp = new STJsonPath("[0]'name'"); // 以上方式均可以使用 $不是必须的
Console.WriteLine(jp.Select(json_src));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
["Tom"]
```
当然在`STJson`中的扩展函数中已经集成`STJsonPath`，可以通过下面的方式直接使用：
```cs
// var jp = new STJsonPath("[0].name");
// Console.WriteLine(json_src.Select(jp));
Console.WriteLine(json_src.Select("[0].name")); // 内部动态构建 STJsonPath
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
["Tom"]
```

普通模式(默认方式)：
```cs
Console.WriteLine(json_src.Select("..name", STJsonPathSelectMode.ItemOnly).ToString(4));
/*******************************************************************************
 *                                [output]                                     *
 *******************************************************************************/
[
    "Tom", "Tony", "Andy", "Kun"
]
```
路径模式：
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
保持结构：
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

当然上面仅仅是介绍了一个基本用法，还有很多丰富的功能并没有介绍到。如果你时间充足可以直接阅读教程：

CN: [https://DebugST.github.io/STJson/tutorial_cn.html](https://DebugST.github.io/STJson/tutorial_cn.html)

EN: [https://DebugST.github.io/STJson/tutorial_en.html](https://DebugST.github.io/STJson/tutorial_en.html)

# 联系作者

* TG: DebugST
* QQ: 2212233137
* Mail: 2212233137@qq.com