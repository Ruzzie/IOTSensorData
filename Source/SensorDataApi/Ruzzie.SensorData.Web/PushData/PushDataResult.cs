using System;
using System.Dynamic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Ruzzie.SensorData.Web.PushData
{
    [KnownType(typeof (DynamicDictionaryObject))]
    [DataContract]
    public class PushDataResult
    {
        private dynamic _resultData;

        [DataMember]
        [JsonConverter(typeof (StringEnumConverter))]
        public PushDataResultCode PushDataResultCode { get; set; }

        [DataMember]        
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

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public string ThingName { get; set; }
    }
    

}