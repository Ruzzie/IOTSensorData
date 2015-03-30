using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                result.GetDataResultCode = GetDataResultCode.FailedNoThingNameProvided;
                return result;                
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

        public async Task<GetDataResult> GetLastestSingleValueForThing(string thingName, string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName))
            {
                return new GetDataResult {GetDataResultCode = GetDataResultCode.ValueNameNotProvided};
            }

            var result = await GetLastestDataEntryForThingAsync(thingName);
            if (result.GetDataResultCode != GetDataResultCode.Success)
            {
                return result;
            }

            if (! ((IDictionary<string,dynamic>) result.ResultData).ContainsKey(valueName))
            {
                return new GetDataResult {GetDataResultCode = GetDataResultCode.ValueNameNotFound};
            }

            result.ResultData = result.ResultData[valueName];
            return result;
        }
    }
}