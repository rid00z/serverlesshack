using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace XAM.Function
{
    public class Product : TableEntity
    {
        private Guid productId;

        public Product()
        {
            RowKey = Guid.NewGuid().ToString();
        }

        public Guid ProductId
        {
            get => productId;
            set {
                productId = value;
                PartitionKey = productId.ToString();
            }
        }

        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
    }
}