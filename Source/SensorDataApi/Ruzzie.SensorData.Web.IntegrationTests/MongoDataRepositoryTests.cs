using System;
using System.Linq;
using NUnit.Framework;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class MongoDataRepositoryTests
    {
        private SensorItemDataRepositoryMongo _repository;
        public static readonly string ConnString = Container.ConnString;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _repository = new SensorItemDataRepositoryMongo(ConnString);

            //Clean all test data
            _repository.RemoveAllSensorDataItems();
        }

        [Test]
        public void InsertSensorDataTest()
        {
            dynamic dataContent = new DynamicDictionaryObject();
            dataContent.Temperature = "22";
            var sensorItemData = new SensorItemDataDocument
            {
                Content = dataContent,
                Created = DateTime.Now,
                ThingName = "SmokeTest3"
            };

            _repository.CreateOrAdd(sensorItemData);

            int count = _repository.SensorItemDataDocuments.Count(doc => doc.ThingName == sensorItemData.ThingName);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(_repository.SensorItemDataDocuments.First(item => item.ThingName == "SmokeTest3").Content.Temperature, Is.EqualTo("22"));
        }
    }


   
}
