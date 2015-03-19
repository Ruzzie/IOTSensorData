using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Ruzzie.SensorData.Web.Repository
{
    
    public interface ISensorItemDataRepository
    {
        IQueryable<SensorItemDataDocument> SensorItemDataDocuments { get; }
        void CreateOrAdd(SensorItemDataDocument sensorItemData);
    }   

    public class SensorItemDataRepositoryMongo : ISensorItemDataRepository
    {
        private readonly MongoDatabase _mongoDatabase;

        public SensorItemDataRepositoryMongo(string connectionstring)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof (SensorItemDataDocument)))
            {
                BsonClassMap.RegisterClassMap<SensorItemDataDocument>(
                    cm =>
                    {                        
                        cm.MapIdProperty(obj => obj.Id).SetIdGenerator(new StringObjectIdGenerator());
                        cm.MapProperty(obj => obj.ThingName);
                        cm.MapProperty(obj => obj.Created);                                                                
                        //cm.MapProperty(obj => obj.RawContent).SetElementName("Content");                        
                        cm.MapProperty(obj => obj.Content);
                    });
            }

            var mongoClient = new MongoClient(connectionstring);

            MongoServer mongoServer = mongoClient.GetServer();
            _mongoDatabase = mongoServer.GetDatabase("sensordatatestdb");
        }

        public MongoCollection<SensorItemDataDocument> SensorItemDataCollection
        {
            get { return _mongoDatabase.GetCollection<SensorItemDataDocument>("SensorItemData"); }
        }

        public IQueryable<SensorItemDataDocument> SensorItemDataDocuments
        {
            get { return SensorItemDataCollection.AsQueryable(); }
        }

        public void CreateOrAdd(SensorItemDataDocument sensorItemData)
        {
            SensorItemDataCollection.Insert(sensorItemData);
        }

        internal void RemoveAllSensorDataItems()
        {
            _mongoDatabase.GetCollection<SensorItemDataDocument>("SensorItemData").RemoveAll();
        }
    }
}