﻿using System;
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

        public DataWriteServiceWithCache(IWriteThroughCache tierOneWriteThroughCache, IWriteThroughCache tierTwoWriteThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo)
        {
            _sensorItemDataRepositoryMongo = sensorItemDataRepositoryMongo;
            TierOneWriteThroughCache = tierOneWriteThroughCache;
            TierTwoWriteThroughCache = tierTwoWriteThroughCache;            
        }

        public DataWriteServiceWithCache(IWriteThroughCache tierOneWriteThroughCache, IWriteThroughCache tierTwoWriteThroughCache, ISensorItemDataRepository sensorItemDataRepositoryMongo, IUpdateSensorDocumentMessageChannel updateUpdateSensorDocumentMessageChannel)
            : this(tierOneWriteThroughCache, tierTwoWriteThroughCache, sensorItemDataRepositoryMongo)
        {
            UpdateUpdateSensorDocumentMessageChannel = updateUpdateSensorDocumentMessageChannel;            
        }

        protected IWriteThroughCache TierOneWriteThroughCache { get; set; }
        protected IWriteThroughCache TierTwoWriteThroughCache { get; set; }
        protected IUpdateSensorDocumentMessageChannel UpdateUpdateSensorDocumentMessageChannel { get; set; }

        public async Task CreateOrUpdateDataForThing(string thingName, DateTime timeStamp, dynamic data)
        {
            var dataDocument = new SensorItemDataDocument();
            dataDocument.ThingName = thingName;
            dataDocument.Created = timeStamp;
            dataDocument.Content = data;

            Task.Run(() => UpdateUpdateSensorDocumentMessageChannel.Publish(thingName));

            //1. store for real TODO:ERROR HANDLING!            
            await Task.WhenAny(_sensorItemDataRepositoryMongo.CreateOrAdd(dataDocument), Task.WhenAny(TierOneWriteThroughCache.Update(dataDocument), TierTwoWriteThroughCache.Update(dataDocument)));
        }
    }
}