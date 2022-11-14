using MyTeamsApp2.Models;
using AdaptiveCards.Templating;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.TeamsFx.Conversation;
using Newtonsoft.Json;

using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;

namespace MyTeamsApp2
{
    public sealed class NotifyTimerTrigger
    {
        private readonly ConversationBot _conversation;
        private readonly ILogger<NotifyTimerTrigger> _log;
        private static HttpClient client = new HttpClient();

        public NotifyTimerTrigger(ConversationBot conversation, ILogger<NotifyTimerTrigger> log)
        {
            _conversation = conversation;
            _log = log;
        }

        public class Quote
        {
            public string _id { get; set; }
            public string content { get; set; }
            public string author { get; set; }
            public string authorSlug { get; set; }
            public int length { get; set; }
            public string[] tags { get; set; }
        }

        static async Task<Quote> GetQuoteAsync(string path)
        {
            Quote quote = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                quote = await response.Content.ReadAsAsync<Quote>();
            }
            return quote;
        }

        [FunctionName("NotifyTimerTrigger")]
        public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer, ExecutionContext context, CancellationToken cancellationToken)
        {
            // Below runs the trigger every weekday at 12:15
            // [TimerTrigger("0 15 12 * * 1-5")]

            // Below runs it every 30 seconds (for development)
            // [TimerTrigger("*/30 * * * * *")]

            // TurnContext turnContext = new TurnContext(_conversation.Adapter, new Microsoft.Bot.Schema.Activity());
            // TeamDetails TeamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, turnContext.Activity.TeamsGetTeamInfo().Id, cancellationToken);

            // Make REST GET request
            Quote quote = await GetQuoteAsync("https://api.quotable.io/random");

            _log.LogInformation($"NotifyTimerTrigger is triggered at {DateTime.Now}.");

            // Read adaptive card template
            var adaptiveCardFilePath = Path.Combine(context.FunctionAppDirectory, "Resources", "NotificationDefault.json");
            var cardTemplate = await File.ReadAllTextAsync(adaptiveCardFilePath, cancellationToken);

            JObject o1 = JObject.Parse(File.ReadAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\test.json"));

            var installations = await _conversation.Notification.GetInstallationsAsync(cancellationToken);
            foreach (var installation in installations)
            {
                // Build and send adaptive card
                var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand
                (
                    new NotificationDefaultModel
                    {
                        Title = quote.content,
                        AppName = quote.author,
                        Description = (string)o1.GetValue("teamsid")
,
                        NotificationUrl = "https://www.adaptivecards.io/",
                    }
                );
                await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), cancellationToken);
            }
        }
    }
}
