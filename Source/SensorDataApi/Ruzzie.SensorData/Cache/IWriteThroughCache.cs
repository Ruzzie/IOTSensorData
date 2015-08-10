using System;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Cache
{
    public interface IWriteThroughCache
    {
        Task Update(SensorItemDataDocument dataDocument);
        Task<SensorItemDataDocument> GetLatest(string thingName);
        Task<int> PruneOldestItemCacheForItemsOlderThan(TimeSpan age);
        void ResetLatestEntryCache();
        void RemoveItemFromLatestEntryCache(string thingName);
    }
}