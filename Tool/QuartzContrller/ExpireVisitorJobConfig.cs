using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.QuartzContrller.QuartzIJob;

namespace Tools.QuartzContrller
{
    public static class ExpireVisitorJobConfig
    {
        public static readonly JobKey JobKey =
            new JobKey("ExpireVisitorJob");

        public static void Configure(IServiceCollectionQuartzConfigurator q)
        {
            q.AddJob<ExpireVisitorJob>(opts =>
                opts.WithIdentity(JobKey));

            q.AddTrigger(opts => opts
                .ForJob(JobKey)
                .WithIdentity("ExpireVisitorJob-trigger")
                .WithCronSchedule("0 */5 * * * ?")); // 每5分钟
        }
    }
}
