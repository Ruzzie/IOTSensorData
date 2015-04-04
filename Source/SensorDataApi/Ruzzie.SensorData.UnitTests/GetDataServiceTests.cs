using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.GetData;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class GetDataServiceTests
    {
        private IGetDataService _getDataService;

        [SetUp]
        public void SetUp()
        {
            var dataReadServiceFake = new Mock<IDataReadService>();
            dataReadServiceFake.Setup(service => service.GetLatestEntryForThing("ThingNameTest"))
                .Returns(Task.FromResult(new SensorItemDataDocument
                {
                    Content = new DynamicObjectDictionary {new KeyValuePair<string, object>("MyValue",1)},
                    Created = DateTime.Now,
                    ThingName = "ThingNameTest"
                }));
            
            _getDataService = new GetDataService(dataReadServiceFake.Object);
        }


        [Test]
        public void SmokeTest()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = _getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(result.GetDataResultCode, Is.EqualTo(GetDataResultCode.Success));
        }

        [Test]
        public void DataObjectMustBeSetWhenValidThingIsRequested()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = _getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(result.ResultData, Is.Not.Null);
        }

        [Test]
        public void MustReturnThingName()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = _getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(result.ThingName, Is.EqualTo("ThingNameTest"));
        }



        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void ReturnFailedNoThingNameWhenThingNameIsNullOrEmpty(string thingName)
        {
            //Act
            GetDataResult result = _getDataService.GetLatestDataEntryForThing(thingName).Result;

            //Assert
            Assert.That(result.GetDataResultCode, Is.EqualTo(GetDataResultCode.FailedThingNameNotProvided));
        }

        [Test]
        public async void GetSingleValueMustReturnSingleValueWhenPresent()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = await _getDataService.GetLatestSingleValueForThing(thingName,"MyValue");

            //Assert
            Assert.That(result.ResultData, Is.EqualTo(1));
        }


        [Test]
        public async void GetSingleValueMustReturnSingleValueNotProvided()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = await _getDataService.GetLatestSingleValueForThing(thingName, "");

            //Assert
            Assert.That(result.GetDataResultCode, Is.EqualTo(GetDataResultCode.ValueNameNotProvided));
        }

        [Test]
        public async void GetSingleValueMustReturnValueNotFoundWhenValueNameIsNotPresent()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = await _getDataService.GetLatestSingleValueForThing(thingName, "MyValueDoesNotExist");

            //Assert
            Assert.That(result.GetDataResultCode, Is.EqualTo(GetDataResultCode.ValueNameNotFound));
        }
    }
}
