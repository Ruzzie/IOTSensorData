using System;
using System.Threading.Tasks;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.Web.Cache
{
    public interface IWriteThroughCache
    {
        Task Update(SensorItemDataDocument dataDocument);
        SensorItemDataDocument GetLatest(string thingName);
        Task<int> PruneOldestItemCacheForItemsOlderThan(TimeSpan age);
    }
}