using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.PushData
{
    public interface IPushDataService
    {
        Task<PushDataResult> PushData(string thingName, DateTime currentDateTime, IEnumerable<KeyValuePair<string, string>> keyValuePairs);

        Task<PushDataResult> PushData(string thingName, DateTime currentDateTime, DynamicObjectDictionary content);
    }
}