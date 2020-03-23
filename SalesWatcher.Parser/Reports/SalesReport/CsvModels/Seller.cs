using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SalesWatcher.Business.CsvModels
{
    public class Seller
    {
        //001çCPFçNameçSalary

        [Index(1)]
        public string CPF { get; set; }
        
        [Index(2)]
        public string Name { get; set; }

        [Index(3)]
        public decimal Salary { get; set; }
    }
}
