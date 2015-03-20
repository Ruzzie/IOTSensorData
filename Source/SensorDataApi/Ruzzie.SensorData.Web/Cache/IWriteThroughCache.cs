using System;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.Cache
{
    public interface IWriteThroughCache
    {
        Task<SensorItemDataDocument> Update(SensorItemDataDocument dataDocument);
        Task<SensorItemDataDocument> GetLatest(string thingName);
        Task<int> PruneOldestItemCacheForItemsOlderThan(TimeSpan age);
        void ResetLatestEntryCache();
    }
}