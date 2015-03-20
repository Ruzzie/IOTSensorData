using System.Threading.Tasks;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.GetData
{
    public class DataReadServiceWithCache : IDataReadService
    {
        private readonly ISensorItemDataRepository _sensorItemDataRepositoryMongo;

        public DataReadServiceWithCache(IWriteThroughCache tierOnewriteThroughCache, IWriteThroughCache tierTwowriteThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo)
        {
            _sensorItemDataRepositoryMongo = sensorItemDataRepositoryMongo;
            TierOnewriteThroughCache = tierOnewriteThroughCache;
            TierTwowriteThroughCache = tierTwowriteThroughCache;
        }

        protected IWriteThroughCache TierOnewriteThroughCache { get; set; }
        protected IWriteThroughCache TierTwowriteThroughCache { get; set; }

        public async Task<SensorItemDataDocument> GetLatestEntryForThing(string thingName)
        {
            //1. look in tier 1 cache
            SensorItemDataDocument itemFromCache = await TierOnewriteThroughCache.GetLatest(thingName);
            if (itemFromCache != null)
            {
                return itemFromCache;
            }

            //2. look in tier 2 cache
            itemFromCache = await TierTwowriteThroughCache.GetLatest(thingName);

            if (itemFromCache != null)
            {
                return itemFromCache;
            }                     
            //3. read from real datastore
            return await StoreDocumentInCacheIfNotNull(_sensorItemDataRepositoryMongo.GetLatest(thingName));
        }

        private async Task<SensorItemDataDocument> StoreDocumentInCacheIfNotNull(Task<SensorItemDataDocument> getLatest)
        {            
            await Task.WhenAll(TierOnewriteThroughCache.Update(await getLatest),TierTwowriteThroughCache.Update(await getLatest));
            return await getLatest;
        }
    }
}