using System.Linq;

namespace STLib.Json
{
    internal class STJsonPathItem
    {
        public static readonly STJsonPathItem Root = new STJsonPathItem(ItemType.Root);
        public static readonly STJsonPathItem Current = new STJsonPathItem(ItemType.Current);
        public static readonly STJsonPathItem Any = new STJsonPathItem(ItemType.Any);
        public static readonly STJsonPathItem Depth = new STJsonPathItem(ItemType.Depth);

        public string Text {
            get {
                switch (this.Type) {
                    case ItemType.Root: return "[$]";
                    case ItemType.Current: return "[@]";
                    case ItemType.Any: return "[*]";
                    case ItemType.Depth: return "[..]";
                    case ItemType.Slice: return "[" + this.Start + ":" + this.End + ":" + this.Step + "]";
                    case ItemType.List:
                        return "["
                            + (string.Join(",", (from a in this.Indices select a.ToString()).ToArray())
                            + ","
                            + string.Join(",", (from a in this.Keys select ("'" + STJson.Escape(a) + "'")).ToArray())).Trim(',')
                            + "]";
                    case ItemType.Express: return this.Express.Text;
                    default: return null;
                }
            }
        }
        public enum ItemType
        {
            Root, Current, List, Slice, Any, Depth, Express
        }

        public ItemType Type { get; private set; }

        public int Start { get; private set; }

        public int End { get; private set; }

        public int Step { get; private set; }

        public int[] Indices { get; set; }

        public string[] Keys { get; set; }

        public object[] List { get; set; }

        public STJsonPathExpression Express { get; set; }

        private STJsonPathItem(ItemType type) {
            this.Type = type;
        }

        public STJsonPathItem(int indices) {
            this.Type = ItemType.List;
            this.Indices = new int[] { indices };
            this.Keys = new string[0];
        }

        public STJsonPathItem(string keys) {
            this.Type = ItemType.List;
            this.Keys = new string[] { keys };
            this.Indices = new int[0];
        }

        public STJsonPathItem(int[] indices, string[] keys) {
            this.Type = ItemType.List;
            this.Indices = indices;
            this.Keys = keys;
        }

        public STJsonPathItem(int nStart, int nEnd, int nStep) {
            this.Type = ItemType.Slice;
            this.Start = nStart;
            this.End = nEnd;
            this.Step = nStep;
        }

        public STJsonPathItem(STJsonPathExpression express) {
            this.Type = ItemType.Express;
            this.Express = express;
        }

        public override string ToString() {
            return "[" + this.Type + "] - " + this.Text;
        }
    }
}

