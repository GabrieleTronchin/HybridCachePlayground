using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024;
    options.MaximumKeyLength = 1024;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    builder.Configuration.Bind("RedisCache", options);
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet(
        "/GetOrCreateAsync/{key}",
        async (string key, HybridCache cache, CancellationToken token = default) =>
        {
            return await cache.GetOrCreateAsync(
                key,
                async (token) => await Task.Run(() => $"{nameof(cache.GetOrCreateAsync)} - Hello World")
            );
        }
    )
    .WithName("GetOrCreateAsync")
    .WithOpenApi();

app.MapPost(
        "/SetAsync/{key}",
        async (string key, HybridCache cache) =>
        {
            await cache.SetAsync(key, $"{nameof(cache.SetAsync)} - Hello World");
        }
    )
    .WithName("CreateAndSet")
    .WithOpenApi();

app.MapDelete(
        "/RemoveAsync",
        (string key, HybridCache cache) =>
        {
            cache.RemoveAsync(key);
        }
    )
    .WithName("RemoveAsync")
    .WithOpenApi();

app.Run();
