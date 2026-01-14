using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Tools.QuartzContrller;

namespace Tools.Extensions
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddVisitorQuartzJobs(
            this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                //q.UseMicrosoftDependencyInjectionJobFactory();

                ExpireVisitorJobConfig.Configure(q);
            });

            services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
