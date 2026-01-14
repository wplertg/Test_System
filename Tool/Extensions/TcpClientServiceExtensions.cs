using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tools.Common;

namespace Tools.Extensions
{
    public static class TcpClientServiceExtensions
    {
        public static IServiceCollection AddTcpClients(this IServiceCollection services)
        {
            services.AddSingleton<Func<string, TcpClientService>>(sp =>
            {
                return BuildTcpClientFactory(sp);
            });

            return services;
        }

        private static Func<string, TcpClientService> BuildTcpClientFactory(IServiceProvider sp)
        {
            // 单例缓存（按 name）
            var cache = new Dictionary<string, TcpClientService>();
            var logger = sp.GetRequiredService<ILogger<TcpClientService>>();

            return name =>
            {
                if (cache.TryGetValue(name, out var existing))
                {
                    return existing;
                }

                TcpClientService instance = new TcpClientService(name, logger);

                //name switch
                //{
                //    "CE1" => new TcpClientService("CE1", logger),
                //    "CE2" => new TcpClientService("CE2", logger),
                //    _ => throw new ArgumentException($"未知的 TCP 客户端标识: {name}")
                //};

                cache[name] = instance;
                return instance;
            };
        }
    }
}
