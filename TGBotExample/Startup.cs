using Microsoft.Data.SqlClient;
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
        ConnectionString =
            "Server=tcp:tschbot.database.windows.net,1433;Initial Catalog=Schedule;Persist Security Info=False;User ID=k.paul;Password=ZbNU)9*P@D*s7\"\">;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;TRUSTED_CONNECTION = TRUE;Integrated Security=False";
        //Configuration.GetConnectionString("MainDB");
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var httpClient = new HttpClient();
        //httpClient.Timeout = new TimeSpan(0, 5, 0);
        services.AddHttpClient("dmb_webhook").AddTypedClient<ITelegramBotClient>(client =>
            new TelegramBotClient(BotConfiguration.BotApiKey, httpClient));
        services.AddHostedService<ConfigureWebhook>();
        services.AddTransient<IDatabaseRepository, DatabaseRepository>(provider =>
            new DatabaseRepository(ConnectionString));
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