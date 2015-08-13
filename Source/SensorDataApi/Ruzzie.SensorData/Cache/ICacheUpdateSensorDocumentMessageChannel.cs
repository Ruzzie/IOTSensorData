using System;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Cache
{
    public interface ICacheUpdateSensorDocumentMessageChannel
    {
        Task Publish(string thingName);
        Task Subscribe(Action<string> callback);
    }
}