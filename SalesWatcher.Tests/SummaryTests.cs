using Microsoft.VisualStudio.TestTools.UnitTesting;
using SalesWatcher.Business.Reports;
using SalesWatcher.Business.Reports.SalesReport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SalesWatcher.Tests
{
    [TestClass]
    public class SummaryTests
    {
        public (string, SalesReportCsvWriter) GetSaveText(string text)
        {
            var ish = new SalesReportCsvReader(TestUtils.GetStreamFromString(text), "ç");
            ish.Process();

            var summary = new SalesReportCsvWriter(ish, ";");

            var outputStream = new MemoryStream();
            summary.Save(outputStream);

            using (var sr = new StreamReader(new MemoryStream(outputStream.ToArray())))
            {
                sr.ReadLine();
                return (sr.ReadLine(), summary);
            }
        }

        [TestMethod]
        public void WritingSuccessfully()
        {
            {
                var inputStream = @"001ç1234567891234çPedroç50000
001ç3245678865434çPauloç40000.99
002ç2345675434544345çJose da SilvaçRural
002ç2345675433444345çEduardo PereiraçRural
003ç10ç[1-10-100,2-30-2.50,3-40-3.10]çPedro
003ç08ç[1-34-10,2-33-1.50,3-40-0.10]çPaulo";

                var (text, _) = GetSaveText(inputStream);

                // QtdClientes;QtdVendedores;IdVendaMaisCara;PiorVendedor
                Assert.AreEqual("2;2;10;Paulo", text);
            }

            {
                var inputStream = @"001ç1234567891234çPedroç50000
001ç3245678865434çPauloç40000.99
001ç3245678866322çJonnyç69000.67
002ç2345675434544345çJose da SilvaçRural
002ç2345675433444345çEduardo PereiraçRural
003ç10ç[1-10-100,2-30-2.50,3-40-3.10]çPedro
003ç08ç[1-34-10,2-33-1.50,3-40-0.10]çPaulo
003ç95ç[1-34-10,2-33-1.49,3-40-0.10]çJonny
";

                var (text, _) = GetSaveText(inputStream);

                // QtdClientes;QtdVendedores;IdVendaMaisCara;PiorVendedor
                Assert.AreEqual("2;3;10;Jonny", text);
            }
        }

        [TestMethod]
        public void Calculation()
        {
            {
                var inputStream = @"001ç1234567891234çPedroç50000
001ç3245678865434çPauloç40000.99
002ç2345675434544345çJose da SilvaçRural
002ç2345675433444345çEduardo PereiraçRural
003ç10ç[1-10-100,2-30-2.50,3-40-3.10]çPedro
003ç08ç[1-34-10,2-33-1.50,3-40-0.10]çPaulo";

                var (_, summary) = GetSaveText(inputStream);

                // QtdClientes;QtdVendedores;IdVendaMaisCara;PiorVendedor
                Assert.AreEqual(2, summary.QtdClientes);
                Assert.AreEqual(2, summary.QtdVendedores);
                Assert.AreEqual(10, summary.IdVendaMaisCara);
                Assert.AreEqual("Paulo", summary.PiorVendedor);
            }

            {
                var inputStream = @"001ç1234567891234çPedroç50000
001ç3245678865434çPauloç40000.99
001ç3245678866322çJonnyç69000.67
002ç2345675434544345çJose da SilvaçRural
002ç2345675433444345çEduardo PereiraçRural
003ç10ç[1-10-100,2-30-2.50,3-40-3.10]çPedro
003ç08ç[1-34-10,2-33-1.50,3-40-0.10]çPaulo
003ç95ç[1-34-10,2-33-1.49,3-40-0.10]çJonny
";

                var (_, summary) = GetSaveText(inputStream);

                Assert.AreEqual(2, summary.QtdClientes);
                Assert.AreEqual(3, summary.QtdVendedores);
                Assert.AreEqual(10, summary.IdVendaMaisCara);
                Assert.AreEqual("Jonny", summary.PiorVendedor);
            }
        }
    }
}
