using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WebScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal number;
            List<Item> items = new List<Item>();

            string html = File.ReadAllText("./../../../index.html"); //Reading the content from the HTML file and returning it as a string

            var htmlDocument = new HtmlDocument(); //Creating HTML Document using HtmlAgilityPack
            htmlDocument.LoadHtml(html);

            var itemList = htmlDocument.DocumentNode.SelectNodes("//*[@class = 'item']"); //Selecting all the nodes which have class equals to "item"

            foreach (var item in itemList)
            {
                string name = GetName(item);

                string price = GetPrice(item);

                string rating = GetRating(item);

                var style = NumberStyles.Any;
                var culture = CultureInfo.InvariantCulture;
                var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

                if (decimal.TryParse(rating, style, culture, out number))
                {
                    var ratingAsDecimal = decimal.Parse(rating, nfi);
                    if (ratingAsDecimal > 5)
                    {
                        ratingAsDecimal = 5;
                    }

                    Item currentItem = CreateAnItem(name, price, ratingAsDecimal);

                    items.Add(currentItem);
                }

            }

            PrintResultAsJson(items);

        }

        private static void PrintResultAsJson(List<Item> items) //Method that serialize list of items to json using Newtonsoft.Json and Newtonsoft.Json.Serialization
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            string json = JsonConvert.SerializeObject(items, new JsonSerializerSettings
            {

                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });


            Console.WriteLine(json);
        }

        private static Item CreateAnItem(string name, string price, decimal ratingAsDecimal)
        {
            return new Item()
            {
                ProductName = name,
                Price = price,
                Rating = ratingAsDecimal,
            };
        }

        private static string GetName(HtmlNode item)
        {
            return HtmlEntity.DeEntitize(item.Descendants()
                                .Where(childNode => childNode.Name == "img")
                                .FirstOrDefault()
                                .GetAttributeValue("alt", ""));
        }

        private static string GetRating(HtmlNode item)
        {
            return item.GetAttributeValue("rating", "");
        }

        private static string GetPrice(HtmlNode item)
        {
            var dollars = item.Descendants()
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("dollars"))
                .FirstOrDefault()
                .InnerText;

            var cents = item.Descendants()
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("cents"))
                .FirstOrDefault()
                .InnerText;

            var dollarsUpdated = dollars.Contains(',') ? dollars.Replace(",", "") : dollars;

            var price = dollarsUpdated + cents;
            return price;
        }
    }
}
