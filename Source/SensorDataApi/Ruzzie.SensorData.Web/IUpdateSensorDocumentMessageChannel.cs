using System;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web
{
    public interface IUpdateSensorDocumentMessageChannel
    {
        Task Publish(string message);
        Task Subscribe(Action<string> callBack);
    }
}