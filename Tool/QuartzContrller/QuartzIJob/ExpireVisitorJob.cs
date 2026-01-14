using Microsoft.Extensions.Logging;
using Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.QuartzContrller.QuartzIJob
{
    public class ExpireVisitorJob : IJob
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ExpireVisitorJob> _logger;

        public ExpireVisitorJob(
            AppDbContext db,
            ILogger<ExpireVisitorJob> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // 你的业务逻辑
            _logger.LogInformation($"定时任务ExpireVisitorJob{DateTime.Now}触发开始执行");
        }
    }
}
