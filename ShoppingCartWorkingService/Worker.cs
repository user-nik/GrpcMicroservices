using Grpc.Core;
using Grpc.Net.Client;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCartWorkingService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                int delay = _config.GetValue<int>("WorkerService:TaskInterval");
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // Get Token

                var token = await GetTokenFromIS();

                //create Sopping cart
                //Retrieve products from server
                //add products to a sc
                using var scChannel = GrpcChannel
                    .ForAddress(_config.GetValue<string>("WorkerService:ShoppingCartServerUrl"));

                //get SC
                var scClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(scChannel);
                var scModel = await GetOrCreateShoppingCartAsync(scClient, token);
                //open shopping cart stream
                using var scClientStream = scClient.AddItemIntoShoppingCart();

                //retrive products 
                using var productChannel = GrpcChannel
                    .ForAddress(_config.GetValue<string>("WorkerService:ProductServerUrl"));
                var productClient = new ProductsProtoService.ProductsProtoServiceClient(productChannel);

                _logger.LogInformation("GetAllProducts --->");
                using var _clientData = productClient.GetAllProducts(new GetAllProductRequest());
                await foreach (var responseData in _clientData.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine("currentProduct - " + responseData.ToString());
                    var addNewScItem = new AddItemIntoShoppingCartRequest
                    {
                        Username = _config.GetValue<string>("WorkerService:UserName"),
                        DiscountCode = "CODE_90",
                        NewCartItem = new ShoppingCartItemModel
                        {
                            Color = "red",
                            Price = responseData.Price,
                            ProductId = responseData.ProductId,
                            ProductName = responseData.Name,
                            Quantity = 1
                        }
                    };

                    await scClientStream.RequestStream.WriteAsync(addNewScItem);
                }
                await scClientStream.RequestStream.CompleteAsync();
                var addItemIntoSCResponce = await scClientStream;
                _logger.LogInformation($"Added  {addItemIntoSCResponce.InsertCount} products to a shopping cart");

            }
        }
        static readonly HttpClient client = new HttpClient();
        private async Task<string> GetTokenFromIS()
        {
            var doc = await client.GetDiscoveryDocumentAsync(
                    _config.GetValue<string>("WorkerService:IdentityServerUrl")
                );

            if (doc.IsError)
            {
                _logger.LogError(doc.Error);
                return string.Empty;
            }

            var token = await client.RequestClientCredentialsTokenAsync(
                    new ClientCredentialsTokenRequest
                    {
                        Address = doc.TokenEndpoint,
                        ClientId = "ShoppingCartClient",
                        ClientSecret = "secret",
                        Scope = "ShoppingCartAPI"
                    }
                );

            if(token.IsError)
            {
                _logger.LogError(token.Error);
                return string.Empty;
            }

            return token.AccessToken;
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(
            ShoppingCartProtoService.ShoppingCartProtoServiceClient scClient,
            string token)
        {
            ShoppingCartModel shoppingCartModel;
            string userName = _config.GetValue<string>("WorkerService:UserName");
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");

            try
            {
                _logger.LogInformation("GetShoppingCartAsync started");
                
                shoppingCartModel = await scClient.GetShoppingCartAsync(
                        new GetShoppingCartRequest
                        { 
                                Username = userName,
                        },
                        headers 
                    );
                _logger.LogInformation($"GetShoppingCartAsync {shoppingCartModel.Username}");
            }
            catch (RpcException ex)
            {

                if(ex.Status.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogInformation("CreateShoppingCartAsync started");
                    shoppingCartModel = await scClient.CreateShoppingCartAsync(
                            new ShoppingCartModel
                            {
                                Username = userName
                            },
                            headers
                        );
                }
                else
                {
                    _logger.LogError(ex.Status.Detail);
                    throw;
                }
            }

            return shoppingCartModel;
        }
    }
}
