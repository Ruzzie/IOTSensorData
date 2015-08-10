using System.Threading.Tasks;

namespace Ruzzie.SensorData.GetData
{
    public interface IDataReadService
    {
        Task<SensorItemDataDocument> GetLatestEntryForThing(string thingName);
    }
}