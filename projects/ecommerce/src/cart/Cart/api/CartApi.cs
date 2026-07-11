using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CartApi
{
    public static class CartApi
    {
        private const string CookieName = "cartId";

        public static RouteGroupBuilder MapCart(this RouteGroupBuilder group)
        {
            static string GetOrCreateCartId(HttpContext http)
            {
                if (http.Request.Cookies.TryGetValue(CookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
                {
                    return existing;
                }

                var id = Guid.NewGuid().ToString();
                http.Response.Cookies.Append(CookieName, id, new CookieOptions { HttpOnly = true, IsEssential = true });
                return id;
            }

            group.MapGet("/", async (HttpContext http, IDistributedCache cache) =>
            {
                var cartId = GetOrCreateCartId(http);
                var json = await cache.GetStringAsync($"cart:{cartId}");
                var items = string.IsNullOrEmpty(json) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json)!;
                return Results.Ok(items);
            }).WithName("GetCart");

            group.MapPost("/items", async (HttpContext http, CartItem item, IDistributedCache cache) =>
            {
                var cartId = GetOrCreateCartId(http);
                var json = await cache.GetStringAsync($"cart:{cartId}");
                var items = string.IsNullOrEmpty(json) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json)!;
                var existing = items.Find(i => i.ProductId == item.ProductId);
                if (existing is null) items.Add(item); else existing.Quantity = item.Quantity;
                await cache.SetStringAsync($"cart:{cartId}", JsonSerializer.Serialize(items));
                return Results.Ok(items);
            }).WithName("AddCartItem");

            group.MapDelete("/items/{productId}", async (HttpContext http, Guid productId, IDistributedCache cache) =>
            {
                var cartId = GetOrCreateCartId(http);
                var json = await cache.GetStringAsync($"cart:{cartId}");
                var items = string.IsNullOrEmpty(json) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json)!;
                items.RemoveAll(i => i.ProductId == productId);
                await cache.SetStringAsync($"cart:{cartId}", JsonSerializer.Serialize(items));
                return Results.NoContent();
            }).WithName("RemoveCartItem");

            group.MapDelete("/", async (HttpContext http, IDistributedCache cache) =>
            {
                var cartId = GetOrCreateCartId(http);
                await cache.RemoveAsync($"cart:{cartId}");
                return Results.NoContent();
            }).WithName("ClearCart");

            return group;
        }
    }
}
