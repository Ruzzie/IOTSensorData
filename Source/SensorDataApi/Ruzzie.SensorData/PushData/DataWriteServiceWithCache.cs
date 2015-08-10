using System;
using System.Threading.Tasks;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.Repository;

namespace Ruzzie.SensorData.PushData
{
    public class DataWriteServiceWithCache : IDataWriteService
    {
        
        private readonly ISensorItemDataRepository _sensorItemDataRepositoryMongo;

        public DataWriteServiceWithCache(IWriteThroughCache tierOneWriteThroughCache, IWriteThroughCache tierTwoWriteThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo)
        {
            _sensorItemDataRepositoryMongo = sensorItemDataRepositoryMongo;
            TierOneWriteThroughCache = tierOneWriteThroughCache;
            TierTwoWriteThroughCache = tierTwoWriteThroughCache;            
        }

        public DataWriteServiceWithCache(IWriteThroughCache tierOneWriteThroughCache, IWriteThroughCache tierTwoWriteThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo, ICacheUpdateSensorDocumentMessageChannel cacheUpdateCacheUpdateSensorDocumentMessageChannel)
            : this(tierOneWriteThroughCache, tierTwoWriteThroughCache, sensorItemDataRepositoryMongo)
        {
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
            await
                Task.WhenAny(_sensorItemDataRepositoryMongo.CreateOrAdd(dataDocument),
                    Task.WhenAll(TierOneWriteThroughCache.Update(dataDocument),
                        TierTwoWriteThroughCache.Update(dataDocument).ContinueWith(task => CacheUpdateCacheUpdateSensorDocumentMessageChannel.Publish(thingName)))
                    );
        }
    }
}