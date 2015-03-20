using System.Configuration;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.GetData;
using Ruzzie.SensorData.Web.PushData;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web
{
    internal static class Container
    {
        internal static readonly string ConnString;

        static Container()
        {
            MongoClassMapBootstrap.Register();
            ConnString = ConfigurationManager.AppSettings["mongodbconnectionstring"];
            ISensorItemDataRepository sensorItemDataRepositoryMongo = new SensorItemDataRepositoryMongo(ConnString);
            var writeThroughCacheLocal = new WriteThroughCacheLocal();
            PushDataService = new PushDataService(new DataWriteServiceWithCache(writeThroughCacheLocal, sensorItemDataRepositoryMongo));
            GetDataService = new GetDataService(new DataReadServiceWithCache(writeThroughCacheLocal, sensorItemDataRepositoryMongo));
        }

        public static IPushDataService PushDataService { get; private set; }
        public static IGetDataService GetDataService { get; private set; }
    }
}