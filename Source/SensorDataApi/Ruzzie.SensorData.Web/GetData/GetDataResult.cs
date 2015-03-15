using System;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Ruzzie.SensorData.Web.Annotations;

namespace Ruzzie.SensorData.Web.GetData
{
    public class GetDataResult
    {
        private dynamic _resultData;

        [JsonConverter(typeof (StringEnumConverter))]
        public GetDataResultCode GetDataResultCode { [UsedImplicitly] get; set; }

        public dynamic ResultData
        {
            get { return _resultData; }
            set
            {
                if (value is ExpandoObject)
                {
                    _resultData = new DynamicDictionaryObject(value);
                    return;
                }

                var token = value as JToken;
                if (token != null)
                {
                    _resultData = JsonConvert.DeserializeObject<DynamicDictionaryObject>(token.ToString());
                    return;
                }
                _resultData = value;
            }
        }
        public DateTime Timestamp { [UsedImplicitly] get; set; }
        public string ThingName { get; set; }
    }
}