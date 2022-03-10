using Telegram.Bot;
using TGBotExample.Models;
using TGBotExample.Services;

namespace TGBotExample;

public class Startup
{
    public IConfiguration Configuration { get; }
    private BotConfiguration BotConfiguration { get; }
    private string ConnectionString { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        BotConfiguration = Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
        ConnectionString = Configuration.GetConnectionString("MainDB");
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var httpClient = new HttpClient();
        //httpClient.Timeout = new TimeSpan(0, 5, 0);
        services.AddHttpClient("dmb_webhook").AddTypedClient<ITelegramBotClient>(client =>
            new TelegramBotClient(BotConfiguration.BotApiKey, httpClient));

        services.AddHostedService<ConfigureWebhook>();
        services.AddTransient<IDatabaseRepository, DatabaseRepository>(provider => new DatabaseRepository(ConnectionString));
        services.AddScoped<HandleUpdateService>();
        services.AddControllers().AddNewtonsoftJson();
        services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            var token = BotConfiguration.BotApiKey;
            endpoints.MapControllerRoute(
                name: "tgbot",
                pattern: $"bot/{token}",
                new {controller = "Webhook", action = "Post"});
        });
    }
}