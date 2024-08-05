namespace STLib.Json
{
    public partial class STJson
    {
        public static STJson Create(STJsonCreatorStartCallback callback)
        {
            return new STJsonCreator().Create(callback);
        }
    }
}