using System.Threading;
using NUnit.Framework;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class RedisPubSubMessageChannelTests
    {
        [Test]
        public void SmokeTest()
        {
            ICacheUpdateSensorDocumentMessageChannel channelOne = new RedisPubSubCacheUpdateSensorDocumentMessageChannel(Container.Redis);
            ICacheUpdateSensorDocumentMessageChannel channelTwo = new RedisPubSubCacheUpdateSensorDocumentMessageChannel(Container.Redis);
            string latestMessage = string.Empty;
            
            channelTwo.Subscribe(s => latestMessage =s);
            channelOne.Publish("SmokeTest1");
            Thread.Sleep(100);

            Assert.That(latestMessage, Is.EqualTo("SmokeTest1"));
        }
    }
}
