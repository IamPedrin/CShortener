using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var redisConnectionString = builder.Configuration["RedisConnection"] ?? "localhost:6379";

//Configuração do banco de dados usando Entity Framework Core e Npgsql para PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//Configuraćão do Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));
//Configuraćão do servico de contagem de acessos em segundo plano
builder.Services.AddHostedService<SyncAccessCountService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => "CShortener!");


//Endpoint para gerar um um link encurtado
app.MapPost("/api/shortener", (CreateUrlRequest request, AppDbContext db, HttpContext context) =>
{
    //Validacao da URL original
    if (string.IsNullOrWhiteSpace(request.OriginalUrl) ||
    !Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var validateUri) ||
    (validateUri.Scheme != Uri.UriSchemeHttp && validateUri.Scheme != Uri.UriSchemeHttps))
    {
        return Results.BadRequest(new { erro = "A URL fornecida é inválida" });
    }

    //Usando Nanoid para gerar o shortcode aleatório
    var newShortCode = Nanoid.Generate(size: 7);
    //Criação do objeto URL
    var urlObj = new Url(request.OriginalUrl, newShortCode);

    //Salvando alterações no banco de dados
    db.Add(urlObj);
    db.SaveChanges();

    var urlBase = $"{context.Request.Scheme}://{context.Request.Host}";
    var urlCompleta = $"{urlBase}/{urlObj.ShortCode}";

    return Results.Created($"/api/shortener/{urlObj.ShortCode}/stats", new { url = urlCompleta });
});

//Endpoint para redirecionar o usuário para a URL original
app.MapGet("/{shortCode}", async (string shortCode, AppDbContext db, IConnectionMultiplexer redis) =>
{
    //Leitura do cache
    var dbRedis = redis.GetDatabase();

    //Salvando a quantidade de acessos pelo redis
    await dbRedis.StringIncrementAsync($"clicks:{shortCode}");
    var cachedUrl = await dbRedis.StringGetAsync(shortCode);

    //Caso encontre, redireciona. Se não, busca no banco de dados
    if (cachedUrl.HasValue)
    {
        return Results.Redirect(cachedUrl.ToString());
    }

    //Busca no banco de dados pelo shortcode
    var urlDb = db.Urls.FirstOrDefault(u => u.ShortCode == shortCode);

    if (urlDb == null)
    {
        return Results.NotFound(new { erro = "Link não encontrado!" });
    }

    //Salva no Redis por uma hora quando encontrado no banco
    await dbRedis.StringSetAsync(shortCode, urlDb.OriginalUrl, TimeSpan.FromHours(1));

    return Results.Redirect(urlDb.OriginalUrl);
});

//Endpoint para obter as estatísticas de um link encurtado
app.MapGet("/api/shortener/{shortCode}/stats", async (string shortCode, AppDbContext db, IConnectionMultiplexer redis) =>
{
    var urlDb = db.Urls.FirstOrDefault(u => u.ShortCode == shortCode);

    if (urlDb == null)
    {
        return Results.NotFound(new { erro = "Link não encontrado!" });
    }

    var dbRedis = redis.GetDatabase();
    var redisClicks = await dbRedis.StringGetAsync($"clicks:{shortCode}");
    long totalClicks = urlDb.AccessCount;
    if (redisClicks.HasValue)
    {
        totalClicks += (long)redisClicks;
    }

    return Results.Ok(new
    {
        urlDb.OriginalUrl,
        AccessCount = totalClicks,
        urlDb.CreatedAt
    });

});

app.Run();
