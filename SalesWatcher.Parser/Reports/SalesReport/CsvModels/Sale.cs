using CsvHelper.Configuration.Attributes;
using SalesWatcher.Business.Converters;
using System.Collections.Generic;

namespace SalesWatcher.Business.CsvModels
{
    public class Sale
    {
        // 003çSale IDç[Item ID-Item Quantity-Item Price]çSalesman name

        [Index(1)]
        public int SaleId{ get; set; }

        [Index(2)]
        [TypeConverter(typeof(SoldItemsConverter))]
        public List<ItemSold> SoldItems { get; set; }

        [Index(3)]
        public string SoldBy { get; set; }

    }
}
