using Quartz;
using TGBotExample;
using TGBotExample.Services.TimerJobs;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(((context, services) =>
            {
                /*// Add the required Quartz.NET services
                services.AddQuartz(q =>  
                {
                    // Use a Scoped container to create jobs. I'll touch on this later
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    var jobKey = new JobKey("UpdateDatabaseJob");
                    q.AddJob<UpdateDatabaseJob>(opts => opts.WithIdentity(jobKey));
                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .StartAt(DateTimeOffset.Now)
                        .WithIdentity("UpdateDatabaseJob-trigger")
                        .WithCronSchedule("0 5 * * *")); // каждый день в 5 часов
                });

                services.AddQuartzHostedService(
                    q => q.WaitForJobsToComplete = true);*/
            }))
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}