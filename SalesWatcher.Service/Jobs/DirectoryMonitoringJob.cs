using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesWatcher.Service.Jobs
{
    [DisallowConcurrentExecution]
    public class DirectoryMonitoringJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dirToWatch = context.JobDetail.JobDataMap.GetString("INPUT_DIR_PATH");
            var outputDirPath = context.JobDetail.JobDataMap.GetString("OUTPUT_DIR_PATH");
            var completedPath = context.JobDetail.JobDataMap.GetString("COMPLETED_DIR_PATH");

            var allFiles = await GetReportPathsToProcess(context, Directory.GetFiles(dirToWatch));

            foreach (var path in allFiles)
            {
                await TriggerJob(context, path, outputDirPath, completedPath); 
            }
        }

        /// <summary>
        /// Verifica se está executando o processamento do relatório, caso positivo remove o caminho da lista de gatilhos.
        /// </summary>
        /// <param name="context">Contexto de execução do Job</param>
        /// <param name="allFiles">Caminhos para verificar se já estão executando.</param>
        /// <returns>Retorna a lista de caminhos de relatório que ainda não foram executados.</returns>
        private async Task<string[]> GetReportPathsToProcess(IJobExecutionContext context, string[] allFiles)
        {
            var runningJobs = await context.Scheduler.GetCurrentlyExecutingJobs();
            var processReportFileJobs = runningJobs.Where(job => job.JobInstance is ProcessReportFileJob).ToArray();
            var reportPathsRunning = processReportFileJobs.Select(ctx => ctx.Trigger.JobDataMap.GetString("INPUT_PATH")).ToArray();

            allFiles = allFiles.Where(path => !reportPathsRunning.Contains(path, StringComparer.OrdinalIgnoreCase)).ToArray();
            return allFiles;
        }

        private async Task TriggerJob(IJobExecutionContext context, string path, string outputDirPath, string completedDirPath)
        {
            var jobKey = new JobKey("process-report-file", "sales-watcher");

            var dm = new JobDataMap();
            dm.Put("INPUT_PATH", path);
            dm.Put("OUTPUT_DIR_PATH", outputDirPath);
            dm.Put("COMPLETED_DIR_PATH", completedDirPath);

            await context.Scheduler.TriggerJob(jobKey, dm);
        }
    }
}
