using System;
using Akka.Actor;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.Repository;

namespace Ruzzie.SensorData.Web.PushData
{
    public class UpdateSensorDataActor: TypedActor, IHandle<UpdateSensorDataDocumentMessage>
    {
        private readonly IActorRef _updateDatabaseActor;
        private readonly IActorRef _updateLocalCacheActor;
        private readonly IActorRef _updateDistributedCacheActor;

        public UpdateSensorDataActor(IActorRef updateDatabaseActor, IActorRef updateLocalCacheActor, IActorRef updateDistributedCacheActor)
        {
            _updateDatabaseActor = updateDatabaseActor;
            _updateLocalCacheActor = updateLocalCacheActor;
            _updateDistributedCacheActor = updateDistributedCacheActor;
        }

        public void Handle(UpdateSensorDataDocumentMessage message)
        {
            _updateDatabaseActor.Tell(message);
            _updateLocalCacheActor.Tell(message);
            _updateDistributedCacheActor.Tell(message);
        }
    }

    public class UpdateDatabaseActor : TypedActor, IHandle<UpdateSensorDataDocumentMessage>
    {
        private readonly ISensorItemDataRepository _sensorItemDataRepository;

        public UpdateDatabaseActor(ISensorItemDataRepository sensorItemDataRepository)
        {
            _sensorItemDataRepository = sensorItemDataRepository;
        }

        public void Handle(UpdateSensorDataDocumentMessage message)
        {
            _sensorItemDataRepository.CreateOrAdd(message.SensorItemDataDocument).Wait();
        }
    }

    public class UpdateLocalCacheActor : TypedActor, IHandle<UpdateSensorDataDocumentMessage>
    {
        private readonly IWriteThroughCache _localCache;

        public UpdateLocalCacheActor(IWriteThroughCache localCache)
        {
            _localCache = localCache;
        }

        public void Handle(UpdateSensorDataDocumentMessage message)
        {
            _localCache.Update(message.SensorItemDataDocument).Wait();
        }
    }

    public class UpdateDistributedCacheActor : TypedActor, IHandle<UpdateSensorDataDocumentMessage>
    {
        private readonly IWriteThroughCache _distributedCache;
        private readonly ICacheUpdateSensorDocumentMessageChannel _updateCacheChannel;

        public UpdateDistributedCacheActor(IWriteThroughCache distributedCache, ICacheUpdateSensorDocumentMessageChannel updateCacheChannel)
        {
            _distributedCache = distributedCache;
            _updateCacheChannel = updateCacheChannel;
        }

        public void Handle(UpdateSensorDataDocumentMessage message)
        {
            _distributedCache.Update(message.SensorItemDataDocument).Wait();
            _updateCacheChannel.Publish(message.SensorItemDataDocument.ThingName).Wait();
        }
    }

    public class UpdateSensorDataDocumentMessage
    {

        public UpdateSensorDataDocumentMessage(string thingName, DateTime created, DynamicObjectDictionary content)
        {
            SensorItemDataDocument = new SensorItemDataDocument {ThingName = thingName, Created = created, Content = content};
        }


        public SensorItemDataDocument SensorItemDataDocument { get; private set; }
    }
}