namespace Ruzzie.SensorData.Web
{
    public interface IWebJob
    {
        void Start();
        string JobId { get; }
    }
}