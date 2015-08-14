using System;
using System.Threading;
using Akka.Actor;
using MongoDB.Driver;
using NUnit.Framework;
using Ruzzie.SensorData.Web.PushData;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class ActorSystemTests
    {
        private SensorItemDataRepositoryMongo _repository;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _repository = new SensorItemDataRepositoryMongo(MongoDataRepositoryTests.ConnString);
        }

        [Test]
        public void DatabaseShouldBeUpdatedAfterUpdateMessage()
        {
            //Arrange
            IActorRef updateSensorDataActorRef = Container.UpdateSensorDataActorRef;
          
            dynamic dataContent = new DynamicObjectDictionary();
            dataContent.Temperature = "25";
            dataContent.RawValues = new[] { 1, 2, 3, 4, 5, 6 };
            var content = dataContent;
            var created = DateTime.UtcNow;
            var thingName = Guid.NewGuid().ToString();
            
            //Act
            updateSensorDataActorRef.Tell(new UpdateSensorDataDocumentMessage(thingName,created,content));

            Thread.Sleep(200);

            //Assert
            long count = _repository.SensorItemDataCollection.CountAsync(doc => doc.ThingName == thingName).Result;

            Assert.That(count, Is.EqualTo(1));
            dynamic resultContent = _repository.GetLatest(thingName).Result.Content;
            Assert.That(resultContent.Temperature, Is.EqualTo("25"));
            Assert.That(resultContent.RawValues[0], Is.EqualTo(1));
        }


        [Test]
        public void LocalCacheShouldBeUpdatedAfterUpdateMessage()
        {
            //Arrange
            IActorRef updateSensorDataActorRef = Container.UpdateSensorDataActorRef;

            dynamic dataContent = new DynamicObjectDictionary();
            dataContent.Temperature = "26";
            dataContent.RawValues = new[] { 1, 2, 3, 4, 5, 6 };
            var content = dataContent;
            var created = DateTime.UtcNow;
            var thingName = Guid.NewGuid().ToString();

            //Act
            updateSensorDataActorRef.Tell(new UpdateSensorDataDocumentMessage(thingName, created, content));

            Thread.Sleep(5);

            //Assert
            var latest = Container.WriteThroughLocalCache.GetLatest(thingName).Result;
          
            dynamic resultContent = latest.Content;
            Assert.That(resultContent.Temperature, Is.EqualTo("26"));
            Assert.That(resultContent.RawValues[0], Is.EqualTo(1));
            Assert.That(latest.Created, Is.EqualTo(created));
        }


        [Test]
        public void DistributedCacheShouldBeUpdatedAfterUpdateMessage()
        {
            //Arrange
            IActorRef updateSensorDataActorRef = Container.UpdateSensorDataActorRef;

            var dynamicObjectDictionary = new DynamicObjectDictionary();    
            dynamic dataContent = dynamicObjectDictionary;
            dataContent.Temperature = "28";
            dataContent.RawValues = new[] { 1, 2, 3, 4, 5, 6 };
            var content = dataContent;
            var created = DateTime.UtcNow;
            var thingName = Guid.NewGuid().ToString();

            //Act
            updateSensorDataActorRef.Tell(new UpdateSensorDataDocumentMessage(thingName, created, content));

            Thread.Sleep(100);

            //Assert
            var latest = Container.RedisWriteThroughCache.GetLatest(thingName).Result;

            dynamic resultContent =  latest.Content;
            Assert.That(resultContent.Temperature, Is.EqualTo("28"));
            Assert.That(resultContent.RawValues[0].Value, Is.EqualTo(1));
            Assert.That(latest.Created, Is.EqualTo(created));
        }
    }
}
