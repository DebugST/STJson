using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;


namespace STLib.Json
{
    internal class STJsonBuildInConverter
    {
        private static Dictionary<int, STJsonConverter> m_dic_type_map;

        static STJsonBuildInConverter() {
            m_dic_type_map = new Dictionary<int, STJsonConverter>();
            m_dic_type_map.Add(typeof(byte).GetHashCode(), new ByteConverter());
            m_dic_type_map.Add(typeof(sbyte).GetHashCode(), new SByteConverter());
            m_dic_type_map.Add(typeof(short).GetHashCode(), new ShortConverter());
            m_dic_type_map.Add(typeof(ushort).GetHashCode(), new UShortConverter());
            m_dic_type_map.Add(typeof(int).GetHashCode(), new IntConverter());
            m_dic_type_map.Add(typeof(uint).GetHashCode(), new UIntConverter());
            m_dic_type_map.Add(typeof(long).GetHashCode(), new LongConverter());
            m_dic_type_map.Add(typeof(ulong).GetHashCode(), new ULongConverter());
            m_dic_type_map.Add(typeof(float).GetHashCode(), new FloatConverter());
            m_dic_type_map.Add(typeof(double).GetHashCode(), new DoubleConverter());
            m_dic_type_map.Add(typeof(decimal).GetHashCode(), new DecimalConverter());
            m_dic_type_map.Add(typeof(bool).GetHashCode(), new BoolConverter());
            m_dic_type_map.Add(typeof(char).GetHashCode(), new CharConverter());
            m_dic_type_map.Add(typeof(string).GetHashCode(), new StringConverter());
            m_dic_type_map.Add(typeof(Point).GetHashCode(), new PointConverter());
            m_dic_type_map.Add(typeof(PointF).GetHashCode(), new PointFConverter());
            m_dic_type_map.Add(typeof(Size).GetHashCode(), new SizeConverter());
            m_dic_type_map.Add(typeof(SizeF).GetHashCode(), new SizeFConverter());
            m_dic_type_map.Add(typeof(Rectangle).GetHashCode(), new RectangleConverter());
            m_dic_type_map.Add(typeof(RectangleF).GetHashCode(), new RectangleFConverter());
            m_dic_type_map.Add(typeof(Color).GetHashCode(), new ColorConverter());
            m_dic_type_map.Add(typeof(DateTime).GetHashCode(), new DateTimeConverter());
            m_dic_type_map.Add(typeof(DataTable).GetHashCode(), new DateTableConverter());
        }

        public static STJsonConverter Get(Type type) {
            int nCode = type.GetHashCode();
            return STJsonBuildInConverter.Get(nCode);
        }

        public static STJsonConverter Get(int nCode) {
            if (m_dic_type_map.ContainsKey(nCode)) {
                return m_dic_type_map[nCode];
            }
            return null;
        }

        public class ByteConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToByte(json.Value);
            }
        }

        public class SByteConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToSByte(json.Value);
            }
        }

        public class ShortConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToInt16(json.Value);
            }
        }

        public class UShortConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToUInt16(json.Value);
            }
        }

        public class IntConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToInt32(json.Value);
            }
        }

        public class UIntConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToUInt32(json.Value);
            }
        }

        public class LongConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToInt64(json.Value);
            }
        }

        public class ULongConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToInt64(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToUInt64(json.Value);
            }
        }

        public class FloatConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToSingle(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToSingle(json.Value);
            }
        }

        public class DoubleConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToDouble(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToDouble(json.Value);
            }
        }

        public class DecimalConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return Convert.ToString(obj);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToDouble(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToDecimal(json.Value);
            }
        }

        public class BoolConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return obj == null ? "false" : Convert.ToString(obj).ToLower();
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToBoolean(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToBoolean(json.Value);
            }
        }

        public class CharConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return "\"" + STJson.Escape(Convert.ToString(obj)) + "\"";
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToString(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToChar(json.Value);
            }
        }

        public class StringConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return "\"" + STJson.Escape(Convert.ToString(obj)) + "\"";
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue(Convert.ToString(obj));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToString(json.Value);
            }
        }

        public class PointConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var point = (Point)obj;
                return string.Format("[{0},{1}]", point.X, point.Y);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                var point = (Point)obj;
                return new STJson().Append(point.X, point.Y);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return new Point(
                    json[0].GetValue<int>(),
                    json[1].GetValue<int>()
                    );
            }
        }

        public class PointFConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var point = (PointF)obj;
                return string.Format("[{0},{1}]", point.X, point.Y);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                var point = (PointF)obj;
                return new STJson().Append(point.X, point.Y);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return new PointF(
                    json[0].GetValue<float>(),
                    json[1].GetValue<float>()
                    );
            }
        }

        public class SizeConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var size = (Size)obj;
                return string.Format("[{0},{1}]", size.Width, size.Width);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                var size = (Size)obj;
                return new STJson().Append(size.Width, size.Width);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return new Size(
                    json[0].GetValue<int>(),
                    json[1].GetValue<int>()
                    );
            }
        }

        public class SizeFConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var size = (SizeF)obj;
                return string.Format("[{0},{1}]", size.Width, size.Width);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                var size = (SizeF)obj;
                return new STJson().Append(size.Width, size.Width);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return new SizeF(
                    json[0].GetValue<float>(),
                    json[1].GetValue<float>()
                    );
            }
        }

        public class RectangleConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var rect = (Rectangle)obj;
                return string.Format("[{0},{1},{2},{3}]", rect.X, rect.Y, rect.Width, rect.Height);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                var rect = (Rectangle)obj;
                return new STJson().Append(rect.X, rect.Y, rect.Width, rect.Height);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return new Rectangle(
                    json[0].GetValue<int>(),
                    json[1].GetValue<int>(),
                    json[2].GetValue<int>(),
                    json[3].GetValue<int>()
                    );
            }
        }

        public class RectangleFConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var rect = (RectangleF)obj;
                return string.Format("[{0},{1},{2},{3}]", rect.X, rect.Y, rect.Width, rect.Height);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                var rect = (RectangleF)obj;
                return new STJson().Append(rect.X, rect.Y, rect.Width, rect.Height);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return new RectangleF(
                    json[0].GetValue<float>(),
                    json[1].GetValue<float>(),
                    json[2].GetValue<float>(),
                    json[3].GetValue<float>()
                    );
            }
        }

        public class ColorConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var color = (Color)obj;
                return string.Format("[{0},{1},{3},{4}]", color.A, color.R, color.G, color.B);
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                var color = (Color)obj;
                return new STJson().Append(color.A, color.R, color.G, color.B);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Color.FromArgb(
                    json[0].GetValue<int>(),
                    json[1].GetValue<int>(),
                    json[2].GetValue<int>(),
                    json[3].GetValue<int>()
                    );
            }
        }

        public class DateTimeConverter : STJsonConverter
        {
            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                return "\"" + (obj == null ? "1970-01-01T00:00:00.0000+00:00" : ((DateTime)obj).ToString("O")) + "\"";
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = new STJson();
                json.SetValue((obj == null ? "1970-01-01T00:00:00.0000+00:00" : ((DateTime)obj).ToString("O")));
                return json;
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                return Convert.ToDateTime(json.Value);
            }
        }

        public class DateTableConverter : STJsonConverter
        {
            private static Type m_type_object = typeof(object);

            public override string ObjectToString(Type t, object obj, ref bool bProcessed) {
                var json = this.ObjectToJson(t, obj, ref bProcessed);
                return json == null ? null : json.ToString();
            }

            public override STJson ObjectToJson(Type t, object obj, ref bool bProcessed) {
                STJson json = STJson.CreateArray();
                DataTable dt = obj as DataTable;
                if (dt == null) {
                    return null;
                }
                STJson json_columns = STJson.CreateArray();
                STJson json_indices = STJson.CreateObject();
                foreach (DataColumn c in dt.Columns) {
                    json_columns.Append(STJson.New().SetItem("name", c.ColumnName).SetItem("caption", c.Caption));
                    json_indices.SetItem(c.ColumnName, json_indices.Count);
                }
                STJson json_rows = STJson.CreateArray();
                foreach (DataRow r in dt.Rows) {
                    STJson json_row = STJson.CreateArray();
                    foreach (DataColumn c in dt.Columns) {
                        json_row.Append(r[c.ColumnName]);
                    }
                    json_rows.Append(json_row);
                }
                return json
                    .SetItem("columns", json_columns)
                    .SetItem("indices", json_indices)
                    .SetItem("rows", json_rows);
            }

            public override object JsonToObject(Type t, STJson json, ref bool bProcessed) {
                DataTable dt = new DataTable();
                foreach (var c in json["columns"]) {
                    DataColumn dc = new DataColumn(c["name"].GetValue(), m_type_object);
                    dc.Caption = c["caption"].GetValue();
                    dt.Columns.Add(dc);
                }
                int nIndex = 0;
                foreach (var r in json["rows"]) {
                    var dr = dt.NewRow();
                    nIndex = 0;
                    foreach (var c in r) {
                        dr[nIndex++] = c.Value;
                    }
                    dt.Rows.Add(dr);
                }
                return dt;
            }
        }
    }
}
