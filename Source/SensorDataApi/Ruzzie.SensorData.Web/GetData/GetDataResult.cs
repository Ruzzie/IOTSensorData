using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ruzzie.SensorData.Web.Annotations;

namespace Ruzzie.SensorData.Web.GetData
{
    /// <summary>
    /// Result of data stored for a thing.
    /// </summary>
    public class GetDataResult
    {
        private dynamic _resultData;

        /// <summary>
        /// The result code of the GetData request.
        /// </summary>
        [JsonConverter(typeof (StringEnumConverter))]
        public GetDataResultCode GetDataResultCode { [UsedImplicitly] get; [UsedImplicitly] set; }

        /// <summary>
        /// The data object.
        /// </summary>
        public dynamic ResultData
        {
            get { return _resultData; }
            set
            {
                _resultData = DynamicDictionaryHelpers.CreateDynamicValueAsDynamicDictionaryWhenTypeIsConvertable(value);
            }
        }
        
        /// <summary>
        /// The Timestamp of the data. When the data was stored.
        /// </summary>
        public DateTime Timestamp { [UsedImplicitly] get; set; }
        
        /// <summary>
        /// The name of the thing this data belongs to.
        /// </summary>
        public string ThingName { get; set; }
    }
}