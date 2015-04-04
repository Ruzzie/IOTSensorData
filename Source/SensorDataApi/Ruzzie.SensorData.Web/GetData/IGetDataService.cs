using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.GetData
{
    public interface IGetDataService
    {        
        Task<GetDataResult> GetLatestDataEntryForThing(string thingName);
        Task<GetDataResult> GetLatestSingleValueForThing(string thingName, string valueName);
    }
}