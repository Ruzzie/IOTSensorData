using System;
using System.Net;
using System.Net.Http;
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
        /// <returns><see cref="GetDataResult"/></returns>
        [Route("get/latest/data/for/{thing}")]
        public async Task<GetDataResult> GetLatest(string thing)
        {
            return await GetDataService.GetLastestDataEntryForThing(thing);
        }

        /// <summary>
        ///     Reads the single value for the latest entry for a thing.
        /// </summary>
        /// <remarks> Use this if you do not want or cannot parse json. This returns plain text.</remarks>
        /// <param name="thing">A unique name of a thing.</param>
        /// <param name="valueName">The name of the value stored in the content data to return.</param>
        /// <response code="404">Thing or value was not found or no thing name provided.</response>        
        /// <returns><see cref="GetDataResult"/></returns>
        [Route("get/latest/singlevalue/for/{thing}/{valueName}")]
        public async Task<HttpResponseMessage> GetLatestSingleValue(string thing,string valueName)
        {            
            return await Task.Run(async () =>
            {                
                var response = new HttpResponseMessage(HttpStatusCode.OK);

                GetDataResult getDataResult = await GetDataService.GetLastestSingleValueForThing(thing,valueName);
                switch (getDataResult.GetDataResultCode)
                {
                    case GetDataResultCode.Success:                        
                        response.Content = new StringContent(getDataResult.ResultData.ToString(), Encoding.UTF8); 
                        response.StatusCode = HttpStatusCode.OK;
                        break;
                    case GetDataResultCode.FailedNoThingNameProvided:                                                
                    case GetDataResultCode.FailedThingNotFound:                        
                    case GetDataResultCode.ValueNameNotFound:
                    case GetDataResultCode.ValueNameNotProvided:
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