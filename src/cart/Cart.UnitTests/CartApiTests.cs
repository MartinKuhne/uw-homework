using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using CartApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CartApi.UnitTests
{
    public class CartApiTests
    {
        private WebApplicationFactory<Program> _factory = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                // Ensure using in-memory distributed cache for tests
                builder.ConfigureServices(services =>
                {
                    services.AddDistributedMemoryCache();
                });
            });
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        [Test]
        public async Task GetCart_InitiallyEmpty_ReturnsEmptyList()
        {
            var client = _factory.CreateClient();
            var res = await client.GetAsync("/api/cart/");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var items = await res.Content.ReadFromJsonAsync<List<CartItem>>();
            Assert.IsNotNull(items);
            Assert.That(items!.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task AddItem_ThenGet_ReturnsItem()
        {
            var client = _factory.CreateClient();
            var newItem = new CartItem { ProductId = System.Guid.NewGuid(), Quantity = 2 };
            var postRes = await client.PostAsJsonAsync("/api/cart/items", newItem);
            Assert.That(postRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var items = await postRes.Content.ReadFromJsonAsync<List<CartItem>>();
            Assert.IsNotNull(items);
            Assert.That(items!.Count, Is.EqualTo(1));
            Assert.That(items[0].ProductId, Is.EqualTo(newItem.ProductId));

            // subsequent get should return same item (cookie maintained by handler)
            var getRes = await client.GetAsync("/api/cart/");
            var got = await getRes.Content.ReadFromJsonAsync<List<CartItem>>();
            Assert.IsNotNull(got);
            Assert.That(got!.Count, Is.EqualTo(1));
            Assert.That(got[0].ProductId, Is.EqualTo(newItem.ProductId));
        }

        [Test]
        public async Task RemoveItem_Works()
        {
            var client = _factory.CreateClient();
            var productId = System.Guid.NewGuid();
            var item = new CartItem { ProductId = productId, Quantity = 1 };
            var postRes = await client.PostAsJsonAsync("/api/cart/items", item);
            Assert.That(postRes.IsSuccessStatusCode, Is.True);

            var delRes = await client.DeleteAsync($"/api/cart/items/{productId}");
            Assert.That(delRes.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var getRes = await client.GetAsync("/api/cart/");
            var got = await getRes.Content.ReadFromJsonAsync<List<CartItem>>();
            Assert.IsNotNull(got);
            Assert.That(got!.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateItem_UpdatesQuantity()
        {
            var client = _factory.CreateClient();
            var productId = System.Guid.NewGuid();
            var first = new CartItem { ProductId = productId, Quantity = 2 };
            var r1 = await client.PostAsJsonAsync("/api/cart/items", first);
            Assert.That(r1.IsSuccessStatusCode, Is.True);

            var second = new CartItem { ProductId = productId, Quantity = 7 };
            var r2 = await client.PostAsJsonAsync("/api/cart/items", second);
            Assert.That(r2.IsSuccessStatusCode, Is.True);

            var items = await r2.Content.ReadFromJsonAsync<List<CartItem>>();
            Assert.IsNotNull(items);
            Assert.That(items!.Count, Is.EqualTo(1));
            Assert.That(items[0].Quantity, Is.EqualTo(7));
        }

        [Test]
        public async Task ClearCart_RemovesAllItems()
        {
            var client = _factory.CreateClient();
            var a = new CartItem { ProductId = System.Guid.NewGuid(), Quantity = 1 };
            var b = new CartItem { ProductId = System.Guid.NewGuid(), Quantity = 3 };
            var p1 = await client.PostAsJsonAsync("/api/cart/items", a);
            var p2 = await client.PostAsJsonAsync("/api/cart/items", b);
            Assert.That(p1.IsSuccessStatusCode && p2.IsSuccessStatusCode, Is.True);

            var clear = await client.DeleteAsync("/api/cart/");
            Assert.That(clear.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var getRes = await client.GetAsync("/api/cart/");
            var got = await getRes.Content.ReadFromJsonAsync<List<CartItem>>();
            Assert.IsNotNull(got);
            Assert.That(got!.Count, Is.EqualTo(0));
        }
    }
}
