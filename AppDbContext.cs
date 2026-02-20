using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)  : base(options)
    {
    }

    public DbSet<Url> Urls {get; set;}
}