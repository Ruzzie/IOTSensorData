using System;
using NUnit.Framework;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.Cache;

namespace Ruzzie.SensorData.UnitTests
{
    public class WriteThroughCacheTestBase    
    {

        public WriteThroughCacheTestBase(IWriteThroughCache cacheUnderTest)
        {
            _cache = cacheUnderTest;
        }

        private readonly IWriteThroughCache _cache = new WriteThroughCacheLocal();

        protected IWriteThroughCache Cache
        {
            get { return _cache; }
        }

        [Test]
        public void GetCachedItemAfterStoring()
        {
            //Arrange
            DateTime current = DateTime.Now;
            var document = new SensorItemDataDocument {Content = new DynamicDictionaryObject(), Created = current,ThingName = "SmokeTest1"};
            Cache.Update(document).Wait();
            
            //Act & Asset
            Assert.That(Cache.GetLatest("SmokeTest1").Result.ThingName,Is.EqualTo("SmokeTest1"));
        }

        [Test]
        public void NoCacheItemFoundShouldReturnNull()
        {
            //Act & Asset
            Assert.That(Cache.GetLatest(Guid.NewGuid().ToString()).Result, Is.Null);
        }

        [Test]
        public async void OnlyLatestItemShouldBeCached()
        {
            //Arrange
            DateTime current = new DateTime(2015,12,1,0,0,0, DateTimeKind.Utc);
            DateTime past = new DateTime(2001,12,25);
            var document = new SensorItemDataDocument { Content = new DynamicDictionaryObject(), Created = current, ThingName = "SmokeTest2" };
            
            await Cache.Update(document);
            var secondDocumentForThing = document.DeepClone();
            secondDocumentForThing.Created = past;

            await Cache.Update(secondDocumentForThing);

            //Act
            var updatedItem = await Cache.GetLatest("SmokeTest2");

            //Assert
            Assert.That(updatedItem.Created, Is.EqualTo(current));
        }

        [Test]
        public void MustNotStoreNullValue()
        {
            //Arrange                        
            var returnedItem = Cache.Update(null);

            //Act & Asset
            Assert.That(returnedItem.Result, Is.EqualTo(null));
        }


        [Test]
        public virtual void PruneCacheItemsOlderThanGivenValue()
        {
            //Arrange
            DateTime current = DateTime.Now;
            var document = new SensorItemDataDocument { Content = new DynamicDictionaryObject(), Created = current, ThingName = "SmokeTest3" };
            Cache.Update(document).Wait();

            int count = Cache.PruneOldestItemCacheForItemsOlderThan(new TimeSpan(1)).Result;//all items

            Assert.That(Cache.GetLatest("SmokeTest3").Result, Is.Null);
            Assert.That(count, Is.GreaterThanOrEqualTo(1));
        }      
    }
}