using System.Threading.Tasks;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.Web.GetData
{
    public class GetDataService : IGetDataService
    {
        public GetDataService(IDataReadService dataReadService)
        {
            DataReadService = dataReadService;
        }

        protected IDataReadService DataReadService { get; set; }

        public GetDataResult GetLastestDataEntryForThing(string thingName)
        {
            return GetLastestDataEntryForThingAsync(thingName).Result;
        }

        public async Task<GetDataResult> GetLastestDataEntryForThingAsync(string thingName)
        {
            var result = new GetDataResult {ThingName = thingName};
            if (string.IsNullOrWhiteSpace(thingName))
            {
                return new GetDataResult {GetDataResultCode = GetDataResultCode.FailedNoThingNameProvided};
            }

            SensorItemDataDocument dataDocument = await DataReadService.GetLatestEntryForThing(thingName);

            if (dataDocument == null)
            {
                result.GetDataResultCode = GetDataResultCode.FailedThingNotFound;
                return result;
            }

            result.GetDataResultCode = GetDataResultCode.Success;
            result.ResultData = dataDocument.Content;
            result.Timestamp = dataDocument.Created;            
            return result;
        }
    }
}