using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Newtonsoft.Json;

namespace Ruzzie.SensorData.Web
{
    [Serializable]
    public class SensorItemDataDocument
    {
        public string ThingName { get; set; }
        public DateTime Created { get; set; }

        [BsonId(IdGenerator = typeof (StringObjectIdGenerator))]
        public string Id { get; set; }

        //[BsonIgnore]
        public dynamic Content { get; set; }

        //Only for serialization purposes
        //[BsonElement("Content")]
        //internal string RawContent
        //{
        //    get { return JsonConvert.SerializeObject(Content); }
        //    set { Content = JsonConvert.DeserializeObject<DynamicDictionaryObject>(value); }
        //}
    }
}