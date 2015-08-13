using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.PushData;
using Ruzzie.SensorData.Repository;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.PushData;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class DataWriteServiceTests
    {
        [Test]
        public void UpdateNotificationShouldBeSent()
        {
            //Arrange
            ISensorItemDataRepository repository = new Moq.Mock<ISensorItemDataRepository>().Object;

            StubCacheUpdateSensorDocumentMessageChannel cacheUpdateCacheUpdateSensorDocumentMessageChannel = new StubCacheUpdateSensorDocumentMessageChannel();
            string lastMessage = string.Empty;
            cacheUpdateCacheUpdateSensorDocumentMessageChannel.Subscribe(message => lastMessage = message).Wait();
            IDataWriteService dataWriteService = new DataWriteServiceWithCache(new WriteThroughCacheLocal(), new WriteThroughCacheLocal(), repository, cacheUpdateCacheUpdateSensorDocumentMessageChannel);

            string thingName = Guid.NewGuid().ToString();
            DateTime timeStamp = new DateTime(2015,3,31,1,0,0,0);
            dynamic data = new DynamicObjectDictionary();
            data.Distance = 23.0;

            //Act
            dataWriteService.CreateOrUpdateDataForThing(thingName, timeStamp, data).Wait();

            Thread.Sleep(1);

            //Assert
            Assert.That(lastMessage,Is.EqualTo(thingName));
        }
    }

    public class StubCacheUpdateSensorDocumentMessageChannel : ICacheUpdateSensorDocumentMessageChannel
    {
        //only supports one subscription per channel for test purposes
        Dictionary<string,Action<string>> _subscriptions  = new Dictionary<string, Action<string>>();

        public async Task Subscribe(Action<string> callback)
        {
            await Task.Run(() =>
                _subscriptions[MessageChannelNames.UpdateLatestThingNotifications] = callback);

        }

        public async Task Publish(string thingName)
        {
            await Task.Run(()=>_subscriptions[MessageChannelNames.UpdateLatestThingNotifications].Invoke(thingName));
        }
    }
}
