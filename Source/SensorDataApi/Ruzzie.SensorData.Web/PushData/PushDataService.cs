using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruzzie.SensorData.Web.PushData
{
    public interface IPushDataService
    {
        PushDataResult PushData(string thingName, DateTime currentDateTime, List<KeyValuePair<string, string>> keyValuePairs);
        PushDataResult PushData(string thingName, DateTime currentDateTime, DynamicDictionaryObject content);

        Task<PushDataResult> PushDataAsync(string thingName, DateTime currentDateTime,
            IEnumerable<KeyValuePair<string, string>> keyValuePairs);

        Task<PushDataResult> PushDataAsync(string thingName, DateTime currentDateTime, DynamicDictionaryObject content);
    }

    public class PushDataService : IPushDataService
    {
        public PushDataService(IDataWriteService dataWriteService)
        {
            DataWriteService = dataWriteService;
        }

        protected IDataWriteService DataWriteService { get; set; }

        public PushDataResult PushData(string thingName, DateTime currentDateTime, List<KeyValuePair<string, string>> keyValuePairs)
        {
            return PushDataAsync(thingName, currentDateTime, keyValuePairs).Result;
        }

        public PushDataResult PushData(string thingName, DateTime currentDateTime, DynamicDictionaryObject content)
        {
            return PushDataAsync(thingName, currentDateTime, content).Result;
        }

        public async Task<PushDataResult> PushDataAsync(string thingName, DateTime currentDateTime,
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

        public async Task<PushDataResult> PushDataAsync(string thingName, DateTime currentDateTime, DynamicDictionaryObject content)
        {
            var result = new PushDataResult {TimeStamp = currentDateTime};
            result.ThingName = thingName;

            if (string.IsNullOrWhiteSpace(thingName))
            {
                result.PushDataResultCode = PushDataResultCode.FailedNoThingNameProvided;
                return result;
            }            

            //if (string.IsNullOrWhiteSpace(jsonContentString))
            //{
            //    result.PushDataResultCode = PushDataResultCode.FailedEmptyData;
            //    return result;
            //}

            //DynamicDictionaryObject contentObject;
            //try
            //{
            //    contentObject = JsonConvert.DeserializeObject<DynamicDictionaryObject>(jsonContentString);
            //}
            //catch (Exception e)
            //{
            //    result.PushDataResultCode = PushDataResultCode.InvalidData;
            //    result.ResultData = new DynamicDictionaryObject();
            //    result.ResultData.ErrorMessage = e.Message;
            //    return result;
            //}

            if (content == null || !content.GetDynamicMemberNames().Any())
            {
                result.PushDataResultCode = PushDataResultCode.FailedEmptyData;
                return result;
            }

            result.ResultData = content;

            await DataWriteService.CreateOrUpdateDataForThing(thingName, result.TimeStamp, content);
            return result;
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