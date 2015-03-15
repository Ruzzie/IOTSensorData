using System.Linq;
using System.Threading.Tasks;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.PushData;
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
            await Task.Run(
                () =>
                {
                    SensorItemDataDocument documentFromStore = _sensorItemDataRepositoryMongo.SensorItemDataDocuments.OrderByDescending(
                        item => item.Created)
                        .FirstOrDefault(item => item.ThingName == thingName);
                    if (documentFromStore != null)
                    {
                        return WriteThroughCache.Update(documentFromStore);
                    }
                    return Task.FromResult<SensorItemDataDocument>(null);
                });

            return null;
        }
    }
}