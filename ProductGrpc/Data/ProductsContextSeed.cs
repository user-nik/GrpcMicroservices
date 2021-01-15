using ProductGrpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductGrpc.Data
{
    public class ProductsContextSeed
    {
        public static void Seed(ProductsContext productContext)
        {
            if(!productContext.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        CreatedTime = DateTime.UtcNow,
                        Description = "DEsc",
                        Name = "Name",
                        Price = 799,
                        Status = ProductStatus.INSTOCK
                    }
                };

                productContext.Products.AddRange(products);
                productContext.SaveChanges();
            }
        }
    }
}
