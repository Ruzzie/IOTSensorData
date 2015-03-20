using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NUnit.Framework;
using Ruzzie.SensorData.Web.Repository;

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class MongoDataRepositoryTests
    {
        private SensorItemDataRepositoryMongo _repository;
        public static readonly string ConnString = Container.MongoConnString;

        [TestFixtureSetUp]
        public void SetUp()
        {            
            _repository = new SensorItemDataRepositoryMongo(ConnString);

            //Clean all test data
            _repository.RemoveAllSensorDataItems();
        }

        [Test]
        public void InsertSensorDataWithSimpleDatastructureTest()
        {
            dynamic dataContent = new DynamicDictionaryObject();
            dataContent.Temperature = "22";
            var sensorItemData = new SensorItemDataDocument
            {
                Content = dataContent,
                Created = DateTime.Now,
                ThingName = "SmokeTest3"
            };

            _repository.CreateOrAdd(sensorItemData).Wait();
            
            long count = _repository.SensorItemDataCollection.CountAsync(doc => doc.ThingName == sensorItemData.ThingName).Result;

            Assert.That(count, Is.EqualTo(1));
            dynamic resultContent = _repository.GetLatest("SmokeTest3").Result.Content;
            Assert.That(resultContent.Temperature, Is.EqualTo("22"));
        }

        [Test]
        public void InsertSensorDataWithComplexDatastructureTest()
        {
            dynamic dataContent = new DynamicDictionaryObject();
            dataContent.Temperature = "22";
            dataContent.RawValues = new[] {1, 2, 3, 4, 5, 6};
            var sensorItemData = new SensorItemDataDocument
            {
                Content = dataContent,
                Created = DateTime.Now,
                ThingName = "SmokeTest4"
            };

            _repository.CreateOrAdd(sensorItemData).Wait();

            long count = _repository.SensorItemDataCollection.CountAsync(doc => doc.ThingName == sensorItemData.ThingName).Result;

            Assert.That(count, Is.EqualTo(1));
            dynamic resultContent = _repository.GetLatest("SmokeTest4").Result.Content;
            Assert.That(resultContent.Temperature, Is.EqualTo("22"));
            Assert.That(resultContent.RawValues[0], Is.EqualTo(1));
        }       
    }

    [TestFixture]
    public class MongoSerializationTests
    {
        [TestFixtureSetUp]
        public void RegisterClassMaps()
        {
            MongoClassMapBootstrap.Register();
        }

        [Test]
        public void TestBsonSerialization()
        {
            //Arrange
            dynamic dataContent = new DynamicDictionaryObject();
            dataContent.Temperature = "22";
            dataContent.RawValues = new[] { 1, 2, 3, 4, 5, 6 };
            var sensorItemData = new SensorItemDataDocument
            {
                Content = dataContent,
                Created = DateTime.Now,
                ThingName = "SerTest1"
            };


            //Act
            BsonDocument bsonDocument = sensorItemData.ToBsonDocument();

            //Assert
            Assert.That(bsonDocument["Content"]["rawvalues"].BsonType, Is.EqualTo(BsonType.Array));
            Assert.That(bsonDocument["Content"]["rawvalues"][0], Is.EqualTo(new BsonInt32(1)));
        }

        [Test]
        public void TestBsonDeserialization()
        {
            //Arrange
            dynamic dataContent = new DynamicDictionaryObject();
            dataContent.Temperature = "22";
            dataContent.RawValues = new[] { 1, 2, 3, 4, 5, 6 };
            var sensorItemData = new SensorItemDataDocument
            {
                Content = dataContent,
                Created = DateTime.Now,
                ThingName = "SerTest1"
            };

            BsonDocument bsonDocument = sensorItemData.ToBsonDocument();            
            
            //Act
            var deserializedDocument = BsonSerializer.Deserialize<SensorItemDataDocument>(bsonDocument);
            dynamic content = deserializedDocument.Content;

            //Assert
            Assert.That(content.RawValues, Is.TypeOf<List<object>>());
            Assert.That(content.RawValues[0], Is.EqualTo(1));
        }

        [Test]
        public void TestBsonDeserializationWithNestedDynamics()
        {
            //Arrange
            dynamic nestedDynamic = new DynamicDictionaryObject();
            nestedDynamic.MyList = new[] {"A", "B"};
            dynamic dataContent = new DynamicDictionaryObject();
            dataContent.Temperature = "22";
            dataContent.RawValues = new[] { 1, 2, 3, 4, 5, 6 };
            dataContent.NestedItem = nestedDynamic;

            var sensorItemData = new SensorItemDataDocument
            {
                Content = dataContent,
                Created = DateTime.Now,
                ThingName = "SerTest1"
            };

            BsonDocument bsonDocument = sensorItemData.ToBsonDocument();

            //Act
            SensorItemDataDocument deserializedDocument = BsonSerializer.Deserialize<SensorItemDataDocument>(bsonDocument);
            dynamic content = deserializedDocument.Content;

            //Assert
            Assert.That(content.NestedItem.MyList, Is.TypeOf<List<object>>());
            Assert.That(content.NestedItem.MyList[0], Is.EqualTo("A"));
        }

        [Test]
        public void ValidDateTimeInContentWithSerialization()
        {
            dynamic content = new DynamicDictionaryObject();
            content.MyDateTimeField = new DateTime(2015, 12, 15);

            //Act
            BsonDocument bsonDocument = (content as DynamicDictionaryObject).ToBsonDocument();

            //Assert
            Assert.That(bsonDocument["mydatetimefield"].BsonType, Is.EqualTo(BsonType.DateTime));
            Assert.That(bsonDocument["mydatetimefield"], Is.EqualTo(new BsonDateTime(new DateTime(2015, 12, 15))));
        }
    }


   
}
