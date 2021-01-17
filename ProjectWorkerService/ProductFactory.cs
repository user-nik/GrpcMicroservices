using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWorkerService
{
    public class ProductFactory
    {
        private readonly ILogger<ProductFactory> _logger;
        private readonly IConfiguration _config;

        public ProductFactory(ILogger<ProductFactory> logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public Task<AddProductRequest> Generate()
        {
            var productName = _config.GetValue<string>("WorkerService:ProductName") + DateTimeOffset.Now;
            var productReuest = new AddProductRequest
            {
                Product = new ProductModel
                {
                    Created = Timestamp.FromDateTime(DateTime.UtcNow),
                    Description = "Product",
                    Name = productName,
                    Price = 1799,
                    Status = ProductStatus.Instock
                }
            };

            return Task.FromResult(productReuest);

        }
    }
}
