using ShoppingCartGrpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Data
{
    public class ShoppingCartContextSeed
    {
        public static void Seed(ShoppingCartContext shoppingCartContext)
        {
            if (!shoppingCartContext.ShoppingCartItems.Any())
            {
                var shoppingCarts = new List<ShoppingCart>
                {
                    new ShoppingCart
                    {
                        UserName = "user",
                        Items = new List<ShoppingCartItem>
                        {
                            new ShoppingCartItem
                            {
                                Color = "Red",
                                Quantity = 3,
                                Price = 899,
                                ProductId = 1,
                                ProductName = "Product",
                            }
                        }
                    }
                };
                shoppingCartContext.ShoppingCarts.AddRange(shoppingCarts);
                shoppingCartContext.SaveChanges();
            }
        }
    }

}
