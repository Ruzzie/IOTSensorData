﻿using System;
using System.Threading.Tasks;
using Ruzzie.SensorData.Web.Cache;
using StackExchange.Redis;

namespace Ruzzie.SensorData.Web
{
    public class RedisPubSubUpdateSensorDocumentMessageChannel : IUpdateSensorDocumentMessageChannel
    {
        private readonly ISubscriber _subscriber;

        public RedisPubSubUpdateSensorDocumentMessageChannel(ConnectionMultiplexer redis)
        {
            _subscriber = redis.GetSubscriber();
            SenderId = Guid.NewGuid().ToString();
        }

        public string SenderId { get; protected set; }

        public async Task Publish(string thingName)
        {
            string message =  new UpdateSensorDocumentMessage(SenderId,thingName).ToString();

            await _subscriber.PublishAsync(MessageChannelNames.UpdateLatestThingNotifications, message, CommandFlags.FireAndForget);
        }

        public async Task Subscribe(Action<string> callBack)
        {
            await _subscriber.SubscribeAsync(MessageChannelNames.UpdateLatestThingNotifications, (channel, value) =>
            {
                UpdateSensorDocumentMessage message = UpdateSensorDocumentMessage.FromString(value);
                if (message.SenderId != SenderId)
                {
                    callBack.Invoke(message.ThingName);
                }
            });
        }                
    }

    public class UpdateSensorDocumentMessage
    {
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

        public override string ToString()
        {
            return string.Format("{0};;;{1}", SenderId, ThingName);
        }
    }
    
}