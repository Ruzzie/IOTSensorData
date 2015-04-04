﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            GetDataResult getDataResult = getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(getDataResult.ResultData.Temperature, Is.EqualTo("25.0"));
        }

        [Test]
        public void UncachedItemShouldBeReturned()
        {
            //Arrange
            IWriteThroughCache writeThroughCache = Container.WriteThroughLocalCache;            
            IPushDataService pushDataService = Container.PushDataService;
            IGetDataService getDataService = Container.GetDataService;

            string thingName = Guid.NewGuid().ToString();
            Debug.WriteLine(thingName);
            pushDataService.PushData(thingName, DateTime.Now,
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "25.0") }).Wait();

            //Act            
            writeThroughCache.ResetLatestEntryCache();
            GetDataResult getDataResult = getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(getDataResult.ResultData, Is.Not.Null);
            Assert.That(getDataResult.ResultData.Temperature.ToString(), Is.EqualTo("25.0"));            
        }

        [Test]
        public void GetLatestForNonExistantThingShouldNotThrowException()
        {
            //Arrange            
            IGetDataService getDataService = Container.GetDataService;

            string thingName = Guid.NewGuid().ToString();

            //Act            
            GetDataResult getDataResult = getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(getDataResult.GetDataResultCode, Is.EqualTo(GetDataResultCode.FailedThingNotFound));            
        }
        
    }
}