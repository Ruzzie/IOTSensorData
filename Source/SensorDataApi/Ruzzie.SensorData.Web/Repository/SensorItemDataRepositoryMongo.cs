using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace Ruzzie.SensorData.Web.Repository
{
    public interface ISensorItemDataRepository
    {
        Task CreateOrAdd(SensorItemDataDocument sensorItemData);
        Task<SensorItemDataDocument> GetLatest(string thingName);
    }

    public class SensorItemDataRepositoryMongo : ISensorItemDataRepository
    {
        private IMongoDatabase _mongoDatabase;

        public SensorItemDataRepositoryMongo(string connectionString)
        {
            ConnectAndGetDatabase(connectionString);
        }

        private void ConnectAndGetDatabase(string connectionString)
        {
            var mongoClient = new MongoClient(connectionString);
            var str = new ConnectionString(connectionString);
            _mongoDatabase = mongoClient.GetDatabase(str.DatabaseName);
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