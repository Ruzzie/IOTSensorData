using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.Cache;

namespace Ruzzie.SensorData.UnitTests
{
    
    public abstract class WriteThroughCacheTestBase    
    {
        public WriteThroughCacheTestBase(IWriteThroughCache cacheUnderTest)
        {
            _cache = cacheUnderTest;
        }

        private readonly IWriteThroughCache _cache;

        protected IWriteThroughCache Cache
        {
            get { return _cache; }
        }

        [Test]
        public void GetCachedItemAfterStoring()
        {
            //Arrange
            DateTime current = DateTime.UtcNow;
            dynamic content = new DynamicObjectDictionary();
            content.Test = "TT";
            var document = new SensorItemDataDocument {Content = content, Created = current,ThingName = "SmokeTest1"};
            Cache.Update(document).Wait();
            
            //Act
            SensorItemDataDocument sensorItemDataDocument = Cache.GetLatest("SmokeTest1").Result;

            //Assert
            Assert.That(sensorItemDataDocument.ThingName,Is.EqualTo("SmokeTest1"));
            Assert.That(((dynamic)sensorItemDataDocument.Content).Test, Is.EqualTo("TT"));
        }

        [Test]
        public void GetCachedItemAfterStoringWithComplexObject()
        {
            //Arrange
            DateTime current = DateTime.UtcNow;
            dynamic content = new DynamicObjectDictionary();
            content.Test = "TT";
            content.SubInts = new[] {1, 2, 3};
            content.SubItems = new Dictionary<string, int> {{"a", 1}, {"b", 2}};
            var document = new SensorItemDataDocument { Content = content, Created = current, ThingName = "SmokeTest1" };
            Cache.Update(document).Wait();

            //Act
            SensorItemDataDocument sensorItemDataDocument = Cache.GetLatest("SmokeTest1").Result;
            dynamic cachedContent = sensorItemDataDocument.Content;
            //Assert
            Assert.That(sensorItemDataDocument.ThingName, Is.EqualTo("SmokeTest1"));
            Assert.That(cachedContent.SubInts[2].ToString(),Is.EqualTo("3"));
            Assert.That(cachedContent.SubItems["b"].ToString(),Is.EqualTo("2"));
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
            dynamic content = new DynamicObjectDictionary();
            content.Test = "TT";
            var document = new SensorItemDataDocument { Content = content, Created = current, ThingName = "SmokeTest2" };
            
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
            Cache.Update(null);                        
        }


        [Test]
        public virtual void PruneCacheItemsOlderThanGivenValue()
        {
            //Arrange
            DateTime current = DateTime.UtcNow;
            var document = new SensorItemDataDocument { Content = new DynamicObjectDictionary(), Created = current, ThingName = "SmokeTest3" };
            Cache.Update(document).Wait();

            int count = Cache.PruneOldestItemCacheForItemsOlderThan(new TimeSpan(1)).Result;//all items

            Assert.That(Cache.GetLatest("SmokeTest3").Result, Is.Null);
            Assert.That(count, Is.GreaterThanOrEqualTo(1));
        }      
    }
}