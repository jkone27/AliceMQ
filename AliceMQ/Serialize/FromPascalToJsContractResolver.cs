using Newtonsoft.Json.Serialization;

namespace AliceMQ.Serialize
{
    public class FromPascalToJsContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return base.ResolvePropertyName(propertyName).FromPascalToJson();
        }
    }
}