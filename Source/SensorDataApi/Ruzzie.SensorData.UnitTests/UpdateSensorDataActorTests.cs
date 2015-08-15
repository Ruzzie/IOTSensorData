using System;
using System.Threading;
using Akka.Actor;
using Akka.TestKit.NUnit;
using Moq;
using NUnit.Framework;
using Ruzzie.SensorData.Repository;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class UpdateSensorDataActorTests : TestKit
    {
        [Test]
        public void UpdateNotificationShouldBeSentToUpdateChannel()
        {
            //Arrange
            ISensorItemDataRepository repository = new Mock<ISensorItemDataRepository>().Object;

            StubCacheUpdateSensorDocumentMessageChannel cacheUpdateCacheUpdateSensorDocumentMessageChannel = new StubCacheUpdateSensorDocumentMessageChannel();
            string lastMessage = string.Empty;
            cacheUpdateCacheUpdateSensorDocumentMessageChannel.Subscribe(message => lastMessage = message).Wait();
          

            var updateDatabaseActorRef = Sys.ActorOf(Props.Create(() => new UpdateDatabaseActor(repository)));
            var updateLocalCacheActorRef = Sys.ActorOf(Props.Create(() => new UpdateLocalCacheActor(new WriteThroughCacheLocal())));
            var updateDistributedCacheActorRef = Sys.ActorOf(
                Props.Create(() => new UpdateDistributedCacheActor(new WriteThroughCacheLocal(), cacheUpdateCacheUpdateSensorDocumentMessageChannel)));
            
            var actor = ActorOfAsTestActorRef<UpdateSensorDataActor>(Props.Create(
                    () => new UpdateSensorDataActor(updateDatabaseActorRef, updateLocalCacheActorRef, updateDistributedCacheActorRef)));

            string thingName = Guid.NewGuid().ToString();
            DateTime timeStamp = new DateTime(2015, 3, 31, 1, 0, 0, 0);
            dynamic data = new DynamicObjectDictionary();
            data.Distance = 23.0;

            //Act
            actor.Tell(new UpdateSensorDataDocumentMessage(thingName,timeStamp,data));

            Thread.Sleep(10);

            //Assert
            Assert.That(lastMessage, Is.EqualTo(thingName));
        }
        
    }
}
