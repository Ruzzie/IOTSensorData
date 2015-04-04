using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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

            StubUpdateSensorDocumentMessageChannel updateUpdateSensorDocumentMessageChannel = new StubUpdateSensorDocumentMessageChannel();
            string lastMessage = string.Empty;
            updateUpdateSensorDocumentMessageChannel.Subscribe(message => lastMessage = message).Wait();
            IDataWriteService dataWriteService = new DataWriteServiceWithCache(new WriteThroughCacheLocal(), new WriteThroughCacheLocal(), repository, updateUpdateSensorDocumentMessageChannel);

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

    public class StubUpdateSensorDocumentMessageChannel : IUpdateSensorDocumentMessageChannel
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
