using NUnit.Framework;
using Ruzzie.SensorData.UnitTests;
using Ruzzie.SensorData.Web.Cache;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class RedisWriteTroughCacheTests : WriteThroughCacheTestBase
    {
        public RedisWriteTroughCacheTests() : base(new WriteThroughRedisCache(Container.RedisConnString))
        {
        }


        [TestFixtureSetUp]
        public void ClearAllItems()
        {            
            Cache.ResetLatestEntryCache();
        }

        [Ignore]
        public override void PruneCacheItemsOlderThanGivenValue()
        {
            
        }
    }
}
