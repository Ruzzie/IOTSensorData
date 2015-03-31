using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.GetData
{
    public interface IGetDataService
    {        
        Task<GetDataResult> GetLastestDataEntryForThing(string thingName);
        Task<GetDataResult> GetLastestSingleValueForThing(string thingName, string valueName);
    }
}