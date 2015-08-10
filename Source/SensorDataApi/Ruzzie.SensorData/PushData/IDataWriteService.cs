using System;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.PushData
{
    public interface IDataWriteService
    {
        Task CreateOrUpdateDataForThing(string thingName, DateTime timestamp, dynamic data);
    }
}