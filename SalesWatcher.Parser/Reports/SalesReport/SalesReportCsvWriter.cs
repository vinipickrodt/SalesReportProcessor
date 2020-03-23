using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SalesWatcher.Business.Reports.SalesReport
{
    public class SalesReportCsvWriter
    {
        public ISalesReportData ReportData { get; protected set; }
        public string Separator { get; protected set; }

        public int QtdClientes { get; protected set; }
        public int QtdVendedores { get; protected set; }
        public int IdVendaMaisCara { get; protected set; }
        public string PiorVendedor { get; protected set; }

        public SalesReportCsvWriter(ISalesReportData reportData, string separator = ";")
        {
            if (reportData == null)
                throw new ArgumentNullException(nameof(reportData));

            if (string.IsNullOrEmpty(separator))
                throw new ArgumentNullException(nameof(separator));

            this.ReportData = reportData;
            this.Separator = separator;
        }

        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var sw = new StreamWriter(stream))
            {
                Process();

                var headerStr = string.Join(Separator, new string[] { "QtdClientes", "QtdVendedores", "IdVendaMaisCara", "PiorVendedor" });
                var text = string.Join(Separator, new object[] { QtdClientes, QtdVendedores, IdVendaMaisCara, PiorVendedor });

                sw.WriteLine(headerStr);
                sw.WriteLine(text);
            }
        }

        private void Process()
        {
            var mostProfitableSales = ReportData.Sales
                                .OrderByDescending(sale => sale.SoldItems.Sum(soldItem => soldItem.Price))
                                .ToList();

            var lessProfitableSellers = ReportData.Sales
                .GroupBy(sale => sale.SoldBy)
                .OrderBy(sellerSales => sellerSales.Sum(sale => sale.SoldItems.Sum(soldItem => soldItem.Price)))
                .ToList();

            this.QtdClientes = (ReportData.Customers?.Count).GetValueOrDefault();
            this.QtdVendedores = (ReportData.Sellers?.Count).GetValueOrDefault();
            this.IdVendaMaisCara = (mostProfitableSales.FirstOrDefault()?.SaleId).GetValueOrDefault();
            this.PiorVendedor = (lessProfitableSellers.FirstOrDefault()?.Key) ?? "";
        }
    }
}
