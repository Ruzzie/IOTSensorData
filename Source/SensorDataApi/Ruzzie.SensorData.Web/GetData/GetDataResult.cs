using System;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
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