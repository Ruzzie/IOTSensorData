using System.Threading.Tasks;

namespace Ruzzie.SensorData.Repository
{
    public interface ISensorItemDataRepository
    {
        Task CreateOrAdd(SensorItemDataDocument sensorItemData);
        Task<SensorItemDataDocument> GetLatest(string thingName);
    }
}