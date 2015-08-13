using System.Threading.Tasks;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.Repository;

namespace Ruzzie.SensorData.GetData
{
    public class DataReadServiceWithCache : IDataReadService
    {
        private readonly ISensorItemDataRepository _sensorItemDataRepository;

        public DataReadServiceWithCache(IWriteThroughCache tierOneWriteThroughCache, IWriteThroughCache tierTwoWriteThroughCache, ISensorItemDataRepository sensorItemDataRepository)
        {
            _sensorItemDataRepository = sensorItemDataRepository;
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
                //update tier one cache if newer and return item
                await TierOneWriteThroughCache.Update(itemFromCache);
                return itemFromCache;
            }                     
            //3. read from real datastore
            return await StoreDocumentInCacheIfNotNull(_sensorItemDataRepository.GetLatest(thingName));
        }

        private async Task<SensorItemDataDocument> StoreDocumentInCacheIfNotNull(Task<SensorItemDataDocument> getLatest)
        {
            SensorItemDataDocument sensorItemDataDocument = await getLatest;
            await Task.WhenAny(TierOneWriteThroughCache.Update(sensorItemDataDocument),TierTwoWriteThroughCache.Update(sensorItemDataDocument));
            return sensorItemDataDocument;
        }
    }
}