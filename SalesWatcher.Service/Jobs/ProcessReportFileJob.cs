using Quartz;
using SalesWatcher.Business.Reports;
using SalesWatcher.Business.Reports.SalesReport;
using System.IO;
using System.Threading.Tasks;

namespace SalesWatcher.Service.Jobs
{
    public class ProcessReportFileJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var inputPath = context.Trigger.JobDataMap.GetString("INPUT_PATH");
            var outputDirPath = context.Trigger.JobDataMap.GetString("OUTPUT_DIR_PATH");
            var completedDirPath = context.Trigger.JobDataMap.GetString("COMPLETED_DIR_PATH");

            var fileName = Path.GetFileName(inputPath);

            var outputPath = Path.Combine(outputDirPath, fileName);
            var completedPath = Path.Combine(completedDirPath, fileName);

            using (var fileStream = File.OpenRead(inputPath))
            {
                var salesReportCsv = new SalesReportCsvReader(fileStream, "ç");
                salesReportCsv.Process();

                var summary = new SalesReportCsvWriter(salesReportCsv);
                using (var fs = File.OpenWrite(outputPath))
                {
                    summary.Save(fs);
                }
            }

            File.Move(inputPath, completedPath);
        }
    }
}
