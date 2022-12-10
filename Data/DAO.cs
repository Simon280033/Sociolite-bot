using Microsoft.AspNetCore.Mvc;
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

        private readonly string GetLastActivityTypeEndPoint = "/Api/Activity/LatestActivityType";

        private readonly string PostVoteEndPoint = "/Api/Activity/Vote";

        private readonly string GetActivePollEndPoint = "/Api/Activity/ActivePoll";

        private readonly string GetLastPollResultsEndPoint = "/Api/Activity/LastPollResults";

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

                JObject jObject = JObject.Parse(File.ReadAllText(@"restapistring.json")); // Refer dynamically

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
                throw new Exception();
                return null;
            }
        }

        public async Task<HttpResponseMessage> GetLastActivityType(string channelId)
        {
            using var client = GetApiClient();
            client.DefaultRequestHeaders.Add("channelId", channelId);
            return await client.GetAsync(GetLastActivityTypeEndPoint);
        }

        public async Task<HttpResponseMessage> Vote(string channelId, string userId, int optionNumber)
        {
            var apiClient = GetApiClient();

            var request = new HttpRequestMessage(HttpMethod.Post, PostVoteEndPoint);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("channelId", channelId);
            request.Headers.Add("userId", userId);
            request.Headers.Add("optionNumber", "" + optionNumber);

            request.Content = new StringContent("json", Encoding.UTF8, "application/json");

            return apiClient.SendAsync(request).Result;
        }

        public async Task<HttpResponseMessage> GetActivePoll(string channelId)
        {
            using var client = GetApiClient();
            client.DefaultRequestHeaders.Add("channelId", channelId);
            return await client.GetAsync(GetActivePollEndPoint);
        }

        public async Task<HttpResponseMessage> GetLastPollResults(string channelId)
        {
            using var client = GetApiClient();
            client.DefaultRequestHeaders.Add("channelId", channelId);
            return await client.GetAsync(GetLastPollResultsEndPoint);
        }
    }
}
