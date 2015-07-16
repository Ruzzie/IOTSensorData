using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf.Meta;

namespace Ruzzie.SensorData.Web.MediaFormatters
{

    public class ProtoBufMediaFormatter : MediaTypeFormatter
    {
        private static readonly MediaTypeHeaderValue ProtobufMediaTypeHeader = new MediaTypeHeaderValue("application/x-protobuf");
        private readonly RuntimeTypeModel _defaultProtobufRuntimeTypeModel;

        public ProtoBufMediaFormatter()
        {
            SupportedMediaTypes.Add(ProtobufMediaTypeHeader);
            _defaultProtobufRuntimeTypeModel = RuntimeTypeModel.Default;
            _defaultProtobufRuntimeTypeModel.AutoCompile = true;
        }

        public override bool CanReadType(Type type)
        {
            return _defaultProtobufRuntimeTypeModel.CanSerialize(type);
        }

        public override bool CanWriteType(Type type)
        {
            return _defaultProtobufRuntimeTypeModel.CanSerialize(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return WriteToStreamAsync(type, value, writeStream, content, transportContext, new CancellationToken(false));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Formatter does not own stream")]
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext,
            CancellationToken cancellationToken)
        {
            Task writeToStreamTask = new Task(() =>
            {
                _defaultProtobufRuntimeTypeModel.Serialize(writeStream, value);
            }, cancellationToken);

            writeToStreamTask.Start();

            return writeToStreamTask;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return ReadFromStreamAsync(type, readStream, content, formatterLogger, new CancellationToken(false));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Formatter does not own stream")]
        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            Task<object> readFromStreamTask = new Task<object>(() => _defaultProtobufRuntimeTypeModel.Deserialize(readStream, null, type),cancellationToken);
            readFromStreamTask.Start();
            return readFromStreamTask;
        }
       
    }
}