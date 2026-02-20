using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//A
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/", () => "CShortener!");

app.MapPost("/api/shortener", (CreateUrlRequest request, AppDbContext db) =>
{
    var urlObj = new Url(1234, request.OriginalUrl, "abc123");

    db.Add(urlObj);
    db.SaveChanges();

    return Results.Ok(new { url = $"site.com/{urlObj.ShortCode}" });
});

app.MapGet("/{shortCode}", (string shortCode, AppDbContext db) =>
{   
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


app.Run();
