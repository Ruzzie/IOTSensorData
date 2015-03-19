﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.Cache;
using Ruzzie.SensorData.Web.PushData;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.UnitTests
{    
    public class PushDataServiceTests
    {
        private IPushDataService _pushDataService;

        [SetUp]
        public void SetUp()
        {
            ISensorItemDataRepository repository =new  Moq.Mock<ISensorItemDataRepository>().Object;
            _pushDataService = new PushDataService(new DataWriteServiceWithCache(new WriteThroughCacheLocal(), repository ));
        }


        [TestFixture]
        class PushDataWithKeyValuePairs : PushDataServiceTests
        {
            [Test]
            public void SmokeTest()
            {
                //Arrange
                var nameValuePairs = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "22.3") };
                string thingName = "MyTestThing";

                //Act
                PushDataResult result = _pushDataService.PushData(thingName, DateTime.Now, nameValuePairs);

                //Assert
                Assert.That(result.PushDataResultCode, Is.EqualTo(PushDataResultCode.Success));
            }

            [Test]
            public void ReturnFailedDataIsEmptyWhenNoDataIsPushed()
            {
                //Act
                PushDataResult result = _pushDataService.PushData("EmptyDataForThing", DateTime.Now, new List<KeyValuePair<string, string>>());

                //Assert
                Assert.That(result.PushDataResultCode, Is.EqualTo(PushDataResultCode.FailedEmptyData));
            }

            [Test]
            public void ReturnFailedDataIsNullWhenNoDataIsPushed()
            {
                List<KeyValuePair<string, string>> nameValuePairs = null;
                //Act
// ReSharper disable once ExpressionIsAlwaysNull
                PushDataResult result = _pushDataService.PushData("EmptyDataForThing", DateTime.Now, nameValuePairs);

                //Assert
                Assert.That(result.PushDataResultCode, Is.EqualTo(PushDataResultCode.FailedEmptyData));
            }

            [Test]
            [TestCase("")]
            [TestCase(" ")]
            [TestCase(null)]
            public void ReturnFailedNoThingNameWhenThingNameIsNullOrEmpty(string thingName)
            {
                //Act
                PushDataResult result = _pushDataService.PushData(thingName, DateTime.Now, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "22.3") });

                //Assert
                Assert.That(result.PushDataResultCode, Is.EqualTo(PushDataResultCode.FailedNoThingNameProvided));
            }

            [Test]
            public void BuildValidDataObjectFromValidKeyValuePairs()
            {
                //Arrange
                var nameValuePairs = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "22.3"), new KeyValuePair<string, string>("Sound", "999") };
                string thingName = "MyTestThing";

                //Act
                PushDataResult result = _pushDataService.PushData(thingName, DateTime.Now, nameValuePairs);

                //Assert
                Assert.That(result.ResultData.Temperature, Is.EqualTo("22.3"));
                Assert.That(result.ResultData.Sound, Is.EqualTo("999"));
            }

            [Test]
            public void DateTimeMustBePresentInResult()
            {
                //Arange
                DateTime currentDateTime = new DateTime(2015, 3, 1);

                //Act            
                PushDataResult result = _pushDataService.PushData("Test", currentDateTime, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "22.3") });

                //Assert
                Assert.That(result.TimeStamp, Is.EqualTo(currentDateTime));
            }

            [Test]
            public void MustReturnThingName()
            {
                //Arange
                DateTime currentDateTime = new DateTime(2015, 3, 1);

                //Act            
                PushDataResult result = _pushDataService.PushData("TestName", currentDateTime, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Temperature", "22.3") });

                //Assert
                Assert.That(result.ThingName, Is.EqualTo("TestName"));
            }
             
        }

        [TestFixture]
        class PushDataWithJsonString : PushDataServiceTests
        {
            [Test]
            public void SmokeTest()
            {
                //Arrange
                dynamic content = new DynamicDictionaryObject();
                content.Temperature = "22.3";
                string thingName = "MyTestThing";

                //Act
                PushDataResult result = _pushDataService.PushData(thingName, DateTime.Now, content);

                //Assert
                Assert.That(result.PushDataResultCode, Is.EqualTo(PushDataResultCode.Success));
            }
           
          

            [Test]
            public void ReturnFailedDataIsNullWhenNoDataIsPushed()
            {
                //Act
                PushDataResult result = _pushDataService.PushData("EmptyDataForThing", DateTime.Now, null as DynamicDictionaryObject);

                //Assert
                Assert.That(result.PushDataResultCode, Is.EqualTo(PushDataResultCode.FailedEmptyData));
            }

            [Test]
            [TestCase("")]
            [TestCase(" ")]
            [TestCase(null)]
            public void ReturnFailedNoThingNameWhenThingNameIsNullOrEmpty(string thingName)
            {                
                //Arrange
                dynamic content = new DynamicDictionaryObject();
                content.Temperature = "22.3";
                
                //Act
                PushDataResult result = _pushDataService.PushData(thingName, DateTime.Now, content);

                //Assert
                Assert.That(result.PushDataResultCode, Is.EqualTo(PushDataResultCode.FailedNoThingNameProvided));
            }

            [Test]
            public void BuildValidDataObjectFromValidObject()
            {                
                //Arrange
                dynamic content = new DynamicDictionaryObject();
                content.Temperature = "22.3";
                content.Sound = "999";
                
                string thingName = "MyTestThing";

                //Act
                PushDataResult result = _pushDataService.PushData(thingName, DateTime.Now, content);

                //Assert
                Assert.That(result.ResultData.Temperature, Is.EqualTo("22.3"));
                Assert.That(result.ResultData.Sound, Is.EqualTo("999"));
            }

            [Test]
            public void DateTimeMustBePresentInResult()
            {
                //Arange
                //Arrange
                dynamic content = new DynamicDictionaryObject();
                content.Temperature = "22.3";
                DateTime currentDateTime = new DateTime(2015, 3, 1);

                //Act            
                PushDataResult result = _pushDataService.PushData("Test", currentDateTime, content);

                //Assert
                Assert.That(result.TimeStamp, Is.EqualTo(currentDateTime));
            }

            [Test]
            public void MustReturnThingName()
            {
                //Arange
                dynamic content = new DynamicDictionaryObject();
                content.Temperature = "22.3";
                DateTime currentDateTime = new DateTime(2015, 3, 1);

                //Act            
                PushDataResult result = _pushDataService.PushData("TestName", currentDateTime, content);

                //Assert
                Assert.That(result.ThingName, Is.EqualTo("TestName"));
            }
        }
       
    }
}
