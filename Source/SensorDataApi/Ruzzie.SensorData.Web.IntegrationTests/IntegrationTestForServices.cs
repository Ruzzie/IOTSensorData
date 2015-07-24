using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.GetData;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class IntegrationTestForServices
    {
        [Test]
        public void IntegrationTest()
        {
            //Arrange            
            IPushDataService pushDataService = Container.PushDataService;
            IGetDataService getDataService = Container.GetDataService;

            string thingName = Guid.NewGuid().ToString();

            //Act
            pushDataService.PushData(thingName, DateTime.Now,
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "25.0") }).Wait();
            DataResult dataResult = getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(dataResult.ResultData.Temperature, Is.EqualTo("25.0"));
        }

        [Test]
        public void UncachedItemInTierOneCache_ShouldBeReturned_FromTierTwoCache_AndUpdateTierOneCache()
        {
            //Arrange
            IWriteThroughCache writeThroughLocalCache = Container.WriteThroughLocalCache;            
            IPushDataService pushDataService = Container.PushDataService;
            IGetDataService getDataService = Container.GetDataService;

            string thingName = Guid.NewGuid().ToString();
            Debug.WriteLine(thingName);
            pushDataService.PushData(thingName, DateTime.Now,
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "25.0") }).Wait();

            //Act            
            writeThroughLocalCache.ResetLatestEntryCache();
            DataResult dataResult = getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(writeThroughLocalCache.GetLatest(thingName).Result,Is.Not.Null);
            Assert.That(dataResult.ResultData, Is.Not.Null);
            Assert.That(dataResult.ResultData.Temperature.ToString(), Is.EqualTo("25.0"));            
        }


        [Test]
        public void UncachedItemInAllCaches_ShouldBeReturned_AndUpdateCaches()
        {
            //Arrange
            IWriteThroughCache writeThroughLocalCache = Container.WriteThroughLocalCache;
            IWriteThroughCache writeThroughRedisCache = Container.RedisWriteThroughCache;
            IPushDataService pushDataService = Container.PushDataService;
            IGetDataService getDataService = Container.GetDataService;

            string thingName = Guid.NewGuid().ToString();
            Debug.WriteLine(thingName);
            pushDataService.PushData(thingName, DateTime.Now,
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "25.0") }).Wait();

            //Act     
            writeThroughRedisCache.RemoveItemFromLatestEntryCache(thingName);
            writeThroughLocalCache.RemoveItemFromLatestEntryCache(thingName);

            //Items should now be flushed from cache
            Assert.That(writeThroughLocalCache.GetLatest(thingName).Result, Is.Null);
            Assert.That(writeThroughRedisCache.GetLatest(thingName).Result, Is.Null);

            DataResult dataResult = getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert            
            Assert.That(dataResult.ResultData, Is.Not.Null);
            Assert.That(dataResult.ResultData.Temperature.ToString(), Is.EqualTo("25.0"));
            //Caches should be updated
            Assert.That(writeThroughLocalCache.GetLatest(thingName).Result, Is.Not.Null);
            Thread.Sleep(100);//wait for redis cache update
            Assert.That(writeThroughRedisCache.GetLatest(thingName).Result, Is.Not.Null);
        }

        [Test]
        public void GetLatestForNonExistantThingShouldNotThrowException()
        {
            //Arrange            
            IGetDataService getDataService = Container.GetDataService;

            string thingName = Guid.NewGuid().ToString();

            //Act            
            DataResult dataResult = getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(dataResult.DataResultCode, Is.EqualTo(DataResultCode.FailedThingNotFound));            
        }
        
    }
}