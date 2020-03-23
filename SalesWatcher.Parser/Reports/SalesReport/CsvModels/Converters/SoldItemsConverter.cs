using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SalesWatcher.Business.CsvModels;
using CsvHelper.TypeConversion;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SalesWatcher.Business.Converters
{
    public class SoldItemsConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            // [Item ID-Item Quantity-Item Price]

            if ((text ?? "").Length > 2)
            {
                if (text[0] == '[' && text.Last() == ']')
                {
                    text = text.Substring(1, text.Length - 2).Trim();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var itemsSold = text.Split(',').Select(part =>
                        {
                            var parts = part.Split('-');

                            var itemSold = new ItemSold();

                            int id = 0;
                            int quantity = 0;
                            decimal price = 0;

                            var parsedId = int.TryParse(parts[0], out id);
                            var parsedQuantity = parts.Length > 1 ? int.TryParse(parts[1], out quantity) : false;
                            var parsedPrice = parts.Length > 2 ? decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out price) : false;

                            if (parsedId)
                                itemSold.Id = id;

                            if (parsedQuantity)
                                itemSold.Quantity = quantity;

                            if (parsedPrice)
                                itemSold.Price = price;

                            return itemSold;
                        }).ToList();

                        return itemsSold;
                    }
                }
            }

            return new List<ItemSold>();
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            var itemSold = value as ItemSold;

            if (itemSold != null)
            {
                return $"[{itemSold.Id};{itemSold.Quantity};{itemSold.Price}]";
            }

            return "";
        }
    }
}
