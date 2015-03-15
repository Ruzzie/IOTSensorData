using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class WriteThroughLocalCacheTests
    {
        WriteThroughCacheLocal _cache = new WriteThroughCacheLocal();

        //ordering in tests is probably required, concurrency ...

        [Test]
        public void GetCachedItemAfterStoring()
        {
            //Arrange
            DateTime current = DateTime.Now;
            var document = new SensorItemDataDocument {Content = new DynamicDictionaryObject(), Created = current,ThingName = "SmokeTest1"};
            _cache.Update(document).Wait();
            
            //Act & Asset
            Assert.That(_cache.GetLatest("SmokeTest1").ThingName,Is.EqualTo("SmokeTest1"));
        }

        [Test]
        public void NoCacheItemFoundShouldReturnNull()
        {
            //Act & Asset
            Assert.That(_cache.GetLatest(Guid.NewGuid().ToString()), Is.Null);
        }

        [Test]
        public void OnlyLatestItemShouldBeCached()
        {
            //Arrange
            DateTime current = new DateTime(2015,12,1);
            DateTime past = new DateTime(2001,12,25);
            var document = new SensorItemDataDocument { Content = new DynamicDictionaryObject(), Created = current, ThingName = "SmokeTest2" };
            
            _cache.Update(document);
            var secondDocumentForThing = document.DeepClone();
            secondDocumentForThing.Created = past;

            _cache.Update(secondDocumentForThing);

            //Act & Asset
            Assert.That(_cache.GetLatest("SmokeTest2").Created, Is.EqualTo(current));
        }


        [Test]
        public void PruneCacheItemsOlderThanGivenValue()
        {
            //Arrange
            DateTime current = DateTime.Now;
            var document = new SensorItemDataDocument { Content = new DynamicDictionaryObject(), Created = current, ThingName = "SmokeTest3" };
            _cache.Update(document);

            int count = _cache.PruneOldestItemCacheForItemsOlderThan(new TimeSpan(1)).Result;//all items

            Assert.That(_cache.GetLatest("SmokeTest3"), Is.Null);
            Assert.That(count, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void CacheTest()
        {            
            DateTime dateTime = DateTime.Now;
            Parallel.For(0, 20000, i =>
            {                
                var document = new SensorItemDataDocument { Content = new DynamicDictionaryObject(), Created = dateTime.Subtract(new TimeSpan(0,0,0,0,i)), ThingName = Guid.NewGuid().ToString() };                
                _cache.Update(document);                
            });

            _cache.PruneCache();

            int count = _cache.PruneOldestItemCacheForItemsOlderThan(new TimeSpan(0,0,0,0,10000)).Result;//about half the items

            Assert.That(count, Is.LessThanOrEqualTo(12000));
        }
    }
    

    [TestFixture]
    public class CloneWithSerializationTests
    {
        [Test]
        public void SmokeTest()
        {
            //Arrange
            DateTime current = new DateTime(2015, 12, 1);
            var document = new SensorItemDataDocument { Content = new DynamicDictionaryObject(), Created = current, ThingName = "SmokeTest2" };

            //Act
            var clonedDocument = document.DeepClone();

            //TODO: Caution change when equality operations change for SensorItemDataDocument
            Assert.That(document, Is.Not.EqualTo(clonedDocument));

        }
    }

    public static class CloneExtensions
    {        
        public static T DeepClone<T>(this T objectToClone)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(objectToClone));            
        }
    }

}
