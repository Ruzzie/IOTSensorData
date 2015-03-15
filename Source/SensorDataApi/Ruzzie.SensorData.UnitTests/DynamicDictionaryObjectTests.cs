using System.Dynamic;
using NUnit.Framework;
using Ruzzie.SensorData.Web;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class DynamicDictionaryObjectTests
    {

        [Test]
        public void SetMemberViaIndexerTwiceShouldOverwriteValue()
        {
            dynamic testObject = new DynamicDictionaryObject();
            testObject["Property"] = "MyValue";
            testObject["Property"] = "MyValue";
        }

        [Test]
        public void SetMemberViaPropertyTwiceShouldOverwriteValue()
        {
            dynamic testObject = new DynamicDictionaryObject();
            testObject.Property = "MyValue";
            testObject.Property = "MyValue";
            
        }

        [Test]
        public void CaseInsesitivityTest()
        {
            dynamic testObject = new DynamicDictionaryObject();
            testObject.property = "MyValue1";
            testObject.Property = "MyValue2";

            Assert.That(testObject.property, Is.EqualTo("MyValue2"));
        }

        [Test]
        public void ImplicitFromExpandoOperatorTest()
        {
            dynamic expandoObject = new ExpandoObject();
            DynamicDictionaryObject myObject = expandoObject;

            Assert.That(myObject,Is.TypeOf<DynamicDictionaryObject>());
        }

       
        //Add data from a device sensor
            //with identifier, value and datetime
        //Get last data for a device sensor 
        //Get all data for a device sensor sorted in time with maxtime
        //Clear all data for a sensor
        //query

        //error response
        //ok response
        
    }   
}
