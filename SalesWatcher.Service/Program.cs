using Quartz;
using Quartz.Impl;
using SalesWatcher.Service.Jobs;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace SalesWatcher.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            StartService().Wait();

            // loop
            while (true)
                Task.Delay(1000).Wait();
        }

        static async Task StartService()
        {
            // construct a scheduler factory
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);

            IScheduler sched = await factory.GetScheduler();
            await sched.Start();

            var inputDirPath = @"c:\sales-watcher\in";
            var outputDirPath = @"c:\sales-watcher\out";
            var completedDirPath = @"c:\sales-watcher\completed";
            int directoryMonitoringIntervalSeconds = 1;

            var homePath = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH");

            inputDirPath = Path.Combine(homePath, "in");
            outputDirPath = Path.Combine(homePath, "out");
            completedDirPath = Path.Combine(homePath, "completed");

            System.IO.Directory.CreateDirectory(inputDirPath);
            System.IO.Directory.CreateDirectory(outputDirPath);
            System.IO.Directory.CreateDirectory(completedDirPath);

            // define the job and tie it to our HelloJob class
            IJobDetail jobDirectoryMonitoring = JobBuilder.Create<DirectoryMonitoringJob>()
                .WithIdentity("directory-monitoring", "sales-watcher")
                .UsingJobData("INPUT_DIR_PATH", inputDirPath)
                .UsingJobData("OUTPUT_DIR_PATH", outputDirPath)
                .UsingJobData("COMPLETED_DIR_PATH", completedDirPath)
                .Build();

            IJobDetail jobProcessReportFile = JobBuilder.Create<ProcessReportFileJob>()
                .WithIdentity("process-report-file", "sales-watcher")
                .StoreDurably()
                .Build();

            ITrigger triggerDirectoryMonitoring = TriggerBuilder.Create()
                .WithIdentity("directory-monitoring-trigger", "sales-watcher")
                .StartNow()
                .WithSimpleSchedule(x =>
                    x.WithIntervalInSeconds(directoryMonitoringIntervalSeconds)
                    .RepeatForever())
            .Build();

            await sched.ScheduleJob(jobDirectoryMonitoring, triggerDirectoryMonitoring);
            await sched.AddJob(jobProcessReportFile, true);

            Console.WriteLine($"Monitorando diretórios:\n\t{@inputDirPath}\n\t{@outputDirPath}");
        }
    }
}
