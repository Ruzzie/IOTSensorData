using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Ruzzie.SensorData.Web.Repository
{
    public static class MongoClassMapBootstrap
    {
        public static void Register()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof (SensorItemDataDocument)))
            {
                BsonClassMap.RegisterClassMap(new SensorItemDataDocumentBsonClassMap());
            }
        }
    }

    public class SensorItemDataDocumentBsonClassMap : BsonClassMap<SensorItemDataDocument>
    {
        public SensorItemDataDocumentBsonClassMap()
        {
            MapIdProperty(obj => obj.Id).SetIdGenerator(new StringObjectIdGenerator());
            MapProperty(obj => obj.ThingName);
            MapProperty(obj => obj.Created);
            MapProperty(obj => obj.Content).SetSerializer(new DynamicObjectDictionarySerializer());
        }
    }
}