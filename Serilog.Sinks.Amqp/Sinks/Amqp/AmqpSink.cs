using Amqp;
using Amqp.Framing;
using AmqpNetLiteRpcCore;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Amqp
{
    public class AmqpSink : ILogEventSink, IDisposable
    {
        private readonly IConnection _connection = null;
        private readonly ISession _amqpSession = null;
        private readonly ISenderLink _amqpSender = null;
        private readonly AmqpConfiguration _amqpConfig = null;
        private readonly AmqpClient _amqpClient = null;
        private readonly IRpcClient _rpcClient = null;

        private readonly ITextFormatter _textFormatter = null;
        private readonly IFormatProvider _formatProvider = null;

        public AmqpSink(AmqpConfiguration amqpConfig, IConnection amqpConnection, ITextFormatter textFormatter, IFormatProvider formatProvider)
        {
            this._amqpConfig = amqpConfig;
            this._amqpClient = new AmqpClient();
            this._connection = amqpConnection;
            if (amqpConfig.IsRpc)
            {
                this._amqpClient.InitiateAmqpRpc(this._connection);
                this._rpcClient = this._amqpClient.CreateAmqpRpcClient(amqpNode: this._amqpConfig.QueueName);
            }
            else
            {
                this._amqpSession = amqpConnection.CreateSession();
                this._amqpSender = this._amqpSession.CreateSender(name: "Amqp-Serilog.Sinks.Amqp", address: this._amqpConfig.QueueName);
            }
            this._textFormatter = textFormatter;
            this._formatProvider = formatProvider;
        }

        public async void Dispose()
        {
            if (this._amqpConfig.IsRpc)
            {
                await this._amqpClient.CloseRpcClientAsync(amqpNode: this._amqpConfig.QueueName);
                await this._amqpClient.CloseAmqpClientSessionAsync();
            }
            else
            {
                await this._amqpSession.CloseAsync();
            }
        }

        public async void Emit(LogEvent logEvent)
        {
            string _logMessage = string.Empty;
            if (this._textFormatter != null)
            {
                var _sb = new StringBuilder();
                using (var _sw = new StringWriter(_sb))
                {
                    _textFormatter.Format(logEvent: logEvent, output: _sw);
                }
                _logMessage = _sb.ToString();
            }
            else
            {
               _logMessage = logEvent.RenderMessage(this._formatProvider);
            }
            var _message = new AmqpSerilogMessage() { LogMessage = _logMessage };
            if (this._amqpConfig.IsRpc)
            {
                await this._rpcClient.CallAsync<object>(this._amqpConfig.RpcMethodName, _message);
            }
            else
            {
                await this._amqpSender.SendAsync(new Message() { BodySection = new AmqpValue<AmqpSerilogMessage>(_message) });
            }
        }
    }
}