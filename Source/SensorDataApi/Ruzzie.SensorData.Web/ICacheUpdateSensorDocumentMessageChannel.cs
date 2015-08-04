using System;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web
{
    public interface ICacheUpdateSensorDocumentMessageChannel
    {
        Task Publish(string thingName);
        Task Subscribe(Action<string> callback);
    }
}