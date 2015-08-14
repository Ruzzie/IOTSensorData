using System;
using System.Globalization;
using System.Threading.Tasks;
using Ruzzie.SensorData.Cache;
using Ruzzie.SensorData.Web.Cache;
using StackExchange.Redis;

namespace Ruzzie.SensorData.Web
{
    public class RedisPubSubCacheUpdateSensorDocumentMessageChannel : ICacheUpdateSensorDocumentMessageChannel
    {
        private readonly ISubscriber _subscriber;

        public RedisPubSubCacheUpdateSensorDocumentMessageChannel(ConnectionMultiplexer redis)
        {
            if (redis == null)
            {
                throw new ArgumentNullException("redis");
            }

            _subscriber = redis.GetSubscriber();
            SenderId = Guid.NewGuid().ToString();
        }

        public string SenderId { get; protected set; }

        public async Task Publish(string thingName)
        {
            string message =  UpdateCacheSensorDocumentMessage.CreateMessage(SenderId,thingName);

            await _subscriber.PublishAsync(MessageChannelNames.UpdateLatestThingNotifications, message, CommandFlags.FireAndForget);
        }

        public async Task Subscribe(Action<string> callback)
        {
            await _subscriber.SubscribeAsync(MessageChannelNames.UpdateLatestThingNotifications, (channel, value) =>
            {
                UpdateCacheSensorDocumentMessage message = UpdateCacheSensorDocumentMessage.FromString(value);
                if (message.SenderId != SenderId)
                {
                    callback.Invoke(message.ThingName);
                }
            });
        }                
    }

    public class UpdateCacheSensorDocumentMessage
    {
        private const string MessageFormat = "{0};;;{1}";
        public string ThingName { get; set; }
        public string SenderId { get; set; }

        public UpdateCacheSensorDocumentMessage(string senderId, string thingName)
        {
            SenderId = senderId;
            ThingName = thingName;
        }

        public static UpdateCacheSensorDocumentMessage FromString(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Null or empty","message");
            }

            string[] messageParts = message.Split(new[] {";;;"}, StringSplitOptions.RemoveEmptyEntries);
            if (messageParts.Length != 2)
            {
                throw new ArgumentException("Incorrect format.","message");
            }

            return new UpdateCacheSensorDocumentMessage(messageParts[0],messageParts[1]);
        }

        public static string CreateMessage(string senderId, string thingName)
        {
            return string.Format(CultureInfo.InvariantCulture, MessageFormat, senderId, thingName);
        }

        public override string ToString()
        {
            return CreateMessage(SenderId, ThingName);
        }
    }
    
}