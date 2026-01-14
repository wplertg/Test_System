using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Tools.Common
{
    public class TcpClientService : IDisposable
    {
        private readonly ILogger<TcpClientService> _logger;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private string? _host;
        private int _port;
        private bool _isReconnecting = false;

        private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(1); // 重连间隔

        private readonly string _name;
        
        public bool IsConnected => _client?.Connected ?? false;

        public event Action<string>? OnMessageReceived;
        public event Action<bool>? OnIsConnected;

        public TcpClientService(string name, ILogger<TcpClientService> logger)
        {
            _name = name;
            _logger = logger;
        }

        // 主动连接方法
        public async Task ConnectAsync(string host, int port)
        {
            _host = host;
            _port = port;
            _isReconnecting = false;
            
            _ = TryConnectAsync();
        }
        /// <summary>
        /// 连接尝试与重连逻辑
        /// </summary>
        /// <returns></returns>
        private async Task TryConnectAsync()
        {
            int retry = 0;
            while (true)
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(_host!, _port);
                    _stream = _client.GetStream();
                    _cts = new CancellationTokenSource();

                    _logger.LogInformation("✅ TCP 已连接 {Host}:{Port}", _host, _port);

                    _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
                    return;
                }
                catch (Exception ex)
                {

                    _logger.LogWarning(ex, "⚠️ 第 {Retry}次连接失败，将在 {Delay}s 后重试", retry, _reconnectDelay.TotalSeconds);
                    retry++;
                    await Task.Delay(_reconnectDelay);
                }
            }
        }
        /// <summary>
        /// 读取线程
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                var buffer = new byte[1024];
                while (!token.IsCancellationRequested && _stream != null)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0)
                    {
                        _logger.LogWarning("⚠️ TCP 连接断开（远程关闭）");
                        await HandleReconnectAsync();
                        break;
                    }

                    string msgHex = BitConverter.ToString(buffer, 0, bytesRead);
                    _logger.LogInformation("接收消息: {Message}", msgHex);

                    
                    OnMessageReceived?.Invoke(msgHex);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "TCP 接收循环异常");
                await HandleReconnectAsync();
            }
        }
        /// <summary>
        /// 自动重连逻辑
        /// </summary>
        /// <returns></returns>
        private async Task HandleReconnectAsync()
        {
            if (_isReconnecting)
                return;

            _isReconnecting = true;

            _logger.LogWarning("🔄 检测到断线，启动自动重连逻辑");
            OnIsConnected?.Invoke(IsConnected);
            DisconnectInternal();

            int attempt = 0;
            while (_cts?.IsCancellationRequested ?? true)
            {
                try
                {
                    attempt++;
                    _client = new TcpClient();
                    await _client.ConnectAsync(_host!, _port);
                    _stream = _client.GetStream();
                    _cts = new CancellationTokenSource();

                    _logger.LogInformation("✅ 重新连接成功 ({Attempt}) {Host}:{Port}", attempt, _host, _port);
                    OnIsConnected?.Invoke(IsConnected);
                    _isReconnecting = false;
                    _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("🔁 重连失败 ({Attempt})：{Message}", attempt, ex.Message);
                    await Task.Delay(_reconnectDelay);
                }
            }
        }
        /// <summary>
        /// 发送字符串消息 UTF8
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendAsync(string message)
        {
            if (_stream == null || !IsConnected)
                throw new InvalidOperationException("未连接 TCP 服务器");

            var buffer = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(buffer, 0, buffer.Length);
            _logger.LogInformation("发送消息: {Message}", message);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendAsync(byte[] message)
        {
            if (_stream == null || !IsConnected)
                throw new InvalidOperationException("未连接 TCP 服务器");

            await _stream.WriteAsync(message, 0, message.Length);
            _logger.LogInformation("发送字节流: {Message}", BitConverter.ToString(message));
        }
        private void DisconnectInternal()
        {
            try
            {
                _cts?.Cancel();
                _stream?.Close();
                _client?.Close();
            }
            catch { /* 忽略清理异常 */ }
        }

        public void Dispose()=>
            DisconnectInternal();
    }
}
