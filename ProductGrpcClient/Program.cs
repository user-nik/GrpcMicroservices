using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ProductGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductsProtoService.ProductsProtoServiceClient(channel);
            await Task.Delay(1000);

            try
            {
                await GetProductAsync(client);
                await Task.Delay(500);

                await AddProductAsync(client);
                await Task.Delay(500);

                await UpdateProductsAsync(client);
                await Task.Delay(500);

                await DeleteProductsAsync(client);
                await Task.Delay(500);

                await GetAllProductsAsync(client);
                await Task.Delay(500);

                await InsertBulkProduct(client);
                await Task.Delay(500);

                await GetAllProductsAsync(client);
                await Task.Delay(500);

            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }


            Console.ReadKey();
        }

        private static async Task InsertBulkProduct(ProductsProtoService.ProductsProtoServiceClient client)
        {
            Console.WriteLine("InsertBulkProduct started..");

            using var clientBulk = client.InsertBulkProduct();

            for (int i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Created = Timestamp.FromDateTime(DateTime.UtcNow),
                    Description = "Bulk Added Product",
                    Name = $"Name {i}",
                    Price = 100 * i,
                    Status = ProductStatus.Instock
                };

                await clientBulk.RequestStream.WriteAsync(productModel);

            }

            await clientBulk.RequestStream.CompleteAsync();

            var responseBulk = await clientBulk;

            Console.WriteLine("InsertBulkProduct ended..");
            Console.WriteLine($"Status {responseBulk.Success} Inserted: {responseBulk.InsertCount}");

        }

        private static async Task GetAllProductsAsync(ProductsProtoService.ProductsProtoServiceClient client)
        {
            Console.WriteLine("GetAllProducts started..");

            using var _clientData = client.GetAllProducts(new GetAllProductRequest());
            await foreach (var responseData in _clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine("currentProduct - " + responseData.ToString());
            }
        }

        private static async Task GetProductAsync(ProductsProtoService.ProductsProtoServiceClient client)
        {
            Console.WriteLine("GetProductAsync started..");

            var response = await client.GetProductAsync(
                    new GetProductRequest
                    {
                        ProductId = 1
                    });

            Console.WriteLine("response - " + response.ToString());
        }


        private static async Task AddProductAsync(ProductsProtoService.ProductsProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync started..");
            var addProductResponse = await client.AddProductAsync(
                    new AddProductRequest
                    {
                        Product = new ProductModel
                        {
                            Created = Timestamp.FromDateTime(DateTime.UtcNow),
                            Description = "Added Product",
                            Name = "Name",
                            Price = 1799,
                            Status = ProductStatus.Instock
                        }
                    }
                );

            Console.WriteLine("GetAllProducts started.." + addProductResponse.ToString());

        }


        private static async Task DeleteProductsAsync(ProductsProtoService.ProductsProtoServiceClient client)
        {
            Console.WriteLine("DeleteProductsAsync started..");

            var deleteReponse = await client.DeleteProductAsync(
               new DeleteProductRequest
               {
                   ProductId = 2,
               });
           
            Console.WriteLine("DeleteProductsAsync response.. " + deleteReponse.ToString());

        }

        private static async Task UpdateProductsAsync(ProductsProtoService.ProductsProtoServiceClient client)
        {
            Console.WriteLine("UpdateProductsAsync started..");
            var updateReponse = await client.UpdateProductAsync(
                new UpdateProductRequest
                {
                    Product = new ProductModel
                    {
                        Created = Timestamp.FromDateTime(DateTime.UtcNow),
                        Description = "Updated Product",
                        Name = "Name",
                        Price = 1799,
                        Status = ProductStatus.Instock,
                        ProductId = 1
                    }
                });

            Console.WriteLine("UpdateProductsAsync response.. " + updateReponse.ToString());

        }
    }
}
