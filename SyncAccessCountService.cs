using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;

public class SyncAccessCountService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;

    public SyncAccessCountService(IConnectionMultiplexer redis, IServiceProvider serviceProvider)
    {
        _redis = redis;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //Dados do banco do redis e informaćões do servidor
            var dbRedis = _redis.GetDatabase();
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());

            var clicksKeys = server.Keys(pattern: "clicks:*");

            //Abre o banco caso tenha chaves para salvar
            if (clicksKeys.Any())
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                foreach (var key in clicksKeys)
                {
                    var totalClicks = await dbRedis.StringGetAsync(key);
                    var shortCode = key.ToString().Replace("clicks:", "");
                    var urlDb = db.Urls.FirstOrDefault(u => u.ShortCode == shortCode);
                    if(urlDb != null)
                    {
                        urlDb.AccessCount += (int)totalClicks;
                        await dbRedis.KeyDeleteAsync(key);
                        Console.WriteLine($"--> ShortCode: {shortCode} --> Sync Access Count: {totalClicks}.");
                    }
                }
                db.SaveChanges();
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}