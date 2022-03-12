using System.Text;
using Newtonsoft.Json;
using TGBotExample.Models;

namespace TGBotExample.Services;

public class Group
{
    public string id { get; init; }
    public string group { get; init; }
    public string forma { get; set; }
}

public class Parser
{
    private static readonly string kaiUrl = "https://kai.ru/raspisanie";

    public static async Task<string> GetGroupIdAsync(string groupNum)
    {
        string request = kaiUrl +
                       "?p_p_id=pubStudentSchedule_WAR_publicStudentSchedule10&p_p_lifecycle=2&p_p_state=normal&p_p_mode=view&p_p_resource_id=getGroupsURL&p_p_cacheability=cacheLevelPage&p_p_col_id=column-1&p_p_col_count=1&query=" +
                       groupNum;
        using (var httpClient = new HttpClient())
        {
            var responseBody = await (await httpClient.GetAsync(request))
                .EnsureSuccessStatusCode()
                .Content.ReadAsStringAsync();

            var group = JsonConvert.DeserializeObject<List<Group>>(responseBody)!.First();

            return group!.id;
        }
    }
    
    public static async Task<string> GetSheduleAsync(string groupNum)
    {
        string requestUri = kaiUrl +
                         "?p_p_id=pubStudentSchedule_WAR_publicStudentSchedule10&p_p_lifecycle=2&p_p_state=normal&p_p_mode=view&p_p_resource_id=schedule&p_p_cacheability=cacheLevelPage&p_p_col_id=column-1&p_p_col_count=1";
        using (var httpClient = new HttpClient())
        {
            var groupId = await GetGroupIdAsync(groupNum);
            var jsonContent = "{ \"groupId\": {" + groupId + "} }";
            
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestUri),
                
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(requestMessage);

            return await response.Content.ReadAsStringAsync();
        }
    }
}