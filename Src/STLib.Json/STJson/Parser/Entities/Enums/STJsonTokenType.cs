namespace STLib.Json
{
    internal enum STJsonTokenType
    {
        None, Symbol, /*Keyword,*/ Long, Double, String, ItemSplitor, KVSplitor, ObjectStart, ObjectEnd, ArrayStart, ArrayEnd, Comment
    }
}
