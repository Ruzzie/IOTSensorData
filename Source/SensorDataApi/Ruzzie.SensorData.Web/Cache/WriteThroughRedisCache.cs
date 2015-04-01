using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Ruzzie.SensorData.Web.Cache
{
    public class WriteThroughRedisCache : IWriteThroughCache
    {
        private const string LastModifiedFieldName = "lastmodified";
        private const string DocumentFieldName = "document";
        private const string LatestItemKeyFormatString = "sensoritemdatadocument:latest:{0}";
// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ConnectionMultiplexer _redis;
        private IDatabase _redisDatabase;
        private TimeSpan _expireAfterTimeSpan = new TimeSpan(0, 0, 5, 0);


        public WriteThroughRedisCache(ConnectionMultiplexer redis)
        {
            _redis = redis;
            LatestEntryCache = _redis.GetDatabase();
        }

        protected IDatabase LatestEntryCache
        {
            get { return _redisDatabase; }
            private set { _redisDatabase = value; }
        }

        public async Task Update(SensorItemDataDocument dataDocument)
        {
            if (dataDocument == null)
            {
                return;
            }
            //create data for thingname + datetime to check if newer
            //update if newer
            //delete old key
            //else return current            
            string keyname = CreateKeyForLatestEntry(dataDocument);
            RedisValue lastModified = await LatestEntryCache.HashGetAsync(keyname, LastModifiedFieldName);

            if (((long) lastModified) < dataDocument.Created.Ticks)
            {
                await LatestEntryCache.HashSetAsync(
                    keyname,
                    new[]
                    {
                        new HashEntry(LastModifiedFieldName, dataDocument.Created.Ticks),
                        new HashEntry(DocumentFieldName, JsonConvert.SerializeObject(dataDocument))
                    });
                await LatestEntryCache.KeyExpireAsync(keyname, _expireAfterTimeSpan,CommandFlags.FireAndForget);
            }
        }

        private RedisKey CreateKeyForLatestEntry(SensorItemDataDocument dataDocument)
        {
            return CreateKeyForLatestEntry(dataDocument.ThingName);
        }

        private RedisKey CreateKeyForLatestEntry(string thingName)
        {
            return string.Format(LatestItemKeyFormatString, thingName);
        }

        public async Task<SensorItemDataDocument> GetLatest(string thingName)
        {
            return await Deserialize(LatestEntryCache.HashGetAsync(CreateKeyForLatestEntry(thingName), DocumentFieldName));
        }


        private async Task<SensorItemDataDocument> Deserialize(Task<RedisValue> getAsync)
        {
            return await Task.Run(async () =>
            {
                string dataAsString = await getAsync;

                if (string.IsNullOrWhiteSpace(dataAsString))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<SensorItemDataDocument>(dataAsString);
            });
        }

        public Task<int> PruneOldestItemCacheForItemsOlderThan(TimeSpan age)
        {
            throw new NotImplementedException();
        }

        public void ResetLatestEntryCache()
        {
            EndPoint[] endPoints = _redis.GetEndPoints();
            foreach (var endPoint in endPoints)
            {
                IServer server = _redis.GetServer(endPoint);
                server.FlushDatabase();
            }
        }
    }
}