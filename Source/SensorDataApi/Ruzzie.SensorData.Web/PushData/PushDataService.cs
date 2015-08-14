using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;

namespace Ruzzie.SensorData.Web.PushData
{
    public class PushDataService : IPushDataService
    {
        protected IActorRef UpdateDataActorRef { get; set; }

        public PushDataService(IActorRef updateDataActorRef)
        {
            UpdateDataActorRef = updateDataActorRef;
        }
        
        public async Task<DataResult> PushData(string thingName, DateTime currentDateTime,
            IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var result = new DataResult {Timestamp = currentDateTime};
            result.ThingName = thingName;

            if (string.IsNullOrWhiteSpace(thingName))
            {
                result.DataResultCode = DataResultCode.FailedThingNameNotProvided;
                return result;
            }

            if (keyValuePairs == null)
            {
                result.DataResultCode = DataResultCode.FailedEmptyData;
                return result;
            }

            List<KeyValuePair<string, string>> keyValuePairsAsList = keyValuePairs.ToList();

            if (keyValuePairsAsList.Count == 0)
            {
                result.DataResultCode = DataResultCode.FailedEmptyData;
                return result;
            }


            dynamic resultObject = MapKeyValuePairsToDynamic(keyValuePairsAsList);            
            result.ResultData = resultObject;

            await Task.Run(()=> UpdateDataActorRef.Tell(new UpdateSensorDataDocumentMessage(thingName,result.Timestamp,result.ResultData)));

            return result;
        }

        public async Task<DataResult> PushData(string thingName, DateTime currentDateTime, DynamicObjectDictionary content)
        {
            var result = new DataResult {Timestamp = currentDateTime, ThingName = thingName};

            if (string.IsNullOrWhiteSpace(thingName))
            {
                result.DataResultCode = DataResultCode.FailedThingNameNotProvided;
                return result;
            }            
            
            try
            {
                if (content == null || content.MemberCount == 0)
                {
                    result.DataResultCode = DataResultCode.FailedEmptyData;
                    return result;
                }

                result.ResultData = content;

                await Task.Run(() => UpdateDataActorRef.Tell(new UpdateSensorDataDocumentMessage(thingName, result.Timestamp, result.ResultData)));
                return result;
            }
            catch (Exception e)
            {
                result.DataResultCode = DataResultCode.UnexpectedError;
                result.ResultData = new DynamicObjectDictionary();
                result.ResultData.ErrorMessage = e.Message;
                return result;
            }
        }

        private static dynamic MapKeyValuePairsToDynamic(List<KeyValuePair<string, string>> keyValuePairs)
        {
            dynamic result = new DynamicObjectDictionary();
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