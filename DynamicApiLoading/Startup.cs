using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DynamicApiLoading.Quartz;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;

namespace DynamicApiLoading
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .SetCompatibilityVersion(version: CompatibilityVersion.Version_2_2);

            services.AddSingleton<IActionDescriptorChangeProvider>(MyActionDescriptorChangeProvider.Instance);
            services.AddSingleton(MyActionDescriptorChangeProvider.Instance);


            services.UseQuartz(typeof(FindAndLoadApiJob), typeof(FindAndLoadApiJob));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              IHostingEnvironment env,
                              IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            var scheduler = (IScheduler)app.ApplicationServices.GetService(typeof(IScheduler));
            var job = JobBuilder.Create<FindAndLoadApiJob>()
                                .Build();
            var jobTrigger = TriggerBuilder.Create()
                                           .WithIdentity("Trigger")
                                           .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(10))
                                           .StartAt(DateTimeOffset.UtcNow.AddSeconds(30))
                                           .Build();
            scheduler.ScheduleJob(job,
                                  jobTrigger)
                     .Wait();

            lifetime.ApplicationStarted.Register(() => scheduler.Start().GetAwaiter().GetResult());
            lifetime.ApplicationStopping.Register(() => scheduler.Shutdown().GetAwaiter().GetResult());
        }
    }
}