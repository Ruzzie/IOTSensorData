using System;
using System.Collections.Generic;
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
            updateUpdateSensorDocumentMessageChannel.Subscribe(message => lastMessage = message);
            IDataWriteService dataWriteService = new DataWriteServiceWithCache(new WriteThroughCacheLocal(), new WriteThroughCacheLocal(), repository, updateUpdateSensorDocumentMessageChannel);

            string thingName = Guid.NewGuid().ToString();
            DateTime timeStamp = new DateTime(2015,3,31,1,0,0,0);
            dynamic data = new DynamicDictionaryObject();
            data.Distance = 23.0;

            //Act
            dataWriteService.CreateOrUpdateDataForThing(thingName, timeStamp, data);

            //Assert
            Assert.That(lastMessage,Is.EqualTo(thingName));
        }
    }

    public class StubUpdateSensorDocumentMessageChannel : IUpdateSensorDocumentMessageChannel
    {
        //only supports one subscription per channel for test purposes
        Dictionary<string,Action<string>> _subscriptions  = new Dictionary<string, Action<string>>();

        public void Subscribe(Action<string> callBack)
        {
            _subscriptions[MessageChannelNames.UpdateLatestThingNotifications] = callBack;
        }

        public void Publish(string message)
        {
            _subscriptions[MessageChannelNames.UpdateLatestThingNotifications].Invoke(message);
        }
    }
}
