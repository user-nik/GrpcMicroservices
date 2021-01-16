using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductsProtoService.ProductsProtoServiceBase
    {
        private readonly ProductsContext _productContext;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductsContext productContext, ILogger<ProductService> logger)
        {
            _productContext = productContext ?? throw new ArgumentNullException(nameof(productContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request,
            ServerCallContext context)
        {
            var product = await _productContext.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "There is no product"));
            }



            var productModel = new ProductModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Created = Timestamp.FromDateTime(product.CreatedTime.ToUniversalTime()),
                Status = (Protos.ProductStatus)product.Status
            };


            return productModel;
        }

        /// <summary>
        /// Server streaming method.
        /// returns all product from db
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetAllProducts(GetAllProductRequest request,
            IServerStreamWriter<ProductModel> responseStream,
            ServerCallContext context)
        {
            var proctList = await _productContext.Products.ToListAsync();

            foreach (var product in proctList)
            {
                var productModel = new ProductModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Created = Timestamp.FromDateTime(product.CreatedTime.ToUniversalTime()),
                    Status = (Protos.ProductStatus)product.Status
                };

                await responseStream.WriteAsync(productModel);

            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request,
            ServerCallContext context)
        {
            var product = new Product
            {
                Name = request.Product.Name,
                Description = request.Product.Description,
                Price = request.Product.Price,
                CreatedTime = request.Product.Created.ToDateTime(),
                Status = (Models.ProductStatus)request.Product.Status
            };

            _productContext.Products.Add(product);
            await _productContext.SaveChangesAsync();

            var productModel = new ProductModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Created = Timestamp.FromDateTime(product.CreatedTime.ToUniversalTime()),
                Status = (Protos.ProductStatus)product.Status
            };


            return productModel;
        }
    }
}
