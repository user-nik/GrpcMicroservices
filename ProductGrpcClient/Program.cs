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

                await GetAllProductsAsync(client);
                await Task.Delay(500);
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }


            Console.ReadKey();
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
    }
}
