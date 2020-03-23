using CsvHelper.Configuration.Attributes;

namespace SalesWatcher.Business.CsvModels
{
    public class ItemSold
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}