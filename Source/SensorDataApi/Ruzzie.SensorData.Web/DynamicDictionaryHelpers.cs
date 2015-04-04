using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ruzzie.SensorData.Web
{
    public static class DynamicDictionaryHelpers
    {
        public static dynamic CreateDynamicValueAsDynamicDictionaryWhenTypeIsConvertible(dynamic valueToSet)
        {
            if (valueToSet is ExpandoObject)
            {
                return new DynamicObjectDictionary(valueToSet);                
            }

            var token = valueToSet as JToken;
            if (token != null)
            {
                return JsonConvert.DeserializeObject<DynamicObjectDictionary>(token.ToString());                
            }
            return valueToSet;
        }
    }
}