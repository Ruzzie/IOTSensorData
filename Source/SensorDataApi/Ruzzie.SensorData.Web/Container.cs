using System;
using System.Configuration;
using System.Web;
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
            RedisWriteThroughCache = new WriteThroughRedisCache(Redis);

            TimeSpan localCacheExpiry = new TimeSpan(0,0,5,0);
            PruneLocalCacheJob = new WebJob(localCacheExpiry, () => WriteThroughLocalCache.PruneOldestItemCacheForItemsOlderThan(localCacheExpiry), HttpRuntime.Cache ?? new System.Web.Caching.Cache());

            PushDataService = new PushDataService(new DataWriteServiceWithCache(WriteThroughLocalCache, RedisWriteThroughCache, sensorItemDataRepositoryMongo,cacheUpdateSensorDocumentMessageChannel));

            GetDataService = new GetDataService(new DataReadServiceWithCache(WriteThroughLocalCache, RedisWriteThroughCache, sensorItemDataRepositoryMongo));
        }

        private static ConnectionMultiplexer CreateRedisConnectionMultiplexer()
        {
            var redis = ConnectionMultiplexer.Connect(RedisConnString);
            redis.PreserveAsyncOrder = false;
            return redis;
        }

        public static IPushDataService PushDataService { get; private set; }
        public static IGetDataService GetDataService { get; private set; }

        public static IWriteThroughCache RedisWriteThroughCache { get; private set; }
        public static IWriteThroughCache WriteThroughLocalCache { get; private set; }

        public static WebJob PruneLocalCacheJob { get; private set; }

        public static ConnectionMultiplexer Redis { get; private set; }
    }
}