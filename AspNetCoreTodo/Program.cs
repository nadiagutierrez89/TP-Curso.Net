using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using AspNetCoreTodo.Jobs;
using System.Threading;
using System.IO;

namespace AspNetCoreTodo
{
    public class Program
    {
        private static IScheduler _scheduler;
        public static void Main(string[] args)
        {
            QuartzStartup quartz = new  QuartzStartup();
            var host = BuildWebHost(args);
            InitializeDatabase(host);
            host.Run();
        }

        private static void SendMailsForExpiredTasks(Object obj)
        {
            var host = (IWebHost)obj;

        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .UseApplicationInsights()
            .Build();

        private static void InitializeDatabase(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    SeedData.InitializeAsync(services).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error occurred seeding the DB.");
                }
            }
        }

        #region Fúncion para el Quartz
        private static async Task SendEmailJob()
        {
            try
            {// Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                Console.WriteLine("------- Initializing ----------------------");

                // First we must get a reference to a scheduler
                ISchedulerFactory sf = new StdSchedulerFactory();
                _scheduler = sf.GetScheduler().Result;

                Console.WriteLine("------- Initialization Complete -----------");


                // computer a time that is on the next round minute
                DateTime runTime = DateTime.Now.AddMinutes(0.5).ToLocalTime();

                Console.WriteLine("------- Scheduling Job  -------------------");

                // define the job and tie it to our ExpiredTaskJob class
                IJobDetail job = JobBuilder.Create<ExpiredTaskJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

                // Trigger the job to run now, and then repeat every 120 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartAt(runTime)
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1200)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await _scheduler.ScheduleJob(job, trigger);
                Console.WriteLine($"{job.Key} will run at: {runTime:r}");

                // Start up the scheduler (nothing can actually run until the
                // scheduler has been started)
                await _scheduler.Start();
                Console.WriteLine("------- Started Scheduler -----------------");

                // wait long enough so that the scheduler as an opportunity to
                // run the job!
                //Console.WriteLine("------- Waiting 65 seconds... -------------");

                // wait 65 seconds to show jobs
                await Task.Delay(TimeSpan.FromSeconds(100));
                // and last shut down the scheduler when you are ready to close your program
                //await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }

        #endregion

    }
}