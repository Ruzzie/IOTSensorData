using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ruzzie.SensorData.Web.PushData
{
    [KnownType(typeof (DynamicObjectDictionary))]
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
                _resultData = DynamicDictionaryHelpers.CreateDynamicValueAsDynamicDictionaryWhenTypeIsConvertible(value);
            }
        }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string ThingName { get; set; }
    }
    

}