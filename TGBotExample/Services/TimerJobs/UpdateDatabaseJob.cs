using Newtonsoft.Json;
using TGBotExample.Models;

namespace TGBotExample.Services.TimerJobs;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

[DisallowConcurrentExecution]
public class UpdateDatabaseJob : IJob
{
    private readonly ILogger<UpdateDatabaseJob> _logger;
    private readonly IServiceProvider _services;
    
    public UpdateDatabaseJob(ILogger<UpdateDatabaseJob> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var db = _services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseRepository>();

        var lessons = await Parser.GetScheduleAsync("4241");
        
        _logger.LogInformation("DB has been updated");
        
        return ;
    }
}