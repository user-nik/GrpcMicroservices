using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Services
{
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IMapper _mapper;

        public ShoppingCartService(ShoppingCartContext shoppingCartContext, ILogger<ShoppingCartService> logger, IMapper mapper)
        {
            _shoppingCartContext = shoppingCartContext ?? throw new ArgumentNullException(nameof(shoppingCartContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request
            ,ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext
                .ShoppingCarts
                .FirstOrDefaultAsync(r=>r.UserName == request.Username);

            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "There is no shoppingCart"));
            }

            var shopingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);

            return shopingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shooppingCart = _mapper.Map<ShoppingCart>(request);

            var isExist = await _shoppingCartContext.ShoppingCarts
                .AnyAsync(r=>r.UserName == shooppingCart.UserName);

            if(isExist)
            {
                _logger.LogError($"CreateShoppingCart for {request.Username}");
                throw new RpcException(new Status(StatusCode.AlreadyExists, "There is shoppingCart"));
            }

            _shoppingCartContext.ShoppingCarts.Add(shooppingCart);
            await _shoppingCartContext.SaveChangesAsync();

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shooppingCart);


            return shoppingCartModel;
        }


        public override async Task<RemoveItemFromShoppingCartResponse> RemoveItemFromShoppingCart(
            RemoveItemFromShoppingCartRequest request,
            ServerCallContext context)
        {

            var shoppingCart = await _shoppingCartContext
                .ShoppingCarts
                .FirstOrDefaultAsync(r => r.UserName == request.Username);

            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "There is no shoppingCart"));
            }

            var removeCartItems = shoppingCart.Items
                .FirstOrDefault(r=>r.ProductId == request.RemoveCartItem.ProductId);

            if (removeCartItems == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"CartItem {request.Username} with productid - {removeCartItems.ProductId} not found"));
            }

            shoppingCart.Items.Remove(removeCartItems);
            var removeCount = await _shoppingCartContext.SaveChangesAsync();


            var response = new RemoveItemFromShoppingCartResponse { 
                Success = removeCount > 0 
            };
            return response;
        }


        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(
            IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream, 
            ServerCallContext context)
        {

            await foreach (var item in requestStream.ReadAllAsync())
            {
                var shoppingCart = await _shoppingCartContext
                    .ShoppingCarts
                    .FirstOrDefaultAsync(r => r.UserName == item.Username);

                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "There is no shoppingCart"));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(item.NewCartItem);
                var cartItem = shoppingCart.Items
                    .FirstOrDefault(r=>r.ProductId == newAddedCartItem.ProductId);

                if(cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    float discount = 100;
                    newAddedCartItem.Price -= discount;
                    shoppingCart.Items.Add(newAddedCartItem);
                }
            }

            var insertCount = await _shoppingCartContext.SaveChangesAsync();

            var response = new AddItemIntoShoppingCartResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };

            return response;
        }
    }
}
