using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using SalesWatcher.Service.Jobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SalesWatcher.Tests
{
    [TestClass]
    public class ProcessReportFileJobTests
    {
        [TestMethod]
        public void BasicTest()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "service-testing");

            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            var INPUT_DIR_PATH = Path.Combine(tempDir, @"in");
            var OUTPUT_DIR_PATH = Path.Combine(tempDir, "out");
            var COMPLETED_DIR_PATH = Path.Combine(tempDir, "completed");

            Directory.CreateDirectory(INPUT_DIR_PATH);
            Directory.CreateDirectory(OUTPUT_DIR_PATH);
            Directory.CreateDirectory(COMPLETED_DIR_PATH);

            try
            {
                var INPUT_PATH = Path.Combine(INPUT_DIR_PATH, @"test.csv");

                var job = new ProcessReportFileJob();
                var context = new Mock<IJobExecutionContext>();
                var trigger = new Mock<ITrigger>();

                context.Setup(_ => _.Trigger).Returns(trigger.Object);

                trigger.Setup(_ => _.JobDataMap.GetString("INPUT_PATH")).Returns(INPUT_PATH);
                trigger.Setup(_ => _.JobDataMap.GetString("OUTPUT_DIR_PATH")).Returns(OUTPUT_DIR_PATH);
                trigger.Setup(_ => _.JobDataMap.GetString("COMPLETED_DIR_PATH")).Returns(COMPLETED_DIR_PATH);

                var testStr1 = @"001ç1234567891234çPedroç50000
001ç3245678865434çPauloç40000.99
001ç3245678866322çJonnyç69000.67
002ç2345675434544345çJose da SilvaçRural
002ç2345675433444345çEduardo PereiraçRural
003ç10ç[1-10-100,2-30-2.50,3-40-3.10]çPedro
003ç08ç[1-34-10,2-33-1.50,3-40-0.10]çPaulo
003ç95ç[1-34-10,2-33-1.49,3-40-0.10]çJonny";

                File.WriteAllText(INPUT_PATH, testStr1);

                job.Execute(context.Object).Wait();

                var fileName = Path.GetFileName(INPUT_PATH);
                var outputPath = Path.Combine(OUTPUT_DIR_PATH, fileName);

                Assert.AreEqual(true, File.Exists(outputPath));

            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
