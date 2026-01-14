using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Text;

namespace Tools.Common
{
    public class SerialPortService : IDisposable
    {
        private readonly ILogger<SerialPortService> _logger;
        private SerialPort? _serialPort;
        private CancellationTokenSource? _cts;

        private readonly string _name;
        private bool _isReconnecting = false;

        private string? _portName;
        private int _baudRate;
        private Parity _parity;
        private int _dataBits;
        private StopBits _stopBits;

        private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(1);

        public bool IsOpen => _serialPort?.IsOpen ?? false;

        public event Action<string>? OnMessageReceived;
        public event Action<bool>? OnIsConnected;

        public SerialPortService(string name, ILogger<SerialPortService> logger)
        {
            _name = name;
            _logger = logger;
        }

        #region ========== 打开串口 ==========

        public async Task OpenAsync(
            string portName,
            int baudRate,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;

            _isReconnecting = false;

            _ = TryOpenAsync();
            await Task.CompletedTask;
        }

        #endregion

        #region ========== 打开与重连逻辑 ==========

        private async Task TryOpenAsync()
        {
            int retry = 0;

            while (true)
            {
                try
                {
                    _serialPort = CreateSerialPort();
                    _serialPort.Open();

                    _cts = new CancellationTokenSource();

                    _logger.LogInformation(
                        "✅ 串口已打开 [{Name}] {Port} @ {Baud}",
                        _name, _portName, _baudRate);

                    OnIsConnected?.Invoke(IsOpen);

                    _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
                    return;
                }
                catch (Exception ex)
                {
                    retry++;
                    _logger.LogWarning(
                        ex,
                        "⚠️ 第 {Retry} 次打开串口失败，将在 {Delay}s 后重试",
                        retry,
                        _reconnectDelay.TotalSeconds);

                    await Task.Delay(_reconnectDelay);
                }
            }
        }

        private SerialPort CreateSerialPort()
        {
            return new SerialPort(_portName!, _baudRate, _parity, _dataBits, _stopBits)
            {
                Encoding = Encoding.ASCII,
                ReadTimeout = 3000,
                WriteTimeout = 3000
            };
        }

        #endregion

        #region ========== 接收循环 ==========

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                var buffer = new byte[1024];

                while (!token.IsCancellationRequested && _serialPort != null)
                {
                    int bytesRead = await _serialPort.BaseStream.ReadAsync(
                        buffer, 0, buffer.Length, token);

                    if (bytesRead <= 0)
                        continue;

                    string msgHex = BitConverter.ToString(buffer, 0, bytesRead);

                    _logger.LogInformation(
                        "📥 串口[{Name}] 接收: {Message}",
                        _name, msgHex);

                    OnMessageReceived?.Invoke(msgHex);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "❌ 串口接收异常");
                await HandleReconnectAsync();
            }
        }

        #endregion

        #region ========== 自动重连 ==========

        private async Task HandleReconnectAsync()
        {
            if (_isReconnecting)
                return;

            _isReconnecting = true;

            _logger.LogWarning("🔄 串口[{Name}] 断开，启动自动重连", _name);
            OnIsConnected?.Invoke(IsOpen);

            DisconnectInternal();

            int attempt = 0;
            while (_cts?.IsCancellationRequested ?? true)
            {
                try
                {
                    attempt++;

                    _serialPort = CreateSerialPort();
                    _serialPort.Open();

                    _cts = new CancellationTokenSource();

                    _logger.LogInformation(
                        "✅ 串口[{Name}] 重连成功 ({Attempt})",
                        _name, attempt);

                    OnIsConnected?.Invoke(IsOpen);

                    _isReconnecting = false;

                    _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        "🔁 串口[{Name}] 重连失败 ({Attempt}): {Message}",
                        _name, attempt, ex.Message);

                    await Task.Delay(_reconnectDelay);
                }
            }
        }

        #endregion

        #region ========== 发送数据 ==========

        public async Task SendAsync(string message)
        {
            if (_serialPort == null || !IsOpen)
                throw new InvalidOperationException("串口未打开");

            var buffer = Encoding.ASCII.GetBytes(message);
            await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length);

            _logger.LogInformation(
                "📤 串口[{Name}] 发送: {Message}",
                _name, message);
        }

        public async Task SendAsync(byte[] message)
        {
            if (_serialPort == null || !IsOpen)
                throw new InvalidOperationException("串口未打开");

            await _serialPort.BaseStream.WriteAsync(message, 0, message.Length);

            _logger.LogInformation(
                "📤 串口[{Name}] 发送字节流: {Message}",
                _name, BitConverter.ToString(message));
        }

        #endregion

        #region ========== 关闭 / 释放 ==========

        private void DisconnectInternal()
        {
            try
            {
                _cts?.Cancel();
                _serialPort?.Close();
                _serialPort?.Dispose();
            }
            catch
            {
                // 忽略释放异常
            }
        }

        public void Dispose()
        {
            DisconnectInternal();
        }

        #endregion
    }
}
