using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Ruzzie.SensorData.Web.GetData;

namespace Ruzzie.SensorData.Web.Controllers
{
    public class GetDataController : ApiController
    {
        //todo:add ioc
        private static readonly IGetDataService GetDataService = Container.GetDataService;

        /// <summary>
        ///     Reads the latest data entry for a thing.
        /// </summary>
        /// <param name="thing">A unique name of a thing.</param>
        /// <returns><see cref="DataResult"/></returns>
        [Route("get/latest/data/for/{thing}")]        
        public async Task<DataResult> GetLatest(string thing)
        {
            return await GetDataService.GetLatestDataEntryForThing(thing);
        }


        /// <summary>
        ///     Reads the single value for the latest entry for a thing.
        /// </summary>
        /// <remarks> Use this if you do not want or cannot parse json. This returns plain text.</remarks>
        /// <param name="thing">A unique name of a thing.</param>
        /// <param name="valueName">The name of the value stored in the content data to return.</param>
        /// <response code="404">Thing or value was not found or no thing name provided.</response>        
        /// <returns><see cref="DataResult"/></returns>
        [Route("get/latest/singlevalue/for/{thing}/{valueName}")]
        public async Task<HttpResponseMessage> GetLatestSingleValue(string thing,string valueName)
        {            
            return await Task.Run(async () =>
            {                
                var response = new HttpResponseMessage(HttpStatusCode.OK);

                DataResult dataResult = await GetDataService.GetLatestSingleValueForThing(thing,valueName);
                switch (dataResult.DataResultCode)
                {
                    case DataResultCode.Success:                        
                        response.Content = new StringContent(dataResult.ResultData.ToString(), Encoding.UTF8); 
                        response.StatusCode = HttpStatusCode.OK;
                        break;
                    case DataResultCode.FailedThingNameNotProvided:                                                
                    case DataResultCode.FailedThingNotFound:                        
                    case DataResultCode.ValueNameNotFound:
                    case DataResultCode.ValueNameNotProvided:
                        response.StatusCode = HttpStatusCode.NotFound;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return response;
            });
        }       

    }
}