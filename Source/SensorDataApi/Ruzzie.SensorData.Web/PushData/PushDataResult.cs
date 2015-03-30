using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
                _resultData = DynamicDictionaryHelpers.CreateDynamicValueAsDynamicDictionaryWhenTypeIsConvertable(value);
            }
        }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public string ThingName { get; set; }
    }
    

}