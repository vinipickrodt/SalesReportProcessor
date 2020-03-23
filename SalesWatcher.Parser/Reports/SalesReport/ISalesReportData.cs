using SalesWatcher.Business.CsvModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SalesWatcher.Business.Reports.SalesReport
{
    public interface ISalesReportData
    {
        List<Seller> Sellers { get; }
        List<Customer> Customers { get; }
        List<Sale> Sales { get; }
    }
}
