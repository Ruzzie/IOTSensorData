using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtoBuf.Meta;
using Ruzzie.SensorData.Web.MediaFormatters;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class ProtoBufMediaFormatterTests
    {
        private ProtoBufMediaFormatter _protoBufMediaFormatter;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _protoBufMediaFormatter = new ProtoBufMediaFormatter();        
        }

        [Test]
        public void MediaHeaderReturnsCorrect()
        {           
            Assert.That(_protoBufMediaFormatter.SupportedMediaTypes.Contains(new MediaTypeHeaderValue("application/x-protobuf")),Is.True);
        }

        [Test]
        [TestCase(typeof(byte))]
        [TestCase(typeof(short))]
        [TestCase(typeof(int))]
        [TestCase(typeof(string))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(ushort))]
        [TestCase(typeof(uint))]
        public void CanSerializePrimitiveTypes(Type typeToWrite)
        {
            _protoBufMediaFormatter.CanWriteType(typeToWrite);
        }

        [Test]
        [TestCase(typeof(byte))]
        [TestCase(typeof(short))]
        [TestCase(typeof(int))]
        [TestCase(typeof(string))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(ushort))]
        [TestCase(typeof(uint))]
        public void CanDeserializePrimitiveTypes(Type typeToWrite)
        {
            _protoBufMediaFormatter.CanReadType(typeToWrite);
        }


        [Test]
        [TestCase(typeof(byte),(byte) 12)]
        [TestCase(typeof(short), (short)12)]
        [TestCase(typeof(int), (int)12)]
        [TestCase(typeof(string), "12")]
        [TestCase(typeof(bool),true)]
        [TestCase(typeof(ushort), (ushort)12)]
        [TestCase(typeof(uint), (uint)12)]
        public void SerializePrimitiveValuesMustSucceed(Type typeToWrite, object valueToWrite)
        {
            //Arrange
            Stream writeStream = new MemoryStream();
            HttpContent content = new StreamContent(writeStream);
            TransportContext transportContext = null;

            //Act
            Task writeToStreamAsync = _protoBufMediaFormatter.WriteToStreamAsync(typeToWrite, valueToWrite, writeStream, content, transportContext);            
            writeToStreamAsync.Wait();
            writeStream.Flush();
            writeStream.Seek(0, SeekOrigin.Begin);            

            //Assert
            object deserializedValue = RuntimeTypeModel.Default.Deserialize(writeStream, null, typeToWrite);
            Assert.That(deserializedValue,Is.EqualTo(valueToWrite));
        }

        [TestCase(typeof(byte), (byte)12)]
        [TestCase(typeof(short), (short)12)]
        [TestCase(typeof(int), (int)12)]
        [TestCase(typeof(string), "12")]
        [TestCase(typeof(bool), true)]
        [TestCase(typeof(ushort), (ushort)12)]
        [TestCase(typeof(uint), (uint)12)]
        public void DeserializePrimitiveValuesMustSucceed(Type type, object value)
        {
            //Arrange
       
            Stream readStream = new MemoryStream();
            HttpContent content =new StreamContent(readStream);
            IFormatterLogger formatLogger = new Moq.Mock<IFormatterLogger>().Object;

            _protoBufMediaFormatter.WriteToStreamAsync(type, value, readStream, content, null).Wait();
            readStream.Flush();
            readStream.Seek(0, SeekOrigin.Begin);   


            _protoBufMediaFormatter.ReadFromStreamAsync(type, readStream, content, formatLogger);

        }
    }

    
}
