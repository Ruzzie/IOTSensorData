using System;
using System.Threading.Tasks;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.PushData
{
    public interface IDataWriteService
    {
        Task CreateOrUpdateDataForThing(string thingName, DateTime timeStamp, dynamic data);
    }

    public class DataWriteServiceWithCache : IDataWriteService
    {
        private readonly ISensorItemDataRepository _sensorItemDataRepositoryMongo;

        public DataWriteServiceWithCache(IWriteThroughCache writeThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo)
        {
            _sensorItemDataRepositoryMongo = sensorItemDataRepositoryMongo;
            WriteThroughCache = writeThroughCache;
        }

        protected IWriteThroughCache WriteThroughCache { get; set; }

        public async Task CreateOrUpdateDataForThing(string thingName, DateTime timeStamp, dynamic data)
        {
            var dataDocument = new SensorItemDataDocument();
            dataDocument.ThingName = thingName;
            dataDocument.Created = timeStamp;
            dataDocument.Content = data;

            //1. store for real TODO:ERROR HANDLING!
            Task updateData = Task.Run(() => _sensorItemDataRepositoryMongo.CreateOrAdd(dataDocument));
            //2. on success store in cache
            await Task.WhenAll(updateData, WriteThroughCache.Update(dataDocument));
        }
    }
}