using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscountGrpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var discount = DiscountContext.Discounts
                .FirstOrDefault(r => r.Code == request.DiscountCode);

            if(discount == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Discount with {request.DiscountCode} code was not found"));
            }

            var response = new DiscountModel
            {
                Amount = discount.Amount,
                Code = discount.Code,
                DiscountId = discount.DiscountId
            };

            return Task.FromResult(response);
        }
    }
}
