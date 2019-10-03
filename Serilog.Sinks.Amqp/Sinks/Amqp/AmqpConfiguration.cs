namespace Serilog.Sinks.Amqp
{
    public class AmqpConfiguration
    {
        public string QueueName { get; set; } = "Serilog.Sinks.Amqp";
        public bool IsRpc { get; set; } = true;
        public string RpcMethodName { get; set; } = "SerilogPlainTextLog";
    }
}