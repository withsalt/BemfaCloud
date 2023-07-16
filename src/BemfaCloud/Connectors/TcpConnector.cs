using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BemfaCloud.Connectors.Base;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Connectors.Strategy;
using BemfaCloud.Exceptions;
using BemfaCloud.Models;
using BemfaCloud.Providers.Tcp;
using BemfaCloud.Providers.Tcp.Abstractions.Connections;
using BemfaCloud.Providers.Tcp.Abstractions.Events;
using TcpClient = BemfaCloud.Providers.Tcp.TcpClient;

namespace BemfaCloud.Connectors
{
    public class TcpConnector : BaseConnector
    {
        private TcpClient _client = null;
        private bool _isManualDisconnect = false;
        private bool _isDispose = false;
        private static readonly object _sendLocker = new object();
        private Task _heartbeatTask = null;
        private CancellationTokenSource _cancellation = null;
        private CancellationTokenSource _heartbeatCancellation = null;
        private static readonly object _connectLock = new object();
        private static readonly object _disposeLock = new object();

        protected override event Action<ErrorEventArgs> OnError;
        protected override event Action<MessageEventArgs> OnMessage;

        private readonly ConcurrentDictionary<CommandType, (long, string)> _cmdResultStatusDic = new ConcurrentDictionary<CommandType, (long, string)>();
        private readonly Dictionary<CommandType, int> _commandResultTypes = new Dictionary<CommandType, int>()
        {
            { CommandType.Ping, (int)CommandType.Ping },
            { CommandType.Subscribe, (int)CommandType.Subscribe },
            { CommandType.Publish, (int)CommandType.Publish },
        };

        public override bool IsConnected
        {
            get
            {
                if (_client == null || _isDispose) return false;
                return this._client.Connected;
            }
        }

        public TcpConnector(BaseConnectorBuilder builder) : base(builder)
        {
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

                    var ipAddress = (Dns.GetHostEntry(this.Builder.Host)?.AddressList?.FirstOrDefault())
                        ?? throw new Exception($"Can not get ip address from host '{this.Builder.Host}'.");
                    BemfaTryConnectionStrategy strategy = new BemfaTryConnectionStrategy(this.Builder);
                    _client = new TcpClient(ipAddress.ToString(), (int)this.Builder.Port, connectTimeout: 3000);
                    _client.OnException += (e) => ExceptionEvent(e);
                    _client.OnConnected += (e) => ConnectedEvent(e, false);
                    _client.OnDisconnected += (e) => DisconnectedEvent(e, strategy);
                    _client.OnReceived += (e) => DataReceivedEvent(e);
                    _client.OnTryConnecting += (e) => TryConnectingEvent(e, strategy);
                    _client.OnStarted += (e) => ConnectedEvent(e, true);
                    _client.ReconnectAvailable(strategy.IsEnableReconnect);
                    _client.UseTryConnectionStrategy(strategy);
                    _client.Start();

                    _isManualDisconnect = false;

                    return Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Can not connect to tcp server. {ex.Message}", ex));
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
                    //call default cancel
                    _cancellation?.Cancel();
                    //stop heartbeat
                    StopHeartbeatTask();
                    //Stop tcp client
                    _client?.Stop();
                    //wait for disconnect
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    while (this.IsConnected && stopwatch.ElapsedMilliseconds < 3000)
                    {
                        Thread.Sleep(10);
                    }
                    stopwatch.Stop();

                    _client.Dispose();
                    _client = null;

                    _cancellation?.Dispose();
                    _cancellation = null;

                    return Task.FromResult(true);
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


        public override Task PublishAsync(string deviceTopic, string payload)
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
            string senData = CommandBuilder(CommandType.Publish, deviceTopic, payload);
            Send(senData);
            return Task.CompletedTask;
        }

        public override Task UpdateAsync(string deviceTopic, string payload)
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
            string senData = CommandBuilder(CommandType.Publish, deviceTopic, payload);
            Send(senData);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 请求服务器时间
        /// </summary>
        /// <param name="timeType"></param>
        public Task GetServerDateTime(DateTimeType timeType)
        {
            string senData = CommandBuilder(CommandType.GetTimestamp, null, null, timeType);
            Send(senData);
            return Task.CompletedTask;
        }

        #region Events

        private void ConnectedEvent(NetClientEventArgs<ITcpConnection> e, bool isStarted)
        {
            if (!isStarted)
            {
                //连接后也会调用一次，准备就绪后也会调用一次
                return;
            }

            OnError?.Invoke(new ErrorEventArgs($"Tcp client connected. Start subscribing topic."));

            string topics = string.Join(',', this.Builder.Devices.Select(p => p.Topic));
            string data = CommandBuilder(CommandType.Subscribe, topics, null);

            if (TryRequest(CommandType.Subscribe, data, out string result) && result?.Equals("1") == true)
            {
                OnError?.Invoke(new ErrorEventArgs($"Subscribe device topic: {topics}"));
            }
            else
            {
                OnError?.Invoke(new ErrorEventArgs($"Subscribe device topic failed."));
            }
            StartHeartbeatTask();
        }

        private void TryConnectingEvent(TryConnectingEventArgs<ITcpConnection> e, BemfaTryConnectionStrategy strategy)
        {
            if (strategy.IsReconnectImmediately)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Tcp client disconnected. Reconnecting..."));
            }
            else
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Tcp client disconnected. Try reconnect after {strategy.AutoReconnectDelaySeconds} seconds..."));
            }
        }

        private void DisconnectedEvent(NetClientEventArgs<ITcpConnection> e, BemfaTryConnectionStrategy strategy)
        {
            if (_isManualDisconnect || !strategy.IsEnableReconnect)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Error, $"Tcp client disconnected."));
            }
        }

        private void DataReceivedEvent(NetClientReceivedEventArgs<ITcpConnection> e)
        {
            if (e.Buffers?.Any() != true)
            {
                return;
            }
            string resultStr = Encoding.UTF8.GetString(e.Buffers)?.TrimEnd('\r', '\n');
            if (string.IsNullOrWhiteSpace(resultStr))
            {
                return;
            }
            Dictionary<string, string> resultDic = GetResultParamValues(resultStr);
            if (resultDic?.Any() != true)
            {
                return;
            }
            if (!resultDic.TryGetValue("cmd", out string cmdTypeStr))
            {
                return;
            }
            CommandType commandType = (int.TryParse(cmdTypeStr, out int typeInt) && Enum.IsDefined(typeof(CommandType), typeInt)) ? (CommandType)typeInt : CommandType.Unknow;
            if (commandType == CommandType.Unknow)
            {
                OnError?.Invoke(new ErrorEventArgs(LogType.Warining, $"Unknow result command type: {typeInt}"));
                return;
            }
            if (resultDic.TryGetValue("res", out string resVal) && _commandResultTypes.ContainsKey(commandType))
            {
                //标识为返回的指令状态
                _cmdResultStatusDic.AddOrUpdate(commandType, (GetMilliTimestamp(), resVal), (c, o) => (GetMilliTimestamp(), resVal));
                if (resVal.Equals("0"))
                {
                    OnError.Invoke(new ErrorEventArgs(LogType.Warining, $"Command excute failed, command type: {commandType}, value: {resVal}"));
                }
            }
            else
            {
                switch (commandType)
                {
                    case CommandType.Publish:
                    case CommandType.SubAndPull:
                    case CommandType.TestamentMsg:
                    case CommandType.GetTimestamp:
                        if (!resultDic.TryGetValue("msg", out string msgVal))
                        {
                            OnError?.Invoke(new ErrorEventArgs(LogType.Warining, $"Unknow result msg: {resultStr}"));
                            return;
                        }
                        DeviceInfo deviceInfo = null;
                        if (resultDic.TryGetValue("topic", out string topicVal))
                        {
                            deviceInfo = new DeviceInfo(topicVal);
                        }
                        if (commandType == CommandType.Publish || commandType == CommandType.SubAndPull || commandType == CommandType.TestamentMsg)
                        {
                            Listener?.Invoke(new MessageEventArgs(commandType, deviceInfo, Encoding.UTF8.GetBytes(msgVal)));
                        }
                        OnMessage?.Invoke(new MessageEventArgs(commandType, deviceInfo, Encoding.UTF8.GetBytes(msgVal)));
                        break;
                    default:
                        OnError?.Invoke(new ErrorEventArgs(LogType.Warining, $"Unknow result msg: {resultStr}"));
                        break;
                }
            }
        }

        private void ExceptionEvent(NetClientEventArgs<ITcpConnection> e)
        {
            if (e.Exception == null)
            {
                return;
            }
            OnError?.Invoke(new ErrorEventArgs(LogType.Error, e.Exception.Message, e.Exception));
        }

        #endregion

        #region Heartbeat

        private void StopHeartbeatTask()
        {
            try
            {
                if (_heartbeatTask != null)
                {
                    if (_heartbeatCancellation != null)
                    {
                        _heartbeatCancellation.Cancel();
                    }

                    try
                    {
                        _heartbeatTask.Wait(3000);
                        _heartbeatTask.Dispose();
                        _heartbeatTask = null;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(new ErrorEventArgs(LogType.Warining, $"Wait heart beart task exit timeout, {ex.Message}", ex));
                    }
                }
            }
            finally
            {
                if (_heartbeatCancellation != null)
                {
                    _heartbeatCancellation.Dispose();
                    _heartbeatCancellation = null;
                }
            }
        }

        private void StartHeartbeatTask()
        {
            if (_heartbeatTask != null)
            {
                StopHeartbeatTask();
            }
            _heartbeatCancellation = new CancellationTokenSource();
            _heartbeatTask = Task.Factory.StartNew(Heartbeat, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 心跳
        /// </summary>
        private async Task Heartbeat()
        {
            Func<bool> isKeep = new Func<bool>(() => !(_isManualDisconnect
                || _isDispose
                || (_cancellation != null && _cancellation.IsCancellationRequested)
                || (_heartbeatCancellation != null && _heartbeatCancellation.IsCancellationRequested)));

            while (isKeep())
            {
                try
                {
                    //每隔15s发送一次ping
                    await Task.Delay(15000, _heartbeatCancellation.Token);
                    if (!isKeep())
                    {
                        break;
                    }
                    Send("ping\r\n");
                }
                catch (TaskCanceledException)
                {

                }
                catch (DisconnectException ex)
                {
                    OnError?.Invoke(new ErrorEventArgs(LogType.Warining, $"Send heartbeat ping failed. {ex.Message}"));
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(new ErrorEventArgs(LogType.Warining, $"Send heartbeat ping failed. {ex.Message}", ex));
                }
            }
        }

        #endregion

        #region private

        private bool TryRequest(CommandType commandType, string data, out string result)
        {
            result = null;
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("Not connected to server");
            }
            lock (_sendLocker)
            {
                if (_cmdResultStatusDic.ContainsKey(commandType) && !_cmdResultStatusDic.TryRemove(commandType, out _))
                {
                    throw new Exception("Can not remove old command type status from status dictionary.");
                }
                string spinResult = null;
                long timestamp = GetMilliTimestamp();
                int sendData = _client.Connection.Send(data, Encoding.UTF8);
                bool isSuccess = SpinWait.SpinUntil(() =>
                {
                    if (_cmdResultStatusDic.TryGetValue(commandType, out var cmdResult) && cmdResult.Item1 > timestamp)
                    {
                        spinResult = cmdResult.Item2;
                        return true;
                    }
                    return false;
                }, TimeSpan.FromMilliseconds(3000));
                if (isSuccess)
                {
                    result = spinResult;
                    return true;
                }
                return false;
            }
        }

        private void Send(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            if (!this.IsConnected)
            {
                throw new DisconnectException("Not connected to server");
            }
            lock (_sendLocker)
            {
                _client.Connection.Send(data, Encoding.UTF8);
            }
        }

        private long GetMilliTimestamp()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        private Dictionary<string, string> GetResultParamValues(string result)
        {
            Dictionary<string, string> resultDic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(result))
            {
                return resultDic;
            }
            string[] resultArgs = result?.Split('&');
            if (resultArgs?.Any() != true)
            {
                return resultDic;
            }
            if (resultArgs.Length == 1 && (DateTime.TryParse(result, out _) || (long.TryParse(result, out long seconds) && seconds > 1672502400 && seconds <= 4102415999)))
            {
                //有可能是获取当前时间，判断一下
                result = $"cmd=7&msg={result}";
                resultArgs = result.Split('&');
            }
            string lastKey = null;
            foreach (string argItem in resultArgs)
            {
                string[] args = argItem.Split('=');
                if (args.Length < 2)
                {
                    resultDic[lastKey] = resultDic.TryGetValue(lastKey, out string oldValue) ? $"{oldValue}&{args[0]}" : $"&{args[0]}";
                }
                else
                {
                    lastKey = args[0].ToLower();
                    resultDic[lastKey] = string.Join("=", args, 1, args.Length - 1);
                }
            }
            return resultDic;
        }

        private string CommandBuilder(CommandType type, string topic, string msg, DateTimeType timeType = DateTimeType.DateTime)
        {
            List<string> args = new List<string>(10)
            {
                $"cmd={(int)type}", $"uid={this.Builder.Secret}"
            };
            switch (type)
            {
                case CommandType.Subscribe:
                case CommandType.SubAndPull:
                case CommandType.TestamentMsg:
                    {
                        if (string.IsNullOrWhiteSpace(topic))
                            throw new ArgumentNullException(nameof(topic), "The topic can not null.");
                        args.Add($"topic={topic}");
                    }
                    break;
                case CommandType.Publish:
                    {
                        if (string.IsNullOrWhiteSpace(topic))
                            throw new ArgumentNullException(nameof(topic), "The topic can not null.");
                        if (string.IsNullOrWhiteSpace(msg))
                            throw new ArgumentNullException(nameof(msg), "The msg can not null.");
                        args.Add($"topic={topic}");
                        args.Add($"msg={msg}");
                    }
                    break;
                case CommandType.GetTimestamp:
                    {
                        args.Add($"type={(int)timeType}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unknow command type: {type}");
            }
            string data = string.Join('&', args) + "\r\n";
            return data;
        }

        #endregion

    }
}
