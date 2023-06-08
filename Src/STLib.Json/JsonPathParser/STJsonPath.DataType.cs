using System;
using System.Collections.Generic;

namespace STLib.Json
{
    public enum STJsonPathSelectMode
    {
        ItemOnly, ItemWithPath, KeepStructure
    }

    public struct STJsonPathCallBackResult
    {
        public bool Selected;
        public STJson Json;

        public STJsonPathCallBackResult(bool selected, STJson json) {
            this.Selected = selected;
            this.Json = json;
        }
    }

    public struct SelectSetting
    {
        public STJson Root;
        public STJsonPathSelectMode Mode;
        public Stack<object> Path;
        public STJsonPathCallBackVoid CallbackVoid;
        public STJsonPathCallBack CallbackReturn;

        public static SelectSetting Create() {
            return new SelectSetting() {
                Path = new Stack<object>()
            };
        }
    }

    public delegate void STJsonPathCallBackVoid(STJson path, STJson json);
    public delegate STJsonPathCallBackResult STJsonPathCallBack(STJson path, STJson json);
    public delegate STJson STJsonPathCustomFuncHander(STJsonPathExpFuncArg[] args);

    public enum STJsonPathExpFuncArgType
    {
        Undefined, Long, Double, String, Boolean, Array, Object,
    }

    public struct STJsonPathExpFuncArg
    {
        public STJsonPathExpFuncArgType Type;
        public object Value;
    }
    //==========================================================
    internal delegate ExpressionToken STJsonPathBuildInFuncHander(ExpressionToken[] args);
    //==========================================================
    internal enum ExpressTokenType
    {
        Undefined, Long, Double, String, Boolean, Array, Object, ArrayExp, Func, PathItem, Operator, LeftParentheses, RightParentheses
    }

    internal struct ExpressionToken
    {

        public static ExpressionToken Undefined;
        public static ExpressionToken True = ExpressionToken.Create(-1, ExpressTokenType.Boolean, true);
        public static ExpressionToken False = ExpressionToken.Create(-1, ExpressTokenType.Boolean, false);

        public bool IsNumber {
            get { return this.Type == ExpressTokenType.Long || this.Type == ExpressTokenType.Double; }
        }

        public string Text {
            get {
                if (this.Type == ExpressTokenType.Operator) {
                    return " " + this.Value + " ";
                }
                if (this.Type == ExpressTokenType.PathItem) {
                    string strItem = string.Empty;
                    foreach (var v in this.PathItems) {
                        strItem += v.Text;
                    }
                    return strItem.Trim();
                }
                if (this.Type == ExpressTokenType.Func) {
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
                if (this.Type == ExpressTokenType.Array) {
                    List<string> lst = new List<string>();
                    foreach (var v in this.Value as ExpressionToken[]) {
                        switch (v.Type) {
                            case ExpressTokenType.String:
                                lst.Add("'" + STJson.Escape(v.Text) + "'");
                                break;
                            default:
                                lst.Add(v.Text);
                                break;
                        }
                    }
                    return "[" + string.Join(",", lst.ToArray()) + "]";
                }
                if (this.Type == ExpressTokenType.ArrayExp) {
                    List<string> lst = new List<string>();
                    foreach (var v in this.Value as STJsonPathExpression[]) {
                        lst.Add(v.Text);
                    }
                    return "[" + string.Join(",", lst.ToArray()) + "]";
                }
                if (this.Type == ExpressTokenType.Undefined) {
                    return "undefined";
                }
                if (this.Type == ExpressTokenType.Boolean) {
                    return Convert.ToString(this.Value).ToLower();
                }
                return Convert.ToString(this.Value);
            }
        }

        public int Index;
        public ExpressTokenType Type;
        public object Value;
        public List<STJsonPathExpression> Args;
        public List<STJsonPathItem> PathItems;

        public static ExpressionToken Create(int nIndex, ExpressTokenType type, object value) {
            return new ExpressionToken() {
                Index = nIndex,
                Type = type,
                Value = value
            };
        }

        public static ExpressionToken Create(int nIndex, List<STJsonPathItem> items) {
            return new ExpressionToken() {
                Index = nIndex,
                Type = ExpressTokenType.PathItem,
                PathItems = items
            };
        }

        public static ExpressionToken Create(int nIndex, string strFuncName, List<STJsonPathExpression> args, List<STJsonPathItem> items) {
            return new ExpressionToken() {
                Index = nIndex,
                Type = ExpressTokenType.Func,
                Value = strFuncName,
                Args = args,
                PathItems = items
            };
        }

        public override string ToString() {
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

        public bool IsNumber {
            get { return this.Type == STJsonPathTokenType.Long || this.Type == STJsonPathTokenType.Double; }
        }

        public STJsonPathToken(int nIndex, string strValue, STJsonPathTokenType type) {
            this.Index = nIndex;
            this.Value = strValue;
            this.Type = type;
        }

        public bool IsSymbol(string strSmb) {
            return this.Value == strSmb && this.Type != STJsonPathTokenType.String;
        }

        public override string ToString() {
            return string.Format("\"{0}\" - [{1}] - [{2}]", this.Value, this.Index, this.Type);
        }
    }
}

