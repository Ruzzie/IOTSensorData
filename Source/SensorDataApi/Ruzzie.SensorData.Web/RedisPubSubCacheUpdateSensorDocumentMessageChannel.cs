using System;
using System.Globalization;
using System.Threading.Tasks;
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
            string message =  UpdateSensorDocumentMessage.CreateMessage(SenderId,thingName);

            await _subscriber.PublishAsync(MessageChannelNames.UpdateLatestThingNotifications, message, CommandFlags.FireAndForget);
        }

        public async Task Subscribe(Action<string> callback)
        {
            await _subscriber.SubscribeAsync(MessageChannelNames.UpdateLatestThingNotifications, (channel, value) =>
            {
                UpdateSensorDocumentMessage message = UpdateSensorDocumentMessage.FromString(value);
                if (message.SenderId != SenderId)
                {
                    callback.Invoke(message.ThingName);
                }
            });
        }                
    }

    public class UpdateSensorDocumentMessage
    {
        private const string Format = "{0};;;{1}";
        public string ThingName { get; set; }
        public string SenderId { get; set; }

        public UpdateSensorDocumentMessage(string senderId, string thingName)
        {
            SenderId = senderId;
            ThingName = thingName;
        }

        public static UpdateSensorDocumentMessage FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Null or empty","value");
            }

            string[] strings = value.Split(new[] {";;;"}, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length != 2)
            {
                throw new ArgumentException("Incorrect format.","value");
            }

            return new UpdateSensorDocumentMessage(strings[0],strings[1]);
        }

        public static string CreateMessage(string senderId, string thingName)
        {
            return string.Format(CultureInfo.InvariantCulture, Format, senderId, thingName);
        }

        public override string ToString()
        {
            return CreateMessage(SenderId, ThingName);
        }
    }
    
}