using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TGBotExample.Models;

namespace TGBotExample.Services;

public class Group
{
    public string id { get; init; }
    public string group { get; init; }
    public string forma { get; set; }
}

public class Temporary
{
    public string prepodNameEnc { get; set; }
    public string dayDate { get; set; }
    public string audNum { get; set; }
    public string disciplName { get; set; }
    public string buildNum { get; set; }
    public string orgUnitName { get; set; }
    public string dayTime { get; set; }
    public string dayNum { get; set; }
    public string potok { get; set; }
    public string prepodName { get; set; }
    public string disciplNum { get; set; }
    public string orgUnitId { get; set; }
    public string prepodLogin { get; set; }
    public string disciplType { get; set; }
    public string disciplNameEnc { get; set; }
}

public class Parser
{
    private static readonly string kaiUrl = "https://kai.ru/raspisanie";

    public static async Task<string> GetGroupIdAsync(string groupNum)
    {
        using var httpClient = new HttpClient();
        string request = kaiUrl +
                         "?p_p_id=pubStudentSchedule_WAR_publicStudentSchedule10"
                         + "&p_p_lifecycle=2"
                         + "&p_p_state=normal"
                         + "&p_p_mode=view"
                         + "&p_p_resource_id=getGroupsURL"
                         + "&p_p_cacheability=cacheLevelPage"
                         + "&p_p_col_id=column-1"
                         + "&p_p_col_count=1"
                         + "&query=" +
                         groupNum;

        string responseBody = await (await httpClient.GetAsync(request))
            .EnsureSuccessStatusCode()
            .Content.ReadAsStringAsync();

        var group = JsonConvert.DeserializeObject<List<Group>>(responseBody)!.First();

        return group!.id;
    }

    public static async Task<string> GetScheduleJsonAsync(string groupNum)
    {
        using var httpClient = new HttpClient();
        var groupId = await GetGroupIdAsync(groupNum);
        string requestUri = kaiUrl
                            + "?p_p_id=pubStudentSchedule_WAR_publicStudentSchedule10"
                            + "&p_p_lifecycle=2"
                            + "&p_p_resource_id=schedule"
                            + "&groupId=" + groupId;

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(requestUri)
        };

        var response = await httpClient.SendAsync(requestMessage);
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<IEnumerable<IEnumerable<DBModels>>> GetScheduleAsync(string groupNum)
    {
        var jsonText = await GetScheduleJsonAsync(groupNum);
        var j = JToken.Parse(jsonText);
        var ps = j.Children();
        var models = ps.Select(n =>
        {
            var model = n.Values().Select(m => m.ToObject<DBModels>());
            return model;
        });
        return models;
    }
}