using System;
using Amqp;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Amqp
{
    public static class AmqpLoggerConfigurationExtensions
    {
        public static LoggerConfiguration AmqpSink(
                this LoggerSinkConfiguration loggerConfiguration,
                IConnection amqpConnection,
                AmqpConfiguration remoteConfiguration,
                ITextFormatter textFormatter = null,
                IFormatProvider formatProvider = null,
                LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            var _sink = new AmqpSink(amqpConfig: remoteConfiguration, amqpConnection: amqpConnection, textFormatter: textFormatter, formatProvider: formatProvider);
            return loggerConfiguration.Sink(logEventSink: _sink);
        }
    }
}
