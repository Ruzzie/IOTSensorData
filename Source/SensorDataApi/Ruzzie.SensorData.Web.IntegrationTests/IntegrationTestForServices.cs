using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.GetData;
using Ruzzie.SensorData.Web.PushData;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class IntegrationTestForServices
    {
        [Test]
        public void IntegrationTest()
        {
            //Arrange
            IWriteThroughCache writeThroughCache = new WriteThroughCacheLocal();
            IWriteThroughCache writeThroughRedisCache = new WriteThroughRedisCache(Container.RedisConnString);
            SensorItemDataRepositoryMongo sensorItemDataRepositoryMongo = new SensorItemDataRepositoryMongo(MongoDataRepositoryTests.ConnString);
            IDataWriteService dataWriteService = new DataWriteServiceWithCache(writeThroughCache, sensorItemDataRepositoryMongo );
            IDataReadService dateReadService = new DataReadServiceWithCache(writeThroughCache, writeThroughRedisCache,
                sensorItemDataRepositoryMongo);
            IPushDataService pushDataService = new PushDataService(dataWriteService);
            IGetDataService getDataService = new GetDataService(dateReadService);

            string thingName = Guid.NewGuid().ToString();

            //Act
            pushDataService.PushData(thingName, DateTime.Now,
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "25.0") });
            GetDataResult getDataResult = getDataService.GetLastestDataEntryForThing(thingName);

            //Assert
            Assert.That(getDataResult.ResultData.Temperature, Is.EqualTo("25.0"));
        }

        [Test]
        public void UncachedItemShouldBeReturned()
        {
            //Arrange
            IWriteThroughCache writeThroughCache = new WriteThroughCacheLocal();
            IWriteThroughCache writeThroughRedisCache = new WriteThroughRedisCache(Container.RedisConnString);
            SensorItemDataRepositoryMongo sensorItemDataRepositoryMongo = new SensorItemDataRepositoryMongo(MongoDataRepositoryTests.ConnString);
            IDataWriteService dataWriteService = new DataWriteServiceWithCache(writeThroughCache, sensorItemDataRepositoryMongo);
            IDataReadService dateReadService = new DataReadServiceWithCache(writeThroughCache, writeThroughRedisCache,
                sensorItemDataRepositoryMongo);
            IPushDataService pushDataService = new PushDataService(dataWriteService);
            IGetDataService getDataService = new GetDataService(dateReadService);

            string thingName = Guid.NewGuid().ToString();
            pushDataService.PushData(thingName, DateTime.Now,
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "25.0") });

            //Act            
            writeThroughCache.ResetLatestEntryCache();
            GetDataResult getDataResult = getDataService.GetLastestDataEntryForThing(thingName);
            

            //Assert
            Assert.That(getDataResult.ResultData, Is.Not.Null);
            Assert.That(getDataResult.ResultData.Temperature, Is.EqualTo("25.0"));            
        }

        [Test]
        public void GetLatestForNonExistantThingShouldNotThrowException()
        {
            //Arrange
            IWriteThroughCache writeThroughCache = new WriteThroughCacheLocal();
            IWriteThroughCache writeThroughRedisCache = new WriteThroughRedisCache(Container.RedisConnString);
            SensorItemDataRepositoryMongo sensorItemDataRepositoryMongo = new SensorItemDataRepositoryMongo(MongoDataRepositoryTests.ConnString);            
            IDataReadService dateReadService = new DataReadServiceWithCache(writeThroughCache,writeThroughRedisCache, sensorItemDataRepositoryMongo);
            
            IGetDataService getDataService = new GetDataService(dateReadService);

            string thingName = Guid.NewGuid().ToString();

            //Act            
            GetDataResult getDataResult = getDataService.GetLastestDataEntryForThing(thingName);

            //Assert
            Assert.That(getDataResult.GetDataResultCode, Is.EqualTo(GetDataResultCode.FailedThingNotFound));
            
        }
        
    }
}