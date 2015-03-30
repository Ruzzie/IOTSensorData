using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ruzzie.SensorData.Web
{
    public static class DynamicDictionaryHelpers
    {
        public static dynamic CreateDynamicValueAsDynamicDictionaryWhenTypeIsConvertable(dynamic valueToSet)
        {
            if (valueToSet is ExpandoObject)
            {
                return new DynamicDictionaryObject(valueToSet);                
            }

            var token = valueToSet as JToken;
            if (token != null)
            {
                return JsonConvert.DeserializeObject<DynamicDictionaryObject>(token.ToString());                
            }
            return valueToSet;
        }
    }
}