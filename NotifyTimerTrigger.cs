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
using static MyTeamsApp2.NotifyTimerTrigger;
using Microsoft.Bot.Schema;
using MyTeamsApp2.Data;
using REST.Model.ExchangeClasses;
using System.Collections.Generic;
using Properties;

namespace MyTeamsApp2
{
    public sealed class NotifyTimerTrigger
    {
        private readonly ConversationBot _conversation;
        private readonly ILogger<NotifyTimerTrigger> _log;
        private static HttpClient client = new HttpClient();
        private RecurranceStringEvaluator recurranceStringEvaluator = new RecurranceStringEvaluator();

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

        [FunctionName("NotifyTimerTrigger")]
        public async Task Run([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer, ExecutionContext context, CancellationToken cancellationToken)
        {
            // Below runs the trigger every 30 minutes on every weekday - use this to check if the time is right for team
            // [TimerTrigger("0 */30 * * * 0-6")]

            // Below runs it every 30 seconds (for development)
            // [TimerTrigger("*/30 * * * * *")]

            // Make REST GET request for recurrance string for team
            // We use this format: 00000000000 -> First 7 are bools, to say if it should run on this day, last is time
            // Example: 10101001230 -> Runs every monday/wednesday/friday at 12:30

            ActivityRequestObject data = await DAO.Instance.TeamAndActivityByChannelId("19:5d175fc71c154b1dbde3b8ee066c5131@thread.tacv2"); // MAKE THIS READ FROM CONTEXT.JSON

            CustomPollProperty customPollProperty = null;
            CustomDiscussionProperty customDiscussionProperty = null;

            if (data.Type.Equals("poll"))
            {
                customPollProperty = JsonConvert.DeserializeObject<CustomPollProperty>(data.Content);
            }
            else
            {
                customDiscussionProperty = JsonConvert.DeserializeObject<CustomDiscussionProperty>(data.Content);
            }

            //string timeToRun = "10101001230"; // This will be fetched from API
            string timeToRun = "always"; // For development purposes

            bool lastActivityWasPoll = true; // This should be fetched from API

            bool isDiscussion = true; // This should be retrieved from the object received from API. If false, it is a poll

            // If it is time for the event to occur
            if (recurranceStringEvaluator.RunNow(timeToRun) && !data.IsActive) // REMOVE ! FROM LAST PART
            {
                Quote quote = await DAO.Instance.GetQuoteAsync("https://api.quotable.io/random");

                _log.LogInformation($"NotifyTimerTrigger is triggered at {DateTime.Now}.");

                // If the last activity was a poll, we post the results of this first
                if (lastActivityWasPoll)
                {
                    var adaptiveCardFilePath = Path.Combine(context.FunctionAppDirectory, "Resources", "PollResults.json");
                    var cardTemplate = await File.ReadAllTextAsync(adaptiveCardFilePath, cancellationToken);

                    var installations = await _conversation.Notification.GetInstallationsAsync(cancellationToken);
                    foreach (var installation in installations)
                    {
                        // Build and send adaptive card
                        var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand
                        (
                            new PollResultModel
                            {
                                PollTitle = "POLL RESULT TEST",
                                PollQuestion = "POLL RESULT QUESTION",
                                AnswersList = new List<Tuple<int, string>> { new Tuple<int, string>(1, "test"), new Tuple<int, string>(1, "test"), new Tuple<int, string>(1, "test"), new Tuple<int, string>(1, "test"), new Tuple<int, string>(1, "test"), new Tuple<int, string>(2, "test") },
                                PossibleAnswersList = new List<string>() { "Hej", "Med", "Dig"}
                            }
                        );
                        await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), cancellationToken);
                    }
                }

                // If we want to display a poll
                if (data.Type.Equals("poll"))
                {
                    var pollAdaptiveCardFilePath = Path.Combine(context.FunctionAppDirectory, "Resources", "PollDefault.json");
                    var cardTemplate = await File.ReadAllTextAsync(pollAdaptiveCardFilePath, cancellationToken);

                    var installations2 = await _conversation.Notification.GetInstallationsAsync(cancellationToken);
                    foreach (var installation in installations2)
                    {
                        // Build and send adaptive card
                        var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand
                        (
                            new PollDefaultModel
                            {
                                PollTitle = customPollProperty.Question,
                                PollQuestion = customPollProperty.Question,
                                AnswersList = new List<string> { quote.author, quote.author, quote.author }
                            }
                        );
                        await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), cancellationToken);
                    }
                } 
                else // If it is a discussion
                {
                    var discussionAdaptiveCardFilePath = Path.Combine(context.FunctionAppDirectory, "Resources", "DiscussionDefault.json");
                    var cardTemplate = await File.ReadAllTextAsync(discussionAdaptiveCardFilePath, cancellationToken);

                    var installations = await _conversation.Notification.GetInstallationsAsync(cancellationToken);
                    foreach (var installation in installations)
                    {
                        // Build and send adaptive card
                        var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand
                        (
                            new DiscussionDefaultModel
                            {
                                DiscussionTopic = customDiscussionProperty.TopicText
                            }
                        );
                        await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), cancellationToken);
                    }
                }
            }
        }
    }
}
