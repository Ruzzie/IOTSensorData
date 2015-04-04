using System.Threading.Tasks;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.GetData
{
    public class DataReadServiceWithCache : IDataReadService
    {
        private readonly ISensorItemDataRepository _sensorItemDataRepositoryMongo;

        public DataReadServiceWithCache(IWriteThroughCache tierOneWriteThroughCache, IWriteThroughCache tierTwoWriteThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo)
        {
            _sensorItemDataRepositoryMongo = sensorItemDataRepositoryMongo;
            TierOneWriteThroughCache = tierOneWriteThroughCache;
            TierTwoWriteThroughCache = tierTwoWriteThroughCache;
        }

        protected IWriteThroughCache TierOneWriteThroughCache { get; set; }
        protected IWriteThroughCache TierTwoWriteThroughCache { get; set; }

        public async Task<SensorItemDataDocument> GetLatestEntryForThing(string thingName)
        {
            //1. look in tier 1 cache
            SensorItemDataDocument itemFromCache = await TierOneWriteThroughCache.GetLatest(thingName);
            if (itemFromCache != null)
            {
                return itemFromCache;
            }

            //2. look in tier 2 cache
            itemFromCache = await TierTwoWriteThroughCache.GetLatest(thingName);

            if (itemFromCache != null)
            {
                return itemFromCache;
            }                     
            //3. read from real datastore
            return await StoreDocumentInCacheIfNotNull(_sensorItemDataRepositoryMongo.GetLatest(thingName));
        }

        private async Task<SensorItemDataDocument> StoreDocumentInCacheIfNotNull(Task<SensorItemDataDocument> getLatest)
        {            
            await Task.WhenAny(TierOneWriteThroughCache.Update(await getLatest),TierTwoWriteThroughCache.Update(await getLatest));
            return await getLatest;
        }
    }
}