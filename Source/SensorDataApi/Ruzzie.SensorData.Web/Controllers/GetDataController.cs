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
        /// <returns></returns>
        [Route("get/latest/data/for/{thing}")]
        public async Task<GetDataResult> GetLatest(string thing)
        {
            return await GetDataService.GetLastestDataEntryForThingAsync(thing);
        }
    }
}