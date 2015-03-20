using System;
using System.Net;
using System.Threading.Tasks;
using Jil;
using StackExchange.Redis;

namespace Ruzzie.SensorData.Web.Cache
{
    public class WriteThroughRedisCache : IWriteThroughCache
    {
// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ConnectionMultiplexer _redis;
        private IDatabase _redisDatabase;
        readonly Options _options = new Options(dateFormat:DateTimeFormat.ISO8601);
        TimeSpan _expireAfterTimeSpan = new TimeSpan(1,0,0,0);


        public WriteThroughRedisCache(string connString)
        {
            _redis = ConnectionMultiplexer.Connect(connString);            
            _redis.PreserveAsyncOrder = false;            
            LatestEntryCache = _redis.GetDatabase();
        }

        protected IDatabase LatestEntryCache
        {
            get { return _redisDatabase; }
            private set { _redisDatabase = value; }
        }

        public async Task<SensorItemDataDocument> Update(SensorItemDataDocument dataDocument)
        {
            if (dataDocument == null)
            {
                return await Task.FromResult<SensorItemDataDocument>(null);
            }
            //create byte[] for thingname + datetime to check if newer
            //update if newer
                //delete old key
            //else return current            
            string keyname = CreateKeyForLatestEntry(dataDocument);
            RedisValue lastModified = await LatestEntryCache.HashGetAsync(keyname, "lastmodified");
            
            if (((long) lastModified) < dataDocument.Created.Ticks)
            {                
                await LatestEntryCache.HashSetAsync(keyname,
                    new[]
                    {
                        new HashEntry("lastmodified", dataDocument.Created.Ticks),
                        new HashEntry("document", JSON.SerializeDynamic(dataDocument, _options))
                    });
                await LatestEntryCache.KeyExpireAsync(keyname, _expireAfterTimeSpan);
            }
            return await Task.FromResult(dataDocument);
        }

        private RedisKey CreateKeyForLatestEntry(SensorItemDataDocument dataDocument)
        {
            return CreateKeyForLatestEntry(dataDocument.ThingName);
        }
        private RedisKey CreateKeyForLatestEntry(string thingName)
        {
            return string.Format("sensoritemdatadocument:latest:{0}", thingName);
        }

        public async Task<SensorItemDataDocument> GetLatest(string thingName)
        {
            return await Deserialize(LatestEntryCache.HashGetAsync(CreateKeyForLatestEntry(thingName),"document"));
        }
       

        private async Task<SensorItemDataDocument> Deserialize(Task<RedisValue> stringGetAsync)
        {
            return await Task.Run(async () =>
            {
                string jsonString = await stringGetAsync;
                if (!string.IsNullOrWhiteSpace(jsonString))
                {                    
                    return JSON.Deserialize<SensorItemDataDocument>(jsonString,_options);
                }
                return null;
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