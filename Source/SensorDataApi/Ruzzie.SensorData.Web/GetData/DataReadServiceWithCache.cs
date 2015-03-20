using System.Threading.Tasks;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.GetData
{
    public class DataReadServiceWithCache : IDataReadService
    {
        private readonly ISensorItemDataRepository _sensorItemDataRepositoryMongo;

        public DataReadServiceWithCache(IWriteThroughCache writeThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo)
        {
            _sensorItemDataRepositoryMongo = sensorItemDataRepositoryMongo;
            WriteThroughCache = writeThroughCache;
        }

        protected IWriteThroughCache WriteThroughCache { get; set; }

        public async Task<SensorItemDataDocument> GetLatestEntryForThing(string thingName)
        {
            //1. look in cache
            SensorItemDataDocument itemFromCache = WriteThroughCache.GetLatest(thingName);
            if (itemFromCache != null)
            {
                return itemFromCache;
            }
            //2. read from real datastore
            return await StoreDocumentInCacheIfNotNull(_sensorItemDataRepositoryMongo.GetLatest(thingName));
        }

        private async Task<SensorItemDataDocument> StoreDocumentInCacheIfNotNull(Task<SensorItemDataDocument> getLatest)
        {
            SensorItemDataDocument documentFromStore = await getLatest;            
            await WriteThroughCache.Update(documentFromStore);
            return documentFromStore;
        }
    }
}