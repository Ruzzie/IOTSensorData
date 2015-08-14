using System;
using System.Configuration;
using System.Web;
using Akka.Actor;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.GetData;
using Ruzzie.SensorData.Repository;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.GetData;
using Ruzzie.SensorData.Web.PushData;
using Ruzzie.SensorData.Web.Repository;
using StackExchange.Redis;

namespace Ruzzie.SensorData.Web
{
    internal static class Container
    {
        internal static readonly string MongoConnString;
        internal static readonly string RedisConnString;
      

        static Container()
        {
            MongoClassMapBootstrap.Register();
            MongoConnString = ConfigurationManager.AppSettings["mongodbconnectionstring"];
            RedisConnString = ConfigurationManager.AppSettings["redisconnectionstring"];


            Redis = CreateRedisConnectionMultiplexer();
            ICacheUpdateSensorDocumentMessageChannel cacheUpdateSensorDocumentMessageChannel = new RedisPubSubCacheUpdateSensorDocumentMessageChannel(Redis);

            ISensorItemDataRepository sensorItemDataRepositoryMongo = new SensorItemDataRepositoryMongo(MongoConnString);
            WriteThroughLocalCache = new WriteThroughCacheLocal(cacheUpdateSensorDocumentMessageChannel);
            RedisWriteThroughCache = new WriteThroughRedisCache(Redis, 600);

            TimeSpan localCacheExpiry = new TimeSpan(0,0,5,0);
            PruneLocalCacheJob = new WebJob(localCacheExpiry, () => WriteThroughLocalCache.PruneOldestItemCacheForItemsOlderThan(localCacheExpiry), HttpRuntime.Cache ?? new System.Web.Caching.Cache());

            GetDataService = new GetDataService(new DataReadServiceWithCache(WriteThroughLocalCache, RedisWriteThroughCache, sensorItemDataRepositoryMongo));

            ActorSystem = ActorSystem.Create("RuzzieSensorDataActorSystem");
            var updateDatabaseActorRef = ActorSystem.ActorOf(Props.Create(() => new UpdateDatabaseActor(sensorItemDataRepositoryMongo)));
            var updateLocalCacheActorRef = ActorSystem.ActorOf(Props.Create(() => new UpdateLocalCacheActor(WriteThroughLocalCache)));
            var updateDistributedCacheActorRef = ActorSystem.ActorOf(
                Props.Create(() => new UpdateDistributedCacheActor(RedisWriteThroughCache, cacheUpdateSensorDocumentMessageChannel)));

            UpdateSensorDataActorRef = ActorSystem.ActorOf(
                Props.Create(
                    () => new UpdateSensorDataActor(updateDatabaseActorRef, updateLocalCacheActorRef, updateDistributedCacheActorRef)));

            PushDataService = new PushDataService(UpdateSensorDataActorRef);
        }

        private static ConnectionMultiplexer CreateRedisConnectionMultiplexer()
        {
            var redis = ConnectionMultiplexer.Connect(RedisConnString);
            redis.PreserveAsyncOrder = false;
            return redis;
        }

        public static ActorSystem ActorSystem { get; private set; }
        public static IActorRef UpdateSensorDataActorRef { get; private set; }

        public static IPushDataService PushDataService { get; private set; }
        public static IGetDataService GetDataService { get; private set; }

        public static IWriteThroughCache RedisWriteThroughCache { get; private set; }
        public static IWriteThroughCache WriteThroughLocalCache { get; private set; }

        public static WebJob PruneLocalCacheJob { get; private set; }

        public static ConnectionMultiplexer Redis { get; private set; }
    }
}