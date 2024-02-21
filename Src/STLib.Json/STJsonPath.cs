using System;
using System.Collections.Generic;

using ME = STLib.Json.STJsonPath;

namespace STLib.Json
{
    public partial class STJsonPath
    {
        private static char[] m_char_trim = new char[] { '$', '@' };
        public string Name { get; set; }
        public string SourceText { get; private set; }

        private List<STJsonPathItem> m_lst_items;

        public STJsonPath(string strPath) : this(strPath, strPath) { }

        public STJsonPath(string strName, string strPath) {
            this.SourceText = strPath;
            if (string.IsNullOrEmpty(strPath)) {
                throw new ArgumentException("The strPath can not be empty.", "strPath");
            }
            if (string.IsNullOrEmpty(strName)) {
                throw new ArgumentException("The strName can not be empty.", "strName");
            }
            this.Name = strName;
            strPath = strPath.Trim(m_char_trim);
            var tokens = STJsonPathTokenizer.GetTokens(strPath);
            m_lst_items = STJsonPathParser.GetPathItems(tokens);
        }

        public STJson GetParsedTokens() {
            string strText = string.Empty;
            foreach (var v in m_lst_items) {
                strText += v.Text;
            }
            var json = STJson.FromObject(new { type = "entry", parsed = strText, items = ME.GetParsedTokens(m_lst_items) });
            return json;
        }

        public STJson Select(STJson json) {
            return this.Select(json, STJsonPathSelectMode.ItemOnly, null);
        }

        public STJson Select(STJson json, STJsonPathSelectMode model) {
            return this.Select(json, model, null);
        }

        public STJson Select(STJson json, STJsonPathCallBack callBack) {
            return this.Select(json, STJsonPathSelectMode.ItemOnly, callBack);
        }

        public STJson Select(STJson json, STJsonPathSelectMode model, STJsonPathCallBack callBack) {
            SelectSetting setting = new SelectSetting() {
                Root = json,
                Mode = model,
                Path = new Stack<object>(),
                //CallbackReturn = callBack
                Callback = callBack
            };
            return this.Select(json, setting);
        }

        //public STJson Select(STJson json, STJsonPathCallBackVoid callBack) {
        //    return this.Select(json, STJsonPathSelectMode.ItemOnly, callBack);
        //}

        //public STJson Select(STJson json, STJsonPathSelectMode model, STJsonPathCallBackVoid callBack) {
        //    SelectSetting setting = new SelectSetting() {
        //        Root = json,
        //        Mode = model,
        //        Path = new Stack<object>(),
        //        CallbackVoid = callBack
        //    };
        //    return this.Select(json, setting);
        //}

        private STJson Select(STJson json, SelectSetting setting) {
            STJson jsonResult = STJson.CreateArray();
            ME.GetSTJsons(m_lst_items, 0, json, setting, jsonResult);
            if (setting.Mode != STJsonPathSelectMode.KeepStructure) {
                return jsonResult;
            }
            return STJsonPath.RestorePathJson(jsonResult);
        }

        public STJson SelectFirst(STJson json) {
            return this.SelectFirst(json, STJsonPathSelectMode.ItemOnly);
        }

        public STJson SelectFirst(STJson json, STJsonPathSelectMode model) {
            STJson json_result = this.Select(json, model, null);
            if (json_result.Count == 0) {
                return null;
            }
            return json_result[0];
        }

        public STJson SelectLast(STJson json) {
            return this.SelectLast(json, STJsonPathSelectMode.ItemOnly);
        }

        public STJson SelectLast(STJson json, STJsonPathSelectMode model) {
            STJson json_result = this.Select(json, model, null);
            if (json_result.Count == 0) {
                return null;
            }
            return json_result[json_result.Count - 1];
        }


        public STJson CreatePathJson(string value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(int value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(long value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(float value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(double value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(bool value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(DateTime value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(object value) {
            return STJsonPath.CreatePathJson(STJson.FromObject(value), m_lst_items);
        }

        public STJson CreatePathJson(STJson value) {
            return STJsonPath.CreatePathJson(value, m_lst_items);
        }

        public STJson CreatePathJson(STJsonPathCallBack callBack) {
            return STJsonPath.CreatePathJson(callBack, m_lst_items);
        }
    }
}
