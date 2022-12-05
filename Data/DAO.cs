using Microsoft.Bot.Connector.Authentication;
using Microsoft.Graph;
using MyTeamsApp2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REST.Model.ExchangeClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyTeamsApp2.NotifyTimerTrigger;
using File = System.IO.File;

namespace MyTeamsApp2.Data
{
    internal class DAO
    {

        private string restAddress;

        private static DAO instance = null;

        private HttpClient apiClient;

        private readonly string PostTeamEndPoint = "/Api/Team";

        private readonly string GetTeamAndActivityByChannelIdEndPoint = "/Api/Activity/TeamAndActivityByChannelId";

        private DAO()
        {
        }

        public static DAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DAO();
                }
                return instance;
            }
        }

        private HttpClient GetApiClient()
        {
            if (restAddress == null)
            {
                string baseString = "";

                JObject jObject = JObject.Parse(File.ReadAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\restapistring.json")); // Refer dynamically

                baseString = jObject["base"].Value<string>();

                restAddress = baseString;
            }

            apiClient = new HttpClient()
            {
                BaseAddress = new Uri(restAddress)
            };

            return apiClient;
        }

        public HttpResponseMessage PostTeam(string channelId)
        {
            var apiClient = GetApiClient();

            var request = new HttpRequestMessage(HttpMethod.Post, PostTeamEndPoint);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("channelId", channelId);
            request.Content = new StringContent("json", Encoding.UTF8, "application/json");

            return apiClient.SendAsync(request).Result;
        }

        public async Task<ActivityRequestObject> TeamAndActivityByChannelId(string channelId)
        {
            using var client = GetApiClient();
            client.DefaultRequestHeaders.Add("channelId", channelId);
            HttpResponseMessage response = await client.GetAsync(GetTeamAndActivityByChannelIdEndPoint);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ActivityRequestObject>(responseBody);
            }
            else
            {
                return null;
            }
        }

        public async Task<Quote> GetQuoteAsync(string path)
        {
            using var client = GetApiClient();
            Quote quote = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                quote = await response.Content.ReadAsAsync<Quote>();
            }
            return quote;
        }

        public async Task<List<SociolitePoll>> GetCustomPollsForTeam(string teamId)
        {
            using var client = GetApiClient();
            client.DefaultRequestHeaders.Add("teamId", teamId);
            HttpResponseMessage response = await client.GetAsync("http://localhost:5229/Api/Poll" + "/" + teamId);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var tempList = JsonConvert.DeserializeObject<List<RESTSociolitePoll>>(responseBody);

                List<SociolitePoll> finalList = new List<SociolitePoll>();
                foreach (var poll in tempList)
                {
                    finalList.Add(RESTMapper.RESTPollToPoll(poll));
                }
                return finalList;
            }
            else
            {
                return new List<SociolitePoll>();
            }
        }

    }
}
