using System;
using System.Collections.Generic;
using System.Dynamic;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Ruzzie.SensorData.Web.Repository
{
    public class DynamicObjectDictionaryBsonSerializer : DynamicDocumentBaseSerializer<DynamicObjectDictionary>,
        IBsonSerializer<DynamicObjectDictionary>
    {
        private static readonly IBsonSerializer<BsonDocument> BsonDocumentSerializer = BsonSerializer.LookupSerializer<BsonDocument>();
        private static readonly IBsonSerializer<List<object>> ListSerializer = BsonSerializer.LookupSerializer<List<object>>();

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        public new void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DynamicObjectDictionary value)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IBsonWriter writer = context.Writer;

            if (writer.State == BsonWriterState.Value && value != null)
            {
                BsonDocument document = BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(value));
                BsonDocumentSerializer.Serialize(context, args, document);
            }
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, value as DynamicObjectDictionary);
        }

        protected override void ConfigureDeserializationContext(BsonDeserializationContext.Builder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            builder.DynamicDocumentSerializer = this;
            builder.DynamicArraySerializer = ListSerializer;
        }

        protected override void ConfigureSerializationContext(BsonSerializationContext.Builder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            builder.IsDynamicType = t => t == typeof (DynamicObjectDictionary) || t == typeof (List<object>) || t == typeof (ExpandoObject);
        }

        protected override DynamicObjectDictionary CreateDocument()
        {
            return new DynamicObjectDictionary();
        }

        protected override void SetValueForMember(DynamicObjectDictionary document, string memberName, object value)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            ((IDictionary<string, object>) document)[memberName] = value;
        }

        protected override bool TryGetValueForMember(DynamicObjectDictionary document, string memberName, out object value)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            return ((IDictionary<string, object>) document).TryGetValue(memberName, out value);
        }
    }
}