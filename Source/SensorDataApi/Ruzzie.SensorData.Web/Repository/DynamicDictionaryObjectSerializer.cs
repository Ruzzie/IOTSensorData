using System.Collections.Generic;
using System.Dynamic;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Ruzzie.SensorData.Web.Repository
{
    public class DynamicDictionaryObjectSerializer : DynamicDocumentBaseSerializer<DynamicDictionaryObject>,
        IBsonSerializer<DynamicDictionaryObject>
    {
        private static readonly IBsonSerializer<BsonDocument> BsonDocumentSerializer = BsonSerializer.LookupSerializer<BsonDocument>();
        private static readonly IBsonSerializer<List<object>> ListSerializer = BsonSerializer.LookupSerializer<List<object>>();

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        public new void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DynamicDictionaryObject value)
        {
            IBsonWriter writer = context.Writer;

            if (writer.State == BsonWriterState.Value && value != null)
            {
                BsonDocument document = BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(value));
                BsonDocumentSerializer.Serialize(context, args, document);
            }
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, value as DynamicDictionaryObject);
        }

        protected override void ConfigureDeserializationContext(BsonDeserializationContext.Builder builder)
        {
            builder.DynamicDocumentSerializer = this;
            builder.DynamicArraySerializer = ListSerializer;
        }

        protected override void ConfigureSerializationContext(BsonSerializationContext.Builder builder)
        {
            builder.IsDynamicType = t => t == typeof (DynamicDictionaryObject) || t == typeof (List<object>) || t == typeof (ExpandoObject);
        }

        protected override DynamicDictionaryObject CreateDocument()
        {
            return new DynamicDictionaryObject();
        }

        protected override void SetValueForMember(DynamicDictionaryObject document, string memberName, object value)
        {
            ((IDictionary<string, object>) document)[memberName] = value;
        }

        protected override bool TryGetValueForMember(DynamicDictionaryObject document, string memberName, out object value)
        {
            return ((IDictionary<string, object>) document).TryGetValue(memberName, out value);
        }
    }
}