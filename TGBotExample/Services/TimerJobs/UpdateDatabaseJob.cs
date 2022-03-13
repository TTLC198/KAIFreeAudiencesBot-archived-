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

        try
        {
            for (int i = 0; i < 9; i++)
            {
                foreach (var groupId in await Parser.GetGroupsIdAsync(i.ToString()))
                {
                    foreach (var dbmodelss in await Parser.GetScheduleAsync(groupId))
                    {
                        foreach (var dbmodels in dbmodelss)
                        {
                            var groups = await db.GetGroups();
                            await db.CreateLesson(dbmodels,
                                groups.First(gr => gr.id.ToString() == groupId).group_number.ToString());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message);
        }

        _logger.LogInformation("DB has been updated");
        
        return ;
    }
}