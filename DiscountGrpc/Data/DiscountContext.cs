using DiscountGrpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscountGrpc.Data
{
    public class DiscountContext
    {
        public static readonly List<Discount> Discounts = new List<Discount>()
        {
            new Discount{ DiscountId = 1, Code ="CODE_10", Amount= 10 },
            new Discount{ DiscountId = 2, Code ="CODE_20", Amount= 20 },
            new Discount{ DiscountId = 3, Code ="CODE_30", Amount= 30 },
            new Discount{ DiscountId = 3, Code ="CODE_90", Amount= 90 },
        };
    }
}
