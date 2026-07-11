using CartApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Distributed cache and session support for cart. Use Redis when configured, otherwise fall back to in-memory.
var cacheProvider = builder.Configuration.GetValue<string>("Cart:DistributedCache:Provider") ?? "InMemory";
if (string.Equals(cacheProvider, "Redis", StringComparison.OrdinalIgnoreCase))
{
    var conn = builder.Configuration.GetValue<string>("Cart:DistributedCache:Connection") ?? "localhost:6379";
    // Requires package Microsoft.Extensions.Caching.StackExchangeRedis
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = conn;
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(1);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSession();

// Map cart endpoints
var cart = app.MapGroup("/api/cart");
cart.MapCart();

app.Run();

