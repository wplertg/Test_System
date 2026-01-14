using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tools.Common;

namespace Tools.Extensions
{
    public static class SerialPortServiceExtensions
    {
        public static IServiceCollection AddSerialPorts(this IServiceCollection services)
        {
            services.AddSingleton<Func<string, SerialPortService>>(sp =>
            {
                return BuildSerialPortServiceFactory(sp);
            });

            return services;
        }

        private static Func<string, SerialPortService> BuildSerialPortServiceFactory(IServiceProvider sp)
        {
            var cache = new Dictionary<string, SerialPortService>();
            var logger = sp.GetRequiredService<ILogger<SerialPortService>>();

            return name =>
            {
                if (cache.TryGetValue(name, out var existing))
                {
                    return existing;
                }

                SerialPortService instance = name switch
                {
                    "COM1" => new SerialPortService("COM1", logger),
                    "COM2" => new SerialPortService("COM2", logger),
                    _ => throw new ArgumentException($"未知的串口标识: {name}")
                };

                cache[name] = instance;
                return instance;
            };
        }
    }
}
