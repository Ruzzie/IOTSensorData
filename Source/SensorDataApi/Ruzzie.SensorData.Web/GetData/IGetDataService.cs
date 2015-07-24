using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.GetData
{
    public interface IGetDataService
    {        
        Task<DataResult> GetLatestDataEntryForThing(string thingName);
        Task<DataResult> GetLatestSingleValueForThing(string thingName, string valueName);
    }
}