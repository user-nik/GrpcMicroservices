using AutoMapper;
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
        private readonly IMapper _mapper;

        public ProductService(ProductsContext productContext, ILogger<ProductService> logger, IMapper mapper)
        {
            _productContext = productContext ?? throw new ArgumentNullException(nameof(productContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request,
            ServerCallContext context)
        {
            var product = await _productContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(r=>r.ProductId == request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "There is no product"));
            } 

            var productModel = _mapper.Map<ProductModel>(product);

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
                var productModel = _mapper.Map<ProductModel>(product);

                await responseStream.WriteAsync(productModel);

            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request,
            ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            _productContext.Products.Add(product);
            await _productContext.SaveChangesAsync();

            _logger.LogInformation($"{product.Name} product saved ");

            var productModel = _mapper.Map<ProductModel>(product);
            return productModel;
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request,
            ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            bool isExists = await _productContext.Products
                .AnyAsync(p=> p.ProductId == product.ProductId);

            if (!isExists)
            {
               throw new RpcException(new Status(StatusCode.NotFound, "There is no product"));
            }

            _productContext.Entry(product).State = EntityState.Modified;
            try
            {
                await _productContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw;
            }

            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }

        public override async Task<DeleteProductModelResponse> DeleteProduct(DeleteProductRequest request,
            ServerCallContext context)
        {
            var product = await _productContext.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "There is no product"));
            }

            _productContext.Products.Remove(product);
            var deleteCount = await _productContext.SaveChangesAsync();

            var reponse = new DeleteProductModelResponse
            {
                Success = deleteCount > 0
            };

            return reponse;
        }

        public override async Task<InsertBulkProductReponse> InsertBulkProduct(IAsyncStreamReader<ProductModel> requestStream,
            ServerCallContext context)
        {
            await foreach (var item in requestStream.ReadAllAsync())
            {
                var product = _mapper.Map<Product>(item);
                _productContext.Products.Add(product);
            }

            //while (await requestStream.MoveNext())
            //{
            //    var product = _mapper.Map<Product>(requestStream.Current);
            //    _productContext.Products.Add(product);
            //}

            var insertCount = await _productContext.SaveChangesAsync();

            var response = new InsertBulkProductReponse
            {
                InsertCount = insertCount,
                Success = insertCount > 0,
            };

            return response;
        }
    }
}
