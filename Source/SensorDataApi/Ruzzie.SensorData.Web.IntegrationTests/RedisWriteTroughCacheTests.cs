using NUnit.Framework;
using Ruzzie.SensorData.UnitTests;
using Ruzzie.SensorData.Web.Cache;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class RedisWriteTroughCacheTests : WriteThroughCacheTestBase
    {
        public RedisWriteTroughCacheTests() : base(Container.RedisWriteThroughCache)
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
