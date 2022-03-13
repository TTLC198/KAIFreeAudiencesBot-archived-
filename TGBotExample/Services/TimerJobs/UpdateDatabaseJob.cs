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

        List<DBModels> dbModelsList = new List<DBModels>();

        try
        {
            for (int i = 8; i < 9; i++)
            {
                foreach (var groupId in await Parser.GetGroupsIdAsync(i.ToString()))
                {
                    var groupSc = await Parser.GetScheduleAsync("22834");
                    foreach (var dbmodelss in groupSc)
                    {
                        var temp = dbmodelss;
                        foreach (var dbmodels in dbmodelss)
                        {
                            var groups = await db.GetGroups();
                            dbModelsList.Add(dbmodels);
                            //await db.CreateLesson(dbmodels, groups.First(gr => gr.id.ToString() == groupId).group_number.ToString());
                        }
                    }
                }
            }
            var groupSchedule = await Parser.GetScheduleAsync("22834");
            var temp1 = dbModelsList;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Something went wrong!\n" + ex.Message);
        }

        _logger.LogInformation("DB has been updated");
        
        return ;
    }
}