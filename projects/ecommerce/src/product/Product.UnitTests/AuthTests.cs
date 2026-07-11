using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ProductApi.UnitTests
{
    public class AuthTests
    {
        private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public const string SchemeName = "Test";
            public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder)
                : base(options, logger, encoder)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                // Read header X-Test-Claims with JSON array of claims { "type":"scope","value":"write:products" }
                if (!Request.Headers.TryGetValue("X-Test-Claims", out var header))
                {
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                var claimsJson = header.ToString();
                try
                {
                    var claims = JsonSerializer.Deserialize<List<TestClaim>>(claimsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                    var identities = new List<ClaimsIdentity>();
                    var claimsList = claims.Select(c => new Claim(c.Type, c.Value)).ToList();
                    var identity = new ClaimsIdentity(claimsList, SchemeName);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, SchemeName);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                catch
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid test claims"));
                }
            }

            private class TestClaim { public string Type { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; }
        }

        private WebApplicationFactory<Program> CreateFactory()
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace authentication with test scheme
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                    // Use in-memory DB for tests
                    // Remove existing DbContext registration and add in-memory
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<ProductApi.Database.ProductDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<ProductApi.Database.ProductDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });
        }

        private HttpClient CreateClient(WebApplicationFactory<Program> factory)
        {
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            return client;
        }

        [Test]
        public async Task PostCategory_Unauthenticated_Returns401()
        {
            var factory = CreateFactory();
            var client = CreateClient(factory);

            var category = new { Name = "T1" };
            var content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("/api/categories/", content);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task PostCategory_AuthenticatedWithoutScope_Returns403()
        {
            var factory = CreateFactory();
            var client = CreateClient(factory);

            var category = new { Name = "T2" };
            var content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json");

            // Provide authenticated user but no scope claim
            var claims = new[] { new { type = "sub", value = "test-user" } };
            client.DefaultRequestHeaders.Add("X-Test-Claims", JsonSerializer.Serialize(claims));

            var resp = await client.PostAsync("/api/categories/", content);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task PostCategory_AuthenticatedWithWriteScope_ReturnsCreated()
        {
            var factory = CreateFactory();
            var client = CreateClient(factory);

            var category = new { Name = "T3" };
            var content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json");

            // Provide authenticated user with write scope matching the appsettings WriteScope
            var claims = new[] { new { type = "scope", value = "uw-ecom-api/write" } };
            client.DefaultRequestHeaders.Add("X-Test-Claims", JsonSerializer.Serialize(claims));

            var resp = await client.PostAsync("/api/categories/", content);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task PostCategory_WithDifferentScope_ReturnsForbidden()
        {
            var factory = CreateFactory();
            var client = CreateClient(factory);

            var category = new { Name = "T4" };
            var content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json");

            // Provide a scope that does not include the write scope
            var claims = new[] { new { type = "scope", value = "read:products" } };
            client.DefaultRequestHeaders.Add("X-Test-Claims", JsonSerializer.Serialize(claims));

            var resp = await client.PostAsync("/api/categories/", content);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task PostCategory_WithMultipleScopesMissingWrite_ReturnsForbidden()
        {
            var factory = CreateFactory();
            var client = CreateClient(factory);

            var category = new { Name = "T5" };
            var content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json");

            // Multiple scopes but missing the configured write scope
            var claims = new[] { new { type = "scope", value = "read:products other:scope" } };
            client.DefaultRequestHeaders.Add("X-Test-Claims", JsonSerializer.Serialize(claims));

            var resp = await client.PostAsync("/api/categories/", content);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }
    }
}
