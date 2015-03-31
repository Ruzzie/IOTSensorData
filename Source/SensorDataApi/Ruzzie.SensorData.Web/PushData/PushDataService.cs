using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.PushData
{
    public class PushDataService : IPushDataService
    {
        public PushDataService(IDataWriteService dataWriteService)
        {
            DataWriteService = dataWriteService;
        }

        protected IDataWriteService DataWriteService { get; set; }
        
        public async Task<PushDataResult> PushData(string thingName, DateTime currentDateTime,
            IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var result = new PushDataResult {TimeStamp = currentDateTime};
            result.ThingName = thingName;

            if (string.IsNullOrWhiteSpace(thingName))
            {
                result.PushDataResultCode = PushDataResultCode.FailedNoThingNameProvided;
                return result;
            }

            if (keyValuePairs == null)
            {
                result.PushDataResultCode = PushDataResultCode.FailedEmptyData;
                return result;
            }

            List<KeyValuePair<string, string>> keyValuePairsAsList = keyValuePairs.ToList();

            if (keyValuePairsAsList.Count == 0)
            {
                result.PushDataResultCode = PushDataResultCode.FailedEmptyData;
                return result;
            }


            dynamic resultObject = MapKeyValuePairsToDynamic(keyValuePairsAsList);            
            result.ResultData = resultObject;

            await DataWriteService.CreateOrUpdateDataForThing(thingName, result.TimeStamp, result.ResultData);

            return result;
        }

        public async Task<PushDataResult> PushData(string thingName, DateTime currentDateTime, DynamicDictionaryObject content)
        {
            var result = new PushDataResult {TimeStamp = currentDateTime};
            result.ThingName = thingName;

            if (string.IsNullOrWhiteSpace(thingName))
            {
                result.PushDataResultCode = PushDataResultCode.FailedNoThingNameProvided;
                return result;
            }            
            
            try
            {
                if (content == null || !content.GetDynamicMemberNames().Any())
                {
                    result.PushDataResultCode = PushDataResultCode.FailedEmptyData;
                    return result;
                }

                result.ResultData = content;

                await DataWriteService.CreateOrUpdateDataForThing(thingName, result.TimeStamp, content);
                return result;
            }
            catch (Exception e)
            {
                result.PushDataResultCode = PushDataResultCode.UnexpectedError;
                result.ResultData = new DynamicDictionaryObject();
                result.ResultData.ErrorMessage = e.Message;
                return result;
            }
        }

        private dynamic MapKeyValuePairsToDynamic(List<KeyValuePair<string, string>> keyValuePairs)
        {
            dynamic result = new DynamicDictionaryObject();
            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(keyValuePairs[i].Key) || string.IsNullOrWhiteSpace(keyValuePairs[i].Value))
                {
                    break;
                }
                result[keyValuePairs[i].Key] = keyValuePairs[i].Value;
            }
            return result;
        }
    }
}