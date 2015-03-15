using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using Ruzzie.SensorData.Web.GetData;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.Web
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            appBuilder.UseWebApi(config);
        } 
    }
}

namespace Ruzzie.SensorData.Web.IntegrationTests
{
    [TestFixture]
    public class ApiCallsTests
    {
        private TestServer _server;

        [TestFixtureSetUp]
        public void StartServer()
        {
            _server = TestServer.Create<Startup>();
        }

        [TestFixtureTearDown]
        public void StopServer()
        {
            if (_server != null)
            {
                _server.Dispose();
            }
        }

        [Test]
        public void PushData_With_Valid_Get_Should_Succeed()
        {            
            var response = _server.HttpClient.GetAsync("/pushdata/for/IntTest?Temperature=2").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            var resultObject = response.Content.ReadAsAsync<PushDataResult>().Result;

            Assert.That(resultObject.PushDataResultCode, Is.EqualTo(PushDataResultCode.Success));
            Assert.That(resultObject.ResultData.Temperature, Is.EqualTo("2"));            
        }

        [Test]
        public void PushData_With_InValid_Get_Should_Fail()
        {
            var response = _server.HttpClient.GetAsync("/pushdata/for/IntTest2?").Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultObject = response.Content.ReadAsAsync<PushDataResult>().Result;

            Assert.That(resultObject.PushDataResultCode, Is.EqualTo(PushDataResultCode.FailedEmptyData));            
        }

        [Test]
        public void PushData_With_Valid_Post_Should_Succeed()
        {
            var response = _server.HttpClient.PostAsJsonAsync("/pushdata/for/IntTest3", new {Temperature = "22"}).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultObject = response.Content.ReadAsAsync<PushDataResult>().Result;

            Assert.That(resultObject.PushDataResultCode, Is.EqualTo(PushDataResultCode.Success));
            Assert.That(resultObject.ResultData.Temperature, Is.EqualTo("22"));
        }

        [Test]
        public void PushData_With_InValid_Post_Should_Fail()
        {            
            var response = _server.HttpClient.PostAsync("/pushdata/for/IntTest4", new{},new JsonMediaTypeFormatter()).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultObject = response.Content.ReadAsAsync<PushDataResult>().Result;

            Assert.That(resultObject.PushDataResultCode, Is.EqualTo(PushDataResultCode.FailedEmptyData));            
        }

        [Test]
        public void GetData_After_Push_Returns_Latest()
        {
            _server.HttpClient.PostAsJsonAsync("/pushdata/for/IntTest5", new { Temperature = "22" }).Wait();
            _server.HttpClient.PostAsJsonAsync("/pushdata/for/IntTest5", new { Temperature = "22.5" }).Wait();

            var resultObject = _server.HttpClient.GetAsync("/get/latest/data/for/IntTest5").Result.Content.ReadAsAsync<GetDataResult>().Result;            

            Assert.That(resultObject.GetDataResultCode, Is.EqualTo(GetDataResultCode.Success));
            Assert.That(resultObject.ResultData.Temperature, Is.EqualTo("22.5"));
        }

        [Test]
        public void GetData_For_Non_Existant_Thing_Should_Fail()
        {
            var response = _server.HttpClient.GetAsync("/get/latest/data/for/"+new Guid()).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var resultObject = response.Content.ReadAsAsync<GetDataResult>().Result;

            Assert.That(resultObject.GetDataResultCode, Is.EqualTo(GetDataResultCode.FailedThingNotFound));            
        }


    }
}
