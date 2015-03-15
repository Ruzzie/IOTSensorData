using System.Threading.Tasks;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.Web.GetData
{
    public interface IDataReadService
    {
        Task<SensorItemDataDocument> GetLatestEntryForThing(string thingName);
    }
}