using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.GetData
{
    public interface IGetDataService
    {
        GetDataResult GetLastestDataEntryForThing(string thingName);
        Task<GetDataResult> GetLastestDataEntryForThingAsync(string thingName);
    }
}