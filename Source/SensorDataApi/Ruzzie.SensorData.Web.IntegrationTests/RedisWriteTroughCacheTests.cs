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

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void CacheItemExpiryAfterInSecondsThrowsExceptionWhenLessOrEqualToZero(int seconds)
        {
            Assert.That(() => new WriteThroughRedisCache(Container.Redis, seconds), Throws.ArgumentException);
        }

        [Ignore]
        public override void PruneCacheItemsOlderThanGivenValue()
        {
            base.PruneCacheItemsOlderThanGivenValue();
        }
    }
}
