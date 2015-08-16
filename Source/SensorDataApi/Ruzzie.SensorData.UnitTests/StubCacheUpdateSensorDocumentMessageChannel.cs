using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.Web.Cache;

namespace Ruzzie.SensorData.UnitTests
{
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