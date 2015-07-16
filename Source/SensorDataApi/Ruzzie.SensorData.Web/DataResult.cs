using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ruzzie.SensorData.Web.Annotations;

namespace Ruzzie.SensorData.Web
{
    [DataContract]
    [KnownType(typeof(DynamicObjectDictionary))]
    public abstract class DataResultBase<T>
    {
        /// <summary>
        /// The result code of the GetData request.
        /// </summary>

        [DataMember]
        [JsonConverter(typeof (StringEnumConverter))]        
        public DataResultCode DataResultCode { [UsedImplicitly] get; [UsedImplicitly] set; }

        /// <summary>
        /// The data object.
        /// </summary>
        public abstract T ResultData { get; set; }

        /// <summary>
        /// The Timestamp of the data. When the data was stored.
        /// </summary>
        [DataMember]
        public DateTime Timestamp { [UsedImplicitly] get; set; }

        /// <summary>
        /// The name of the thing this data belongs to.
        /// </summary>
        [DataMember]
        public string ThingName { get; set; }
    }

    /// <summary>
    /// Result of data stored for a thing.
    /// </summary>   
    public class DataResult : DataResultBase<dynamic>
    {
        private dynamic _resultData;

        /// <summary>
        /// The data object.
        /// </summary>
        [DataMember]
        public override dynamic ResultData
        {
            get { return _resultData; }
            set
            {
                _resultData = DynamicDictionaryHelpers.CreateDynamicValueAsDynamicDictionaryWhenTypeIsConvertible(value);
            }
        }
    }
}