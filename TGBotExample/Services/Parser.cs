using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TGBotExample.Models;

namespace TGBotExample.Services;

public class Parser
{
    private static readonly string kaiUrl = "https://kai.ru/raspisanie";

    public static async Task<List<string>> GetGroupsIdAsync(string groupNum)
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

        var groups = JsonConvert.DeserializeObject<List<Group>>(responseBody)!;

        return groups.Select(gr => gr.group_number.ToString()).ToList();
    }

    public static async Task<string> GetScheduleJsonAsync(string groupId)
    {
        using var httpClient = new HttpClient();
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

    public static async Task<IEnumerable<IEnumerable<DBModels>>> GetScheduleAsync(string groupId)
    {
        var jsonText = await GetScheduleJsonAsync(groupId);
        var j = JToken.Parse(jsonText);
        var ps = j.Children();
        var models = ps.Select(n =>
        {
            var model = n.Values() != null ? n.Values().Select(m => m.ToObject<DBModels>()) : null; 
            return model;
        });
        return models!;
    }
}