using System;

namespace Ruzzie.SensorData.Web
{
    public interface IUpdateSensorDocumentMessageChannel
    {
        void Publish(string message);
        void Subscribe(Action<string> callBack);
    }
}