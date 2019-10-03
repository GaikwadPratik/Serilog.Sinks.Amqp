using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using Amqp.Serialization;
using AmqpNetLiteRpcCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Sinks.Amqp;

namespace Serilog.Sinks.Amqp.ConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello World!");
                Address _address = new Address(host: "newfoundland.8thsensus.com", port: 5672, user: "admin", password: "admin", scheme: "amqp");
                ConnectionFactory _connFactory = new ConnectionFactory();
                //_connFactory.SSL.ClientCertificates 
                Connection _connection = await _connFactory.CreateAsync(_address);

                var _amqpClient = new AmqpClient();
                _amqpClient.InitiateAmqpRpc(connection: _connection);
                IRpcServer _rpcServer = _amqpClient.CreateAmqpRpcServer(amqpNode: "amq.topic/test");
                _rpcServer.Bind();

                var _logger = new LoggerConfiguration()
                    .WriteTo.AmqpSink(_connection, new AmqpConfiguration() { QueueName = "amq.topic/test" }, textFormatter: new CustomFormatter(customerId: "Test"))
                    .CreateLogger();
                _logger.Information("Hello world, this is my first log line over amqp");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();
        }
    }

    [RpcMethodParameter]
    [AmqpContract(Encoding = EncodingType.SimpleMap)]
    public class TestRequestMap
    {
        [AmqpMember]
        public string LogMessage { get; set; }
    }

    class Test
    {
        [RpcMethod]
        public void SerilogPlainTextLog(AmqpSerilogMessage request)
        {
            Console.WriteLine(request.LogMessage);
        }
    }

    class CustomFormatter : ITextFormatter
    {
        private readonly string _customerId = string.Empty;

        public CustomFormatter(string customerId)
        {
            this._customerId = customerId;
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            output.WriteLine("{");
            output.WriteLine($"\"cid\": \"{this._customerId}\",");
            output.WriteLine($"\"message\": \"{logEvent.RenderMessage(null)}\"");
            output.WriteLine("}");
        }
    }
}
