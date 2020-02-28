using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace XAM.Function
{
    public static class ProductAPI
    {
        const string TableConnectionString = "TableConnectionString";

        [FunctionName("GetProduct")]
        public static async Task<IActionResult> GetProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Table("Products", Connection = TableConnectionString)] CloudTable productTable,
            ILogger log)
        {
            string productId = req.Query["productId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            productId = productId ?? data?.name;

            if (productId != null)
            {
                TableQuery<Product> rangeQuery = new TableQuery<Product>().Where(
                   TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, productId));

                foreach (Product product in await productTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
                {
                    return (ActionResult)new OkObjectResult(product);
                }
            }

            return new BadRequestObjectResult("Product not found");
        }

        [FunctionName("GetProducts")]
        public static async Task<IActionResult> GetProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Table("Products", Connection = TableConnectionString)] CloudTable productTable,
            ILogger log)
        {
            TableQuery<Product> rangeQuery = new TableQuery<Product>();

            var products = await productTable.ExecuteQuerySegmentedAsync(rangeQuery, null);
            return (ActionResult)new OkObjectResult(products);
        }

        [FunctionName("PostProduct")]
        public static async Task<IActionResult> PostProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Table("Products", Connection = TableConnectionString)] CloudTable productTable,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Product data = JsonConvert.DeserializeObject<Product>(requestBody);

            TableOperation tableOperation = TableOperation.InsertOrReplace(data);
            var result = await productTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode >= 200 || result.HttpStatusCode < 300)
            {
                TableQuery<Product> rangeQuery = new TableQuery<Product>().Where(
                   TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, data.ProductId.ToString()));

                foreach (Product product in await productTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
                {
                    return (ActionResult)new OkObjectResult(product);
                }
            }

            return new BadRequestObjectResult("Product not found");
        }


    }
}
