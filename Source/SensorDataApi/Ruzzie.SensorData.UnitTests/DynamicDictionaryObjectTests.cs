using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class DynamicDictionaryObjectTests
    {

        class ConstructorTests
        {
            [Test]
            public void CreateWithoutParameters()
            {
                //Act
                dynamic dynamicObjectDictionary = new DynamicObjectDictionary();
                dynamicObjectDictionary.KeyOne = "1";
                //Assert
                Assert.That(dynamicObjectDictionary.keyone, Is.EqualTo("1"));
                Assert.That(new DynamicObjectDictionary(), Is.Not.Null);
            }

            [Test]
            public void CreateWithIEnumerable()
            {
                //Act
                dynamic dynamicObjectDictionary = new DynamicObjectDictionary(new[]
                {new KeyValuePair<string, dynamic>("KeyOne", "ValueOne"), new KeyValuePair<string, dynamic>("KeyTwo", 2),});

                //Assert
                Assert.That(dynamicObjectDictionary.keyOne, Is.EqualTo("ValueOne"));
                Assert.That(dynamicObjectDictionary.KeyTwo , Is.EqualTo(2));
            }

            [Test]
            public void CreateWithInitialSize()
            {
                //Act
                dynamic dynamicObjectDictionary = new DynamicObjectDictionary(10);
                dynamicObjectDictionary.KeyOne = "1";
                //Assert
                Assert.That(dynamicObjectDictionary.keyone, Is.EqualTo("1"));
            }
        }

        [Test]
        public void SetMemberViaIndexerTwiceShouldOverwriteValue()
        {
            dynamic testObject = new DynamicObjectDictionary();
            testObject["Property"] = "MyValue";
            testObject["Property"] = "MyValue";
        }

        [Test]
        public void SetMemberViaPropertyTwiceShouldOverwriteValue()
        {
            dynamic testObject = new DynamicObjectDictionary();
            testObject.Property = "MyValue";
            testObject.Property = "MyValue";
            
        }

        [Test]
        public void CaseInsensitivityTest()
        {
            dynamic testObject = new DynamicObjectDictionary();
            testObject.property = "MyValue1";
            testObject.Property = "MyValue2";

            Assert.That(testObject.property, Is.EqualTo("MyValue2"));
        }

        [Test]
        public void ImplicitFromExpandoOperatorTest()
        {
            dynamic expandoObject = new ExpandoObject();
            DynamicObjectDictionary myObject = expandoObject;

            Assert.That(myObject,Is.TypeOf<DynamicObjectDictionary>());
        }

        [Test]
        public void GetObjectEnumeratorTest()
        {
            //Arrange
            dynamic obj = new DynamicObjectDictionary();
            obj.Temperature = "22";
            
            //Act
            var enumberableObj = obj as IEnumerable<KeyValuePair<string, object>>;           

            //Assert
            Assert.That(enumberableObj.Count(),Is.EqualTo(1));
        }
      

// ReSharper disable once UnusedMember.Global
        public static bool IsDictionaryType(Type type)
        {
            Type dictType = typeof (IDictionary);
            Type interfaceType;
            return dictType.IsAssignableFrom(type) || type.Name == "IDictionary`2"
            || ((interfaceType = type.GetInterface("IEnumerable`1")) != null && interfaceType.GetGenericArguments()[0].Name == "KeyValuePair`2");
        }

        [Test]
        public void CastToDictionaryIlTests()
        {
            MethodInfo isDictTypeMethodInfo = typeof (DynamicDictionaryObjectTests).GetMethod("IsDictionaryType");
            Type dictStringObject = typeof (IDictionary<string, object>);
            //Arrange
            DynamicObjectDictionary dynamicObjectDictionary = new DynamicObjectDictionary();
            DynamicMethod methodBuilder = new DynamicMethod("Cast_Test", typeof (IDictionary<string, object>),new []{typeof(DynamicObjectDictionary)});
            ILGenerator il = methodBuilder.GetILGenerator();


            var isDictTypeLabel = il.DefineLabel();

            var localTypeDecl = il.DeclareLocal(typeof(Type));//Type type = typeof(DynamicObjectDictionary);
            
            il.Emit(OpCodes.Ldarg_0);//load the dictionary on the stack
            il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType"));//gettype from dynamicdictionaryobject
            il.Emit(OpCodes.Stloc,localTypeDecl);      //store in local variable
      
            il.Emit(OpCodes.Ldloc,localTypeDecl);
            il.Emit(OpCodes.Call, isDictTypeMethodInfo);
            il.Emit(OpCodes.Brfalse, isDictTypeLabel);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, dictStringObject);
            
            il.Emit(OpCodes.Ret);

            il.MarkLabel(isDictTypeLabel);

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            //Act
            var castedDynamicDic = methodBuilder.Invoke(null,new object[]{dynamicObjectDictionary});

            Assert.That(castedDynamicDic,Is.Not.Null);


        }                    
    }

    
    public abstract class SensorItemDataSerializationTestBase
    {

        [Test]
        public void SimpleSerializeAndDeserializeTest()
        {
            //Arrange
            SensorItemDataDocument doc = new SensorItemDataDocument();
            doc.ThingName = "SerializedTest";
            doc.Created = new DateTime(2015, 1, 31, 0, 0, 0, DateTimeKind.Utc);

            dynamic content = new DynamicObjectDictionary();
            content.TemperatureAsString = "22";
            content.TemperatureAsInt = 22;
            content.TemperatureAsDouble = 22.0d;
            content.ItemsAsList = new List<string> {"1", "2"};
            content.ItemsAsIntArray = new[] {1, 2};
        

            doc.Content = content;

            //Act
            dynamic serializedData = Serialize(doc);
            SensorItemDataDocument deserializedDoc = Deserialize(serializedData);

            dynamic deserializedContent = deserializedDoc.Content;

            //Assert
            Assert.That(deserializedDoc.ThingName, Is.EqualTo("SerializedTest"));
            Assert.That(deserializedDoc.Created, Is.EqualTo(new DateTime(2015, 1, 31)));


            Assert.That((string)deserializedContent.TemperatureAsString, Is.EqualTo("22"));
            Assert.That((int)deserializedContent.TemperatureAsInt, Is.EqualTo(22));
            Assert.That((double)deserializedContent.TemperatureAsDouble, Is.EqualTo(22.0d));
           // Assert.That(((JArray)deserializedContent.ItemsAsIntArray)[0].Value<int>(), Is.EqualTo(1));

            Assert.That(deserializedContent.ItemsAsList.Count, Is.EqualTo(2));

        }

        [Test]
        public void SerializeAndDeserializeWithNestedObjectTest()
        {
            //Arrange
            SensorItemDataDocument doc = new SensorItemDataDocument();
            doc.ThingName = "SerializedTest";
            doc.Created = new DateTime(2015,1,31,0,0,0,DateTimeKind.Utc);
           
            dynamic content = new DynamicObjectDictionary();
            content.TemperatureAsString = "22";
            content.TemperatureAsInt = 22;
            content.TemperatureAsDouble = 22.0d;
            content.ItemsAsList = new List<string> {"1", "2"};
           
            dynamic nestedContent = new DynamicObjectDictionary();
            nestedContent.DataItemOne = new Dictionary<string,int> {{"key", 1},{"key2",2}};
            content.NestedContent = nestedContent;

            doc.Content = content;

            //Act
            dynamic serializedData = Serialize(doc);
            SensorItemDataDocument deserializedDoc = Deserialize(serializedData);
            
            dynamic deserializedContent = deserializedDoc.Content;
            dynamic deserializedNestedContent = deserializedContent.NestedContent;

            //Assert
            Assert.That(deserializedDoc.ThingName, Is.EqualTo("SerializedTest"));
            Assert.That(deserializedDoc.Created, Is.EqualTo(new DateTime(2015,1,31)));

          
            Assert.That((string) deserializedContent.TemperatureAsString,Is.EqualTo("22"));
            Assert.That((int) deserializedContent.TemperatureAsInt, Is.EqualTo(22));
            Assert.That((double) deserializedContent.TemperatureAsDouble, Is.EqualTo(22.0d));
            
            Assert.That(deserializedContent.ItemsAsList.Count,Is.EqualTo(2));

            Assert.That((int) deserializedNestedContent.dataitemone["key"], Is.EqualTo(1));

        }
       

        public abstract SensorItemDataDocument Deserialize(object serializedData);


        public abstract object Serialize(SensorItemDataDocument doc);

        public SensorItemDataDocument SerializeAndDeserialize(SensorItemDataDocument doc)
        {
            var data = Serialize(doc);
            var deserializedDoc = Deserialize(data);
            return deserializedDoc;
        }
    }
   

    [TestFixture]
    public class NewtonSoftSerializationTests : SensorItemDataSerializationTestBase
    {
        public override SensorItemDataDocument Deserialize(object serializedData)
        {
            return JsonConvert.DeserializeObject<SensorItemDataDocument>(serializedData as string);
        }

        public override object Serialize(SensorItemDataDocument doc)
        {
            return JsonConvert.SerializeObject(doc);
        }
    }
            
}
