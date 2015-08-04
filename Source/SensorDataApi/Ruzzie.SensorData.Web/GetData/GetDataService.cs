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

        public async Task<DataResult> GetLatestDataEntryForThing(string thingName)
        {
            var result = new DataResult {ThingName = thingName};
            if (string.IsNullOrWhiteSpace(thingName))
            {
                result.DataResultCode = DataResultCode.FailedThingNameNotProvided;
                return result;                
            }

            SensorItemDataDocument dataDocument = await DataReadService.GetLatestEntryForThing(thingName);

            if (dataDocument == null)
            {
                result.DataResultCode = DataResultCode.FailedThingNotFound;
                return result;
            }

            result.DataResultCode = DataResultCode.Success;
            result.ResultData = dataDocument.Content;
            result.Timestamp = dataDocument.Created;            
            return result;
        }

        public async Task<DataResult> GetLatestSingleValueForThing(string thingName, string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName))
            {
                return new DataResult {DataResultCode = DataResultCode.ValueNameNotProvided};
            }

            var result = await GetLatestDataEntryForThing(thingName);
            if (result.DataResultCode != DataResultCode.Success)
            {
                return result;
            }

            if (! ((IDictionary<string,dynamic>) result.ResultData).ContainsKey(valueName))
            {
                return new DataResult {DataResultCode = DataResultCode.ValueNameNotFound};
            }

            result.ResultData = result.ResultData[valueName];
            return result;
        }
    }
}