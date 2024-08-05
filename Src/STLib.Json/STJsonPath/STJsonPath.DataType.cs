using System;
using System.Collections.Generic;

namespace STLib.Json
{
    public enum STJsonPathSelectMode
    {
        ItemOnly, ItemWithPath, KeepStructure
    }

    public class STJsonPathCallbackArgs
    {
        public bool Selected { get; set; }
        public STJson Path { get; internal set; }
        public STJson Json { get; set; }
    }

    public struct SelectSetting
    {
        public STJson Root;
        public STJsonPathSelectMode Mode;
        public Stack<object> Path;
        public STJsonPathCallBack Callback;

        public static SelectSetting Create()
        {
            return new SelectSetting()
            {
                Path = new Stack<object>()
            };
        }
    }


    public delegate void STJsonPathCallBack(STJsonPathCallbackArgs args);
    public delegate STJson STJsonPathCustomFunctionHander(STJsonPathExpressionFunctionArg[] args);

    public enum STJsonPathExpressionFunctionArgType
    {
        Undefined, Long, Double, String, Boolean, Array, Object,
    }

    public struct STJsonPathExpressionFunctionArg
    {
        public STJsonPathExpressionFunctionArgType Type;
        public object Value;
    }


    /* ================================================== internal ================================================== */


    //==========================================================
    internal delegate STJsonPathExpressionToken STJsonPathBuildInFuncHander(STJsonPathExpressionToken[] args);
    //==========================================================

    internal enum STJsonPathExpressionTokenType
    {
        Undefined, Long, Double, String, Boolean, Array, Object, ArrayExp, Func, PathItem, Operator, LeftParentheses, RightParentheses, Error
    }

    internal struct STJsonPathExpressionToken
    {

        public static readonly STJsonPathExpressionToken Undefined = new STJsonPathExpressionToken() { Type = STJsonPathExpressionTokenType.Undefined };
        public static STJsonPathExpressionToken True = STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Boolean, true);
        public static STJsonPathExpressionToken False = STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Boolean, false);

        public bool IsNumber
        {
            get { return this.Type == STJsonPathExpressionTokenType.Long || this.Type == STJsonPathExpressionTokenType.Double; }
        }

        public string Text
        {
            get {
                if (this.Type == STJsonPathExpressionTokenType.Operator) {
                    return " " + this.Value + " ";
                }
                if (this.Type == STJsonPathExpressionTokenType.PathItem) {
                    string strItem = string.Empty;
                    foreach (var v in this.PathItems) {
                        strItem += v.Text;
                    }
                    return strItem.Trim();
                }
                if (this.Type == STJsonPathExpressionTokenType.Func) {
                    string strFunc = this.Value + "(";
                    foreach (var v in this.Args) {
                        strFunc += v.Text + ", ";
                    }
                    strFunc = strFunc.Trim().Trim(',') + ")";
                    if (this.PathItems == null || this.PathItems.Count < 1) {
                        return strFunc;
                    }
                    strFunc += ".";
                    for (int i = 1; i < this.PathItems.Count; i++) {
                        strFunc += this.PathItems[i].Text + ".";
                    }
                    return strFunc.Trim('.');
                }
                if (this.Type == STJsonPathExpressionTokenType.Array) {
                    List<string> lst = new List<string>();
                    foreach (var v in this.Value as STJsonPathExpressionToken[]) {
                        switch (v.Type) {
                            case STJsonPathExpressionTokenType.String:
                                lst.Add("'" + STJson.Escape(v.Text) + "'");
                                break;
                            default:
                                lst.Add(v.Text);
                                break;
                        }
                    }
                    return "[" + string.Join(",", lst.ToArray()) + "]";
                }
                if (this.Type == STJsonPathExpressionTokenType.ArrayExp) {
                    List<string> lst = new List<string>();
                    foreach (var v in this.Value as STJsonPathExpression[]) {
                        lst.Add(v.Text);
                    }
                    return "[" + string.Join(",", lst.ToArray()) + "]";
                }
                if (this.Type == STJsonPathExpressionTokenType.Undefined) {
                    return "undefined";
                }
                if (this.Type == STJsonPathExpressionTokenType.Boolean) {
                    return Convert.ToString(this.Value).ToLower();
                }
                return Convert.ToString(this.Value);
            }
        }

        public int Index;
        public STJsonPathExpressionTokenType Type;
        public object Value;
        public List<STJsonPathExpression> Args;
        public List<STJsonPathItem> PathItems;

        public static STJsonPathExpressionToken CreateError(string str_error) {
            return new STJsonPathExpressionToken()
            {
                Index = -1,
                Type = STJsonPathExpressionTokenType.Error,
                Value = str_error
            };
        }

        public static STJsonPathExpressionToken Create(int nIndex, STJsonPathExpressionTokenType type, object value)
        {
            return new STJsonPathExpressionToken()
            {
                Index = nIndex,
                Type = type,
                Value = value
            };
        }

        public static STJsonPathExpressionToken Create(int nIndex, List<STJsonPathItem> items)
        {
            return new STJsonPathExpressionToken()
            {
                Index = nIndex,
                Type = STJsonPathExpressionTokenType.PathItem,
                PathItems = items
            };
        }

        public static STJsonPathExpressionToken Create(int nIndex, string strFuncName, List<STJsonPathExpression> args, List<STJsonPathItem> items)
        {
            return new STJsonPathExpressionToken()
            {
                Index = nIndex,
                Type = STJsonPathExpressionTokenType.Func,
                Value = strFuncName,
                Args = args,
                PathItems = items
            };
        }

        public override string ToString()
        {
            return "[" + this.Type.ToString() + "] - (" + this.Value + ")";
        }
    }
    //=======================================
    internal enum STJsonPathTokenType
    {
        None, Dot, Property, Operator, Long, Double, String, Keyword, Range
    }

    internal struct STJsonPathToken
    {
        public int Index;
        public string Value;
        public STJsonPathTokenType Type;

        public bool IsNumber
        {
            get { return this.Type == STJsonPathTokenType.Long || this.Type == STJsonPathTokenType.Double; }
        }

        public STJsonPathToken(int nIndex, string strValue, STJsonPathTokenType type)
        {
            this.Index = nIndex;
            this.Value = strValue;
            this.Type = type;
        }

        public bool IsSymbol(string strSmb)
        {
            return this.Value == strSmb && this.Type != STJsonPathTokenType.String;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\" - [{1}] - [{2}]", this.Value, this.Index, this.Type);
        }
    }
}

