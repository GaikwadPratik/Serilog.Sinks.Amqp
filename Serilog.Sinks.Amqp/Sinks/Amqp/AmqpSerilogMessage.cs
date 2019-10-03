using Amqp.Serialization;
using AmqpNetLiteRpcCore;

namespace Serilog.Sinks.Amqp
{
    [RpcMethodParameter]
    [AmqpContract(Encoding = EncodingType.SimpleMap)]
    public class AmqpSerilogMessage
    {
        [AmqpMember(Name = "logMessage")]
        public string LogMessage { get; set; }
    }
}