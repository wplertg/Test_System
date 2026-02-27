using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tools.middleware
{
    public class IpWhitelistAttribute : ActionFilterAttribute
    {
        //[IpWhitelist("127.0.0.1,10.0.0.5")] 只有 127.0.0.1 和 公司内网IP 10.0.0.5 能访问此接口

        /*
         builder.Services.AddControllers(options =>
        {
            // 全局添加过滤器，所有接口都只允许这些 IP 访问
            options.Filters.Add(new IpWhitelistAttribute("127.0.0.1,192.168.0.100"));
        });
         */

        private readonly string[] _allowedIps;
        private readonly ILogger<IpWhitelistAttribute> _logger;
        public IpWhitelistAttribute(string allowedIps, ILogger<IpWhitelistAttribute> logger)
        {
            // 允许传入逗号分隔的 IP 字符串，例如 "127.0.0.1,192.168.1.5"
            _allowedIps = allowedIps.Split(',');
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. 获取中间件处理后的真实 IP
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;

            // 处理本地回环地址 (IPv6 localhost)
            string ipAddress = remoteIp?.ToString();
            if (ipAddress == "::1") ipAddress = "127.0.0.1";
            _logger.LogInformation("访问IP:{0}", ipAddress);
            //ipAddress
            // 2. 检查 IP 是否存在
            if (string.IsNullOrEmpty(ipAddress))
            {
                context.Result = new BadRequestObjectResult("无法获取客户端 IP");
                return;
            }

            // 3. 校验白名单
            // 注意：这里是简单的字符串比对。生产环境可能需要处理 CIDR (子网掩码)
            bool isAllowed = _allowedIps.Any(ip => ip.Trim() == ipAddress);

            if (!isAllowed)
            {
                // 记录非法访问日志 (建议使用 ILogger)
                Console.WriteLine($"[非法访问拦截] IP: {ipAddress} 试图访问 {context.ActionDescriptor.DisplayName}");

                // 4. 直接短路管道，返回 403 Forbidden
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
