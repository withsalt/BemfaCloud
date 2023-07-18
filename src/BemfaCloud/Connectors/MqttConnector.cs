using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BemfaCloud.Connectors.Base;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Exceptions;
using BemfaCloud.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using ErrorEventArgs = BemfaCloud.Models.ErrorEventArgs;

namespace BemfaCloud.Connectors
{
    public class MqttConnector : BaseConnector
    {
        private IMqttClient _client = null;
        private readonly MqttClientOptions _clientOptions = null;
        private bool _isManualDisconnect = false;
        private bool _isDispose = false;
        private CancellationTokenSource _cancellation = null;
        private static readonly object _connectLock = new object();
        private static readonly object _disposeLock = new object();

        protected override event Action<ErrorEventArgs> OnError;
        protected override event Action<MessageEventArgs> OnMessage;

        public override bool IsConnected
        {
            get
            {
                if (_client == null || _isDispose) return false;
                return _client.IsConnected;
            }
        }

        public MqttConnector(BaseConnectorBuilder builder) : base(builder)
        {
            MqttClientOptionsBuilder optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(this.Builder.Host, (int)this.Builder.Port)
                .WithClientId(this.Builder.Secret)
                .WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);

            if (builder.IsEnableTls)
            {
                optionsBuilder.WithTls((o) =>
                {
                    o.AllowUntrustedCertificates = true;
                });
            }
            _clientOptions = optionsBuilder.Build();

            OnMessage += this.Builder.OnMessageHandler;
            OnError += this.Builder.OnErrorHandler;
        }

        public override Task<bool> ConnectAsync()
        {
            try
            {
                lock (_connectLock)
                {
                    if (_isDispose)
                    {
                        throw new ObjectDisposedException(this.GetType().FullName);
                    }
                    if (this.IsConnected)
                    {
                        return Task.FromResult(true);
                    }

                    _cancellation = new CancellationTokenSource();

                    _client = new MqttFactory().CreateMqttClient();
                    _client.ConnectedAsync += ConnectedEvent;
                    _client.DisconnectedAsync += DisconnectedEvent;
                    _client.ApplicationMessageReceivedAsync += DataReceivedEvent;
                    var result = _client.ConnectAsync(_clientOptions, _cancellation.Token)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                    if (result.ResultCode != MqttClientConnectResultCode.Success)
                    {
                        OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Can not connect to mqtt server. return code is {result.ResultCode}."));
                        return Task.FromResult(true);
                    }
                    _isManualDisconnect = false;
                    return Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Can not connect to mqtt server. {ex.Message}", ex));
                return Task.FromResult(false);
            }
        }

        public override Task<bool> DisconnectAsync()
        {
            if (_client == null)
            {
                return Task.FromResult(true);
            }
            try
            {
                lock (_connectLock)
                {
                    _isManualDisconnect = true;
                    _cancellation?.Cancel();

                    bool disconnectResult = false;
                    if (this.IsConnected)
                    {
                        disconnectResult = _client.TryDisconnectAsync(MqttClientDisconnectOptionsReason.NormalDisconnection)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                        if (!disconnectResult)
                        {
                            OnError?.Invoke(new ErrorEventArgs(LogType.Error, "Client disconnect with server failed."));
                        }
                    }

                    _client.Dispose();
                    _client = null;
                    _cancellation?.Dispose();
                    _cancellation = null;

                    return Task.FromResult(disconnectResult);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Can not disconnect from .", ex));
                return Task.FromResult(false);
            }
        }

        public override void Dispose()
        {
            lock (_disposeLock)
            {
                try
                {
                    if (_isDispose) return;
                    if (_client == null) return;
                    if (this.IsConnected)
                    {
                        bool result = DisconnectAsync().Result;
                        if (!result)
                        {
                            OnError?.Invoke(new ErrorEventArgs(LogType.Warining, "Disconnect with server failed."));
                        }
                    }
                    if (_client != null)
                    {
                        _client.Dispose();
                        _client = null;
                    }
                    if (_cancellation != null)
                    {
                        _cancellation.Dispose();
                        _cancellation = null;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Can not dispose mqtt client.", ex));
                }
                finally
                {
                    _isDispose = true;
                }
            }
        }

        public override async Task PublishAsync(string deviceTopic, string payload)
        {
            if (string.IsNullOrWhiteSpace(deviceTopic))
            {
                throw new ArgumentNullException(nameof(deviceTopic), "Topic can not null");
            }
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentNullException(nameof(deviceTopic), "Payload can not null");
            }
            if (!this.IsConnected)
            {
                throw new DisconnectException("Not connected to server");
            }
            if (!deviceTopic.EndsWith("/set"))
            {
                deviceTopic = deviceTopic.EndsWith('/') ? (deviceTopic + "set") : (deviceTopic + "/set");
            }
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(deviceTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();
            await _client.PublishAsync(applicationMessage, _cancellation.Token).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(string deviceTopic, string payload)
        {
            if (string.IsNullOrWhiteSpace(deviceTopic))
            {
                throw new ArgumentNullException(nameof(deviceTopic), "Topic can not null");
            }
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentNullException(nameof(deviceTopic), "Payload can not null");
            }
            if (!this.IsConnected)
            {
                throw new DisconnectException("Not connected to server");
            }
            if (!deviceTopic.EndsWith("/up"))
            {
                deviceTopic = deviceTopic.EndsWith('/') ? (deviceTopic + "up") : (deviceTopic + "/up");
            }
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(deviceTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();
            await _client.PublishAsync(applicationMessage, _cancellation.Token).ConfigureAwait(false);
        }

        #region privite

        private async Task ConnectedEvent(MqttClientConnectedEventArgs e)
        {
            OnError?.Invoke(new ErrorEventArgs($"Mqtt client connected. Start subscribing topic."));
            foreach (var item in this.Builder.Devices)
            {
                await _client.SubscribeAsync(item.Topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce, _cancellation.Token).ConfigureAwait(false);
            }
            OnError?.Invoke(new ErrorEventArgs($"Subscribe device topic: {string.Join(',', this.Builder.Devices.Select(P => P.Topic))}"));
        }

        private async Task DisconnectedEvent(MqttClientDisconnectedEventArgs e)
        {
            try
            {
                if (_isManualDisconnect || Builder.AutoReconnectDelaySeconds < 0)
                {
                    OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Mqtt client disconnected."));
                    return;
                }
                if (Builder.AutoReconnectDelaySeconds == 0)
                {
                    OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Mqtt client disconnected. Reconnecting..."));
                }
                else
                {
                    OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Mqtt client disconnected. Try reconnect after {Builder.AutoReconnectDelaySeconds} seconds..."));
                    await Task.Delay(Builder.AutoReconnectDelaySeconds, _cancellation.Token);
                }
                if (_isManualDisconnect || _isDispose || (_cancellation != null && _cancellation.IsCancellationRequested))
                {
                    return;
                }
                await _client.ConnectAsync(_clientOptions, _cancellation.Token).ConfigureAwait(false);
                return;
            }
            catch (Exception ex) when (ex != null && (ex is MqttCommunicationException || ex is MqttCommunicationTimedOutException))
            {
                return;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Error, ex.Message, ex));
            }
        }

        private Task DataReceivedEvent(MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ReasonCode != MqttApplicationMessageReceivedReasonCode.Success)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Warining, $"Message received failed. ReasonCode: {e.ReasonCode}, reason: {e.ResponseReasonString}"));
                return Task.CompletedTask;
            }
            OnMessage?.Invoke(new MessageEventArgs(CommandType.Publish, new DeviceInfo(e.ApplicationMessage.Topic), e.ApplicationMessage.PayloadSegment));
            return Task.CompletedTask;
        }
        #endregion
    }
}
