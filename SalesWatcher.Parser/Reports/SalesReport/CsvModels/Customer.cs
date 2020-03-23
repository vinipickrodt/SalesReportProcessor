using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SalesWatcher.Business.CsvModels
{
    public class Customer
    {
        // 002çCNPJçNameçBusiness Area

        [Index(1)]
        public string CNPJ { get; set; }
        
        [Index(2)]
        public string Name { get; set; }
        
        [Index(3)]
        public string BusinessArea { get; set; }
    }
}
