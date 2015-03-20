using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ruzzie.SensorData.Web.Repository
{
    public interface ISensorItemDataRepository
    {
        Task CreateOrAdd(SensorItemDataDocument sensorItemData);
        Task<SensorItemDataDocument> GetLatest(string thingName);
    }

    public class SensorItemDataRepositoryMongo : ISensorItemDataRepository
    {
        private readonly IMongoDatabase _mongoDatabase;

        public SensorItemDataRepositoryMongo(string connectionstring)
        {
            var mongoClient = new MongoClient(connectionstring);

            _mongoDatabase = mongoClient.GetDatabase("sensordatatestdb");
        }

        public IMongoCollection<SensorItemDataDocument> SensorItemDataCollection
        {
            get { return _mongoDatabase.GetCollection<SensorItemDataDocument>("SensorItemData"); }
        }

        public async Task CreateOrAdd(SensorItemDataDocument sensorItemData)
        {
            await SensorItemDataCollection.InsertOneAsync(sensorItemData);
        }

        public async Task<SensorItemDataDocument> GetLatest(string thingName)
        {
            return
                await
                    SensorItemDataCollection.Find(item => item.ThingName == thingName)
                        .SortByDescending(item => item.Created)
                        .FirstOrDefaultAsync();
        }

        internal Task<DeleteResult> RemoveAllSensorDataItems()
        {
            return SensorItemDataCollection.DeleteManyAsync(new BsonDocument());
        }
    }
}