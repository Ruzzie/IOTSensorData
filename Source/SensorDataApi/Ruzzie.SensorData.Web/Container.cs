using System;
using System.Configuration;
using System.Web;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.GetData;
using Ruzzie.SensorData.Web.PushData;
using Ruzzie.SensorData.Web.Repository;

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
            ISensorItemDataRepository sensorItemDataRepositoryMongo = new SensorItemDataRepositoryMongo(MongoConnString);
            var writeThroughCacheLocal = new WriteThroughCacheLocal();
            var writeThroughCacheRedis = new WriteThroughRedisCache(RedisConnString);

            TimeSpan localCacheExpiry = new TimeSpan(0,0,5,0);
            PruneLocalCacheJob = new WebJob(localCacheExpiry, ()=> writeThroughCacheLocal.PruneOldestItemCacheForItemsOlderThan(localCacheExpiry), HttpRuntime.Cache ?? new System.Web.Caching.Cache());

            PushDataService = new PushDataService(new DataWriteServiceWithCache(writeThroughCacheLocal, writeThroughCacheRedis, sensorItemDataRepositoryMongo));
            
            GetDataService = new GetDataService(new DataReadServiceWithCache(writeThroughCacheLocal,writeThroughCacheRedis, sensorItemDataRepositoryMongo));
        }

        public static IPushDataService PushDataService { get; private set; }
        public static IGetDataService GetDataService { get; private set; }

        public static WebJob PruneLocalCacheJob { get; private set; }
    }
}