using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.Web.Controllers
{
    public class PushDataController : ApiController
    {
        //todo:add ioc
        private static readonly IPushDataService PushDataService = Container.PushDataService;

        /// <summary>
        ///     Push data for a thing with querystring parameters.
        /// </summary>
        /// <param name="thing">A unique name of a thing. It is recommended that you use a GUID as to avoid name collisions.</param>
        /// <returns><see cref="DataResult"/></returns>
        public async Task<DataResult> Get(string thing)
        {
            return await PushDataService.PushData(thing, DateTime.UtcNow, Request.GetQueryNameValuePairs());
        }

        
        /// <summary>
        ///     Push data for a thing.
        /// </summary>
        /// <param name="thing">A unique name of a thing. It is recommended that you use a GUID as to avoid name collisions.</param>
        /// <param name="content">The actual content of the string. Can be any valid JSON string.</param>
        /// <returns><see cref="DataResult"/></returns>
        public async Task<DataResult> Post(string thing, [FromBody] DynamicObjectDictionary content)
        {
            return await PushDataService.PushData(thing, DateTime.UtcNow, content);
        }
    }
}