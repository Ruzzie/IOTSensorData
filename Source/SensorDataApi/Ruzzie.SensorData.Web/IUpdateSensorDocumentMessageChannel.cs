using System;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web
{
    public interface IUpdateSensorDocumentMessageChannel
    {
        Task Publish(string thingName);
        Task Subscribe(Action<string> callback);
    }
}