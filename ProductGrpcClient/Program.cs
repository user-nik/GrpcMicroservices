﻿using Grpc.Core;
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
                await GetAllProductsAsync(client);

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

            //using var clientData = client.GetAllProducts(new GetAllProductRequest());
            //while (await clientData.ResponseStream.MoveNext())
            //{
            //    var currentProduct = clientData.ResponseStream.Current;
            //    Console.WriteLine("currentProduct - " + currentProduct.ToString());
            //
            //}

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
    }
}