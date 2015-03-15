using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ruzzie.SensorData.Web;
using Ruzzie.SensorData.Web.GetData;
using Ruzzie.SensorData.Web.PushData;

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
                    Content = new DynamicDictionaryObject(),
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
            GetDataResult result = _getDataService.GetLastestDataEntryForThing(thingName);

            //Assert
            Assert.That(result.GetDataResultCode, Is.EqualTo(GetDataResultCode.Success));
        }

        [Test]
        public void DataObjectMustBeSetWhenValidThingIsRequested()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = _getDataService.GetLastestDataEntryForThing(thingName);

            //Assert
            Assert.That(result.ResultData, Is.Not.Null);
        }

        [Test]
        public void MustReturnThingName()
        {
            //Arrange
            string thingName = "ThingNameTest";

            //Act            
            GetDataResult result = _getDataService.GetLastestDataEntryForThing(thingName);

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
            GetDataResult result = _getDataService.GetLastestDataEntryForThing(thingName);

            //Assert
            Assert.That(result.GetDataResultCode, Is.EqualTo(GetDataResultCode.FailedNoThingNameProvided));
        }
    }
}
