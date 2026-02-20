using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//A
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/", () => "CShortener!");

app.MapPost("/api/shortener", (CreateUrlRequest request) =>
{
    var urlObj = new Url(1234, request.OriginalUrl, "abc123");
    return Results.Ok(new { url = $"site.com/{urlObj.ShortCode}" });
});

app.MapGet("/{shortCode}", (string shortCode) =>
{
    //urlObj.NewAcess();
    return Results.Redirect("https://google.com", permanent: false);
});


app.Run();
