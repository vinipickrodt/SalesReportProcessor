using Microsoft.VisualStudio.TestTools.UnitTesting;
using SalesWatcher.Business;
using SalesWatcher.Business.CsvModels;
using SalesWatcher.Business.Reports;
using System;
using System.IO;
using System.Text;

namespace SalesWatcher.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void SingleItemParse()
        {
            var ms = TestUtils.GetStreamFromString("001;98765432100;John;652500.58");

            var ish = new SalesReportCsvReader(ms, ";");
            ish.Process();

            Assert.AreEqual(1, ish.Result.Length);

            var john = ish.Result[0] as Seller;
            Assert.AreEqual("98765432100", john.CPF);
            Assert.AreEqual("John", john.Name);
            Assert.AreEqual(652500.58M, john.Salary);
        }

        [TestMethod]
        public void MultipleItemsParse()
        {
            var ms = TestUtils.GetStreamFromString(@"001Á1234567891234ÁPedroÁ50000
001Á3245678865434ÁPauloÁ40000.99
002Á2345675434544345ÁJose da SilvaÁRural
002Á2345675433444345ÁEduardo PereiraÁRural
003Á10Á[1-10-100,2-30-2.50,3-40-3.10]ÁPedro
003Á08Á[1-34-10,2-33-1.50,3-40-0.10]ÁPaulo");

            var ish = new SalesReportCsvReader(ms, "Á");
            ish.Process();

            Assert.AreEqual(6, ish.Result.Length);

            Assert.AreEqual(2, ish.Sellers.Count);
            Assert.AreEqual(2, ish.Customers.Count);
            Assert.AreEqual(2, ish.Sales.Count);
        }

        [TestMethod]
        public void AllValuesParsedCorrectly()
        {
            var ms = TestUtils.GetStreamFromString(@"001Á3245678865434ÁPauloÁ40000.99
002Á2345675434544345ÁJose da SilvaÁRural
003Á08Á[1-34-10,2-33-1.50,3-40-0.10]ÁPaulo");

            var ish = new SalesReportCsvReader(ms, "Á");
            ish.Process();

            Assert.AreEqual(3, ish.Result.Length);

            var sellerPaulo = ish.Sellers[0] as Seller;
            var customerJose = ish.Customers[0] as Customer;
            var sale_08 = ish.Sales[0] as Sale;

            Assert.AreEqual("3245678865434", sellerPaulo.CPF);
            Assert.AreEqual("Paulo", sellerPaulo.Name);
            Assert.AreEqual(40000.99M, sellerPaulo.Salary);

            Assert.AreEqual("2345675434544345", customerJose.CNPJ);
            Assert.AreEqual("Jose da Silva", customerJose.Name);
            Assert.AreEqual("Rural", customerJose.BusinessArea);

            Assert.AreEqual(8, sale_08.SaleId);
            Assert.AreEqual("Paulo", sale_08.SoldBy);
            Assert.AreEqual(3, sale_08.SoldItems.Count);

            {
                var sale_08_item1 = sale_08.SoldItems[0] as ItemSold;

                Assert.AreEqual(1, sale_08_item1.Id);
                Assert.AreEqual(34, sale_08_item1.Quantity);
                Assert.AreEqual(10M, sale_08_item1.Price);
            }

            {
                var sale_08_item2 = sale_08.SoldItems[1] as ItemSold;

                Assert.AreEqual(2, sale_08_item2.Id);
                Assert.AreEqual(33, sale_08_item2.Quantity);
                Assert.AreEqual(1.5M, sale_08_item2.Price);
            }

            {
                var sale_08_item3 = sale_08.SoldItems[2] as ItemSold;

                Assert.AreEqual(3, sale_08_item3.Id);
                Assert.AreEqual(40, sale_08_item3.Quantity);
                Assert.AreEqual(0.1M, sale_08_item3.Price);
            }
        }

        [TestMethod]
        public void EmptyItemsParsedCorrectly()
        {
            var stringsToTest = new string[] { "003Á08Á[]ÁPaulo da Silva Santos Oliveira Garcia Ramos Pereira Borba Guimar„es",
                "003Á08ÁÁPaulo da Silva Santos Oliveira Garcia Ramos Pereira Borba Guimar„es",
                "003Á08Á[ ]ÁPaulo da Silva Santos Oliveira Garcia Ramos Pereira Borba Guimar„es",
                "003Á08Á[     \t\t\t\t     ]ÁPaulo da Silva Santos Oliveira Garcia Ramos Pereira Borba Guimar„es",
            };

            foreach (var stringToTest in stringsToTest)
            {
                var ms = TestUtils.GetStreamFromString(stringToTest);

                var ish = new SalesReportCsvReader(ms, "Á");
                ish.Process();

                Assert.AreEqual(1, ish.Result.Length, stringToTest);

                var sale_08 = ish.Result[0] as Sale;
                Assert.AreEqual(8, sale_08.SaleId, stringToTest);
                Assert.AreEqual("Paulo da Silva Santos Oliveira Garcia Ramos Pereira Borba Guimar„es", sale_08.SoldBy, stringToTest);
                Assert.AreEqual(0, sale_08.SoldItems.Count, stringToTest);
            }
        }

        [TestMethod]
        public void MinMaxSalaryValueTest()
        {
            {
                var ms = TestUtils.GetStreamFromString(@"001Á3245678865434ÁPauloÁ79228162514264337593543950335");

                var ish = new SalesReportCsvReader(ms, "Á");
                ish.Process();

                Assert.AreEqual(1, ish.Result.Length);

                var sellerPaulo = ish.Result[0] as Seller;

                Assert.AreEqual("3245678865434", sellerPaulo.CPF);
                Assert.AreEqual("Paulo", sellerPaulo.Name);
                Assert.AreEqual(decimal.MaxValue, sellerPaulo.Salary);
            }

            {
                var ms = TestUtils.GetStreamFromString(@"001Á3245678865434ÁPauloÁ-79228162514264337593543950335");

                var ish = new SalesReportCsvReader(ms, "Á");
                ish.Process();

                Assert.AreEqual(1, ish.Result.Length);

                var sellerPaulo = ish.Result[0] as Seller;

                Assert.AreEqual("3245678865434", sellerPaulo.CPF);
                Assert.AreEqual("Paulo", sellerPaulo.Name);
                Assert.AreEqual(decimal.MinValue, sellerPaulo.Salary);
            }
        }

        [TestMethod]
        public void ThrowsWhenInstantiatingWithInvalidValues()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new SalesReportCsvReader(null));
            Assert.ThrowsException<ArgumentNullException>(() => new SalesReportCsvReader(new MemoryStream(), ""));
        }

        /*
         ParseDosItens
         ParseSemItens
         */
    }
}
