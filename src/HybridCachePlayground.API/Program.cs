using Microsoft.Extensions.Caching.Hybrid;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024;
    options.MaximumKeyLength = 1024;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(2),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddStackExchangeRedisCache(options => builder.Configuration.Bind("RedisCache", options));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet(
        "/GetOrCreateAsync/{key}",
        async (string key, HybridCache cache, CancellationToken token = default) =>
        await cache.GetOrCreateAsync(
                key,
                async (token) => await Task.Run(() => $"{nameof(cache.GetOrCreateAsync)} - Hello World - {DateTime.Now}"),
                token: token
            )
    )
    .WithName("GetOrCreateAsync")
    .WithOpenApi();


app.MapGet(
        "/GetOrCreateWithTagsAsync/{key}",
        async (string key, HybridCache cache, CancellationToken token = default) =>
        {
            var tags = new List<string> { $"{nameof(Assembly.FullName)}" };

            return await cache.GetOrCreateAsync(
                key,
                async (token) => await Task.Run(() => $"{nameof(cache.GetOrCreateAsync)} - Hello World - {DateTime.Now}"),
                tags: tags,
                token: token
            );
        }
    )
    .WithName("GetOrCreateWithTagsAsync")
    .WithOpenApi();


app.MapPost(
        "/SetAsync/{key}",
        async (string key, HybridCache cache, CancellationToken token = default) =>
        await cache.SetAsync(key, $"{nameof(cache.SetAsync)} - Hello World - {DateTime.Now}", token: token)
    )
    .WithName("CreateAndSet")
    .WithOpenApi();

app.MapDelete(
        "/RemoveByTagAsync",
        async (HybridCache cache, CancellationToken token = default) =>
        await cache.RemoveByTagAsync($"{nameof(Assembly.FullName)}", token: token)
    )
    .WithName("RemoveByTagAsync")
    .WithOpenApi();


app.MapDelete(
        "/RemoveAsync/{key}",
        async (string key, HybridCache cache, CancellationToken token = default) =>
        await cache.RemoveAsync(key, token: token)
    )
    .WithName("RemoveAsync")
    .WithOpenApi();

app.Run();
