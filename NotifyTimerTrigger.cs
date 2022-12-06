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
using System.Threading;

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
        public async Task Run([TimerTrigger("*/40 * * * * *")] TimerInfo myTimer, ExecutionContext context, CancellationToken cancellationToken)
        {
            // Below runs the trigger every 30 minutes on every weekday - use this to check if the time is right for team
            // [TimerTrigger("0 */30 * * * 0-6")]

            // Below runs it every 30 seconds (for development)
            // [TimerTrigger("*/30 * * * * *")]

            // Below: once every hour
            //0 0 */1 * * *

            ActivityRequestObject data = await DAO.Instance.TeamAndActivityByChannelId("19:5d175fc71c154b1dbde3b8ee066c5131@thread.tacv2"); // MAKE THIS READ FROM CONTEXT.JSON

            if (data != null)
            {
                _log.LogInformation($"Data type is {data.Type}.");
                if (data.IsActive)
                {
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

                    //string timeToRun = data.RecurranceString;
                    string timeToRun = "always"; // For development purposes

                    bool lastActivityWasPoll = false;

                    HttpResponseMessage response = await DAO.Instance.GetLastActivityType("19:5d175fc71c154b1dbde3b8ee066c5131@thread.tacv2"); // READ FROM JSON
                    if (response.IsSuccessStatusCode)
                    {
                        string type = await response.Content.ReadAsStringAsync();
                        lastActivityWasPoll = type.Equals("poll");
                    }

                    // If it is time for the event to occur
                    if (recurranceStringEvaluator.RunNow(timeToRun) && data.IsActive)
                    {
                        _log.LogInformation($"NotifyTimerTrigger is triggered at {DateTime.Now}.");

                        // If the last activity was a poll, we post the results of this first
                        _log.LogInformation($"Last activity was poll: {lastActivityWasPoll}.");
                        if (lastActivityWasPoll)
                        {
                            HttpResponseMessage resultsResponse = await DAO.Instance.GetLastPollResults("19:5d175fc71c154b1dbde3b8ee066c5131@thread.tacv2"); // READ FROM JSON

                            if (resultsResponse.IsSuccessStatusCode)
                            {
                                _log.LogInformation($"JSON: {await resultsResponse.Content.ReadAsStringAsync()}.");

                                PollResultDisplayObject results = JsonConvert.DeserializeObject<PollResultDisplayObject>(await resultsResponse.Content.ReadAsStringAsync());
                                await ShowPollResults(context, cancellationToken, results);
                            }
                        }

                        // If we want to display a poll
                        if (data.Type.Equals("poll"))
                        {
                            await DisplayPoll(context, cancellationToken, customPollProperty);
                        }
                        else if (data.Type.Equals("discussion")) // If it is a discussion
                        {
                            await DisplayDiscussion(context, cancellationToken, customDiscussionProperty);
                        }
                    }
                } else
                {
                    _log.LogInformation($"Team is NOT active. Skipping event.");
                }
            } else
            {
                _log.LogInformation($"Data is null!");
            }
        }

        public async Task ShowPollResults(ExecutionContext context, CancellationToken cancellationToken, PollResultDisplayObject results)
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
                        PollTitle = results.PollQuestion + results.AnswersAndRespondants.Count(),
                        PollQuestion = results.PollQuestion,
                        AnswersList = results.AnswersAndRespondants,
                        PossibleAnswersList = results.PossibleAnswers
                    }
                );
                await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), cancellationToken);
            }
        }

        public async Task DisplayPoll(ExecutionContext context, CancellationToken cancellationToken, CustomPollProperty customPollProperty)
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
                        AnswersList = customPollProperty.getOptionsAsList()
                    }
                );
                await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), cancellationToken);
            }
        }

        public async Task DisplayDiscussion(ExecutionContext context, CancellationToken cancellationToken, CustomDiscussionProperty customDiscussionProperty)
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
