using System;
using System.Threading.Tasks;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.Repository;

namespace Ruzzie.SensorData.PushData
{
    public class DataWriteServiceWithCache : IDataWriteService
    {

        private readonly ISensorItemDataRepository _sensorItemDataRepositoryMongo;

        public DataWriteServiceWithCache(IWriteThroughCache tierOneWriteThroughCache, IWriteThroughCache tierTwoWriteThroughCache,
            ISensorItemDataRepository sensorItemDataRepositoryMongo,
            ICacheUpdateSensorDocumentMessageChannel cacheUpdateCacheUpdateSensorDocumentMessageChannel)
        {
            _sensorItemDataRepositoryMongo = sensorItemDataRepositoryMongo;
            TierOneWriteThroughCache = tierOneWriteThroughCache;
            TierTwoWriteThroughCache = tierTwoWriteThroughCache;
            CacheUpdateCacheUpdateSensorDocumentMessageChannel = cacheUpdateCacheUpdateSensorDocumentMessageChannel;
        }

        protected IWriteThroughCache TierOneWriteThroughCache { get; set; }
        protected IWriteThroughCache TierTwoWriteThroughCache { get; set; }
        protected ICacheUpdateSensorDocumentMessageChannel CacheUpdateCacheUpdateSensorDocumentMessageChannel { get; set; }

        public async Task CreateOrUpdateDataForThing(string thingName, DateTime timestamp, dynamic data)
        {
            var dataDocument = new SensorItemDataDocument();
            dataDocument.ThingName = thingName;
            dataDocument.Created = timestamp;
            dataDocument.Content = data;

            //1. store for real TODO:ERROR HANDLING! 
            Task updateDatabaseTask = _sensorItemDataRepositoryMongo.CreateOrAdd(dataDocument);
            

            Task updateLocalCacheTask = TierOneWriteThroughCache.Update(dataDocument);
           

            Task updateDistributedCacheTask = TierTwoWriteThroughCache.Update(dataDocument)
                            .ContinueWith(task => CacheUpdateCacheUpdateSensorDocumentMessageChannel.Publish(thingName));
           

            await Task.WhenAll(updateDatabaseTask, updateLocalCacheTask, updateDistributedCacheTask);        
        }
    }
}