using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Ruzzie.SensorData.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/for/{thing}");            
            config.MapHttpAttributeRoutes();

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());            
            config.Formatters.Add(new FormUrlEncodedMediaTypeFormatter());
            config.EnableCors(new EnableCorsAttribute("*","*","GET,POST"));
            //config.Formatters.Add(new XmlMediaTypeFormatter());

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
        }
    }
}