using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.Cache;

namespace Ruzzie.SensorData.UnitTests
{

    [TestFixture]
    public class LocalWriteThroughCacheTests : WriteThroughCacheTestBase{
        
        private static StubCacheUpdateSensorDocumentMessageChannel _stubCacheUpdateSensorDocumentMessageChannel = new StubCacheUpdateSensorDocumentMessageChannel();

        public LocalWriteThroughCacheTests():base(new WriteThroughCacheLocal(_stubCacheUpdateSensorDocumentMessageChannel))
        {            
            
        }
                
        [Test]
        public void ItemShouldBeExpiredWhenUpdateMessageReceived()
        {
            //Arrange
            
            SensorItemDataDocument document = new SensorItemDataDocument();
            document.ThingName = "ExpireThing";
            document.Created = DateTime.Now;
            document.Content = new DynamicObjectDictionary();
            Cache.Update(document).Wait();

            _stubCacheUpdateSensorDocumentMessageChannel.Publish(document.ThingName).Wait();
            Thread.Sleep(1);

            Assert.That(Cache.GetLatest(document.ThingName).Result,Is.EqualTo(null));
        }

        [Test]
        public void CacheTest()
        {
            DateTime dateTime = DateTime.Now;
            Parallel.For(0, 20000, async i =>
            {
                var document = new SensorItemDataDocument { Content = new DynamicObjectDictionary(), Created = dateTime.Subtract(new TimeSpan(0, 0, 0, 0, i)), ThingName = Guid.NewGuid().ToString() };
                await Cache.Update(document);
            });

            ((WriteThroughCacheLocal) Cache).PruneCache();

            int count = Cache.PruneOldestItemCacheForItemsOlderThan(new TimeSpan(0, 0, 0, 0, 10000)).Result;//about half the items

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
            var document = new SensorItemDataDocument { Content = new DynamicObjectDictionary(), Created = current, ThingName = "SmokeTest2" };

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
