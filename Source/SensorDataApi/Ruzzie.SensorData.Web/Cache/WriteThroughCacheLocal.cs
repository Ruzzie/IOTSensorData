using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.Cache
{
    public class WriteThroughCacheLocal : IWriteThroughCache
    {
        private readonly ICacheUpdateSensorDocumentMessageChannel _cacheUpdateSensorDocumentMessageChannel;
        private static readonly ConcurrentDictionary<string, SensorItemDataDocument> LatestEntryCache = new ConcurrentDictionary<string, SensorItemDataDocument>(StringComparer.OrdinalIgnoreCase);

        public WriteThroughCacheLocal()
        {
            
        }

        public WriteThroughCacheLocal(ICacheUpdateSensorDocumentMessageChannel cacheUpdateSensorDocumentMessageChannel)
        {
            if (cacheUpdateSensorDocumentMessageChannel == null)
            {
                throw new ArgumentNullException("cacheUpdateSensorDocumentMessageChannel");
            }

            _cacheUpdateSensorDocumentMessageChannel = cacheUpdateSensorDocumentMessageChannel;
            cacheUpdateSensorDocumentMessageChannel.Subscribe(LatestThingIsUpdatedNotification);
        }

        private void LatestThingIsUpdatedNotification(string thingName)
        {
            SensorItemDataDocument document;
            LatestEntryCache.TryRemove(thingName, out document);
        }

        public async Task Update(SensorItemDataDocument dataDocument)
        {
            if (dataDocument == null)
            {
                return;
            }

            await Task.Run(() =>
            {
                LatestEntryCache.AddOrUpdate(dataDocument.ThingName, dataDocument, (key, oldValue) =>
                {
                    if (dataDocument.Created > oldValue.Created)
                    {
                        return dataDocument;
                    }
                    return oldValue;
                });
                
            });
        }

        public async Task<SensorItemDataDocument> GetLatest(string thingName)
        {
            return await Task.Run(() =>
            {
                SensorItemDataDocument document;
                if (LatestEntryCache.TryGetValue(thingName, out document))
                {
                    return document;
                }
                return null;
            });
        }

        public async Task<int> PruneOldestItemCacheForItemsOlderThan(TimeSpan age)
        {
            ParallelQuery<string> itemsToPrune =
                LatestEntryCache.AsParallel().Where(item => item.Value.Created < DateTime.UtcNow.Subtract(age)).Select(item => item.Key);

            return await RemoveItems(itemsToPrune);
        }

        public void ResetLatestEntryCache()
        {
            LatestEntryCache.Clear();
        }

        public void RemoveItemFromLatestEntryCache(string thingName)
        {
            SensorItemDataDocument document;
            LatestEntryCache.TryRemove(thingName, out document);
        }

        private async Task<int> RemoveItems(ParallelQuery<string> itemsToPrune)
        {
            return await Task.Run(() =>
            {
                return itemsToPrune.Count(key =>
                {
                    SensorItemDataDocument document;
                    return LatestEntryCache.TryRemove(key, out document);
                });
            });
        }
    }
}