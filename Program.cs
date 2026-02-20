using Microsoft.EntityFrameworkCore;
using NanoidDotNet;

var builder = WebApplication.CreateBuilder(args);

//Configuração do banco de dados usando Entity Framework Core e Npgsql para PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.MapGet("/", () => "CShortener!");


//Endpoint para gerar um um link encurtado
app.MapPost("/api/shortener", (CreateUrlRequest request, AppDbContext db) =>
{
    //Usando Nanoid para gerar o shortcode aleatório
    var newShortCode = Nanoid.Generate(size: 7);

    //Criação do objeto URL
    var urlObj = new Url(request.OriginalUrl, newShortCode);

    //Salvando alterações no banco de dados
    db.Add(urlObj);
    db.SaveChanges();

    return Results.Ok(new { url = $"{urlObj.ShortCode}" });
});

//Endpoint para redirecionar o usuário para a URL original
app.MapGet("/{shortCode}", (string shortCode, AppDbContext db) =>
{   
    //Busca no banco de dados pelo shortcode
    var urlDb = db.Urls.FirstOrDefault(u => u.ShortCode == shortCode);

    if(urlDb == null)
    {
        return Results.NotFound(new {erro = "Link não encontrado!"});
    }
    else
    {
        urlDb.NewAccess();
        db.SaveChanges();
        return Results.Redirect(urlDb.OriginalUrl);
    }
});

//Endpoint para obter as estatísticas de um link encurtado
app.MapGet("/api/shortener/{shortCode}/stats", (string shortCode, AppDbContext db) =>
{
    var urlDb = db.Urls.FirstOrDefault(u => u.ShortCode == shortCode);

    if(urlDb == null)
    {
        return Results.NotFound(new {erro = "Link não encontrado!"});
    }
    else
    {
        return Results.Ok(new
        {
            urlDb.OriginalUrl,
            urlDb.AccessCount,
            urlDb.CreatedAt
        });
    }
});

app.Run();
