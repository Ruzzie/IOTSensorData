using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.GetData
{
    public interface IDataReadService
    {
        Task<SensorItemDataDocument> GetLatestEntryForThing(string thingName);
    }
}