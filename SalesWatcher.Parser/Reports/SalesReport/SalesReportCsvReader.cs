using CsvHelper;
using CsvHelper.Configuration;
using SalesWatcher.Business.CsvModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SalesWatcher.Business.Reports.SalesReport;

namespace SalesWatcher.Business.Reports
{
    /// <summary>
    /// Classe que recebe um Stream e transforma em objetos de negócio.
    /// </summary>
    public class SalesReportCsvReader : BaseCsvReport, ISalesReportData
    {
        public SalesReportCsvReader(Stream stream, string separator = ";") : base(stream, separator) { }

        public List<Seller> Sellers { get; protected set; }
        public List<Customer> Customers { get; protected set; }
        public List<Sale> Sales { get; protected set; }

        public override void Process()
        {
            var streamReader = new StreamReader(Stream);
            var results = new List<object>();

            // mapeia o codigo ao tipo do modelo de negócio.
            var dictCodigoTipo = new Dictionary<string, Type>();
            dictCodigoTipo["1"] = typeof(Seller);
            dictCodigoTipo["2"] = typeof(Customer);
            dictCodigoTipo["3"] = typeof(Sale);

            using (var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.Delimiter = Separator;
                csv.Configuration.HasHeaderRecord = false;

                while (csv.Read())
                {
                    var codigo = csv.GetField(0).Trim().TrimStart(new char[] { '0' });

                    if (dictCodigoTipo.ContainsKey(codigo))
                    {
                        var obj = csv.GetRecord(dictCodigoTipo[codigo]);
                        results.Add(obj);
                    }
                }
            }

            this.Result = results.ToArray();

            this.Sellers = this.Result.OfType<Seller>().ToList();
            this.Customers = this.Result.OfType<Customer>().ToList();
            this.Sales = this.Result.OfType<Sale>().ToList();
        }
    }
}
