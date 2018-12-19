using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace AspNetCoreTodo.Jobs
{
    // Responsible for starting and gracefully stopping the scheduler.
    public class QuartzStartup
    {
        private IScheduler _scheduler; // after Start, and until shutdown completes, references the scheduler object

        // starts the scheduler, defines the jobs and the triggers
        public void Start()
        {
            if (_scheduler != null)
            {
                throw new InvalidOperationException("Already started.");
            }

            var properties = new NameValueCollection
            {
                // json serialization is the one supported under .NET Core (binary isn't)
                ["quartz.serializer.type"] = "json"

            };

            var schedulerFactory = new StdSchedulerFactory(properties);
            _scheduler = schedulerFactory.GetScheduler().Result;
            _scheduler.Start();

            var userEmailsJob = JobBuilder.Create<ExpiredTaskJob>()
                .WithIdentity("SendUserEmailsForExpiredTasks")
                .Build();
            var userEmailsTrigger = TriggerBuilder.Create()
                .WithIdentity("UserEmailsCron")
                .StartNow() //WithCronSchedule("0 25 23 ? * *")
                .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(60)
                        .RepeatForever())
                         .Build();

            _scheduler.ScheduleJob(userEmailsJob, userEmailsTrigger);
        }

        // initiates shutdown of the scheduler, and waits until jobs exit gracefully (within allotted timeout)
        public void Stop()
        {
            if (_scheduler == null)
            {
                return;
            }

            // give running jobs 30 sec (for example) to stop gracefully
            if (_scheduler.Shutdown(waitForJobsToComplete: true).Wait(30000))
            {
                _scheduler = null;
            }
            else
            {
                // jobs didn't exit in timely fashion - log a warning...
            }
        }
    }
}