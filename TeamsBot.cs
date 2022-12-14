using AdaptiveCards.Templating;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Graph;
using Microsoft.Graph.TermStore;
using Microsoft.TeamsFx;
using MyTeamsApp2.Data;
using MyTeamsApp2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Properties;
using REST.Model.ExchangeClasses;
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using File = System.IO.File;

namespace MyTeamsApp2
{
    public class TeamsBot : IBot
    {
        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default) =>
            TurnLogic(turnContext, cancellationToken);

        public static async Task<Task> TurnLogic(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Text.Length  > 0) { 
            // Reply to message to bot when vote
            await HandleMessage(turnContext, cancellationToken);
            }

            return Task.CompletedTask;
            
        }

        public static async Task<string> EvaluateBotMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // We remove @Bot from text
            var messageText = turnContext.Activity.Text;

            messageText = messageText.Split(new string[] { "</at>" }, StringSplitOptions.None).Last();

            if (Regex.Replace(messageText.ToLower(), @"\s+", "").Equals("hello"))
            {
                return "setup";
            }

            if (int.TryParse(messageText, out _))
            {
                return "vote";

            }

            if (Regex.Replace(messageText.ToLower(), @"\s+", "").Equals("poll"))
            {
                return "poll";
            }

            return "error";
        }

        public static async Task<string> GetChannelId(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Get team details
            string channelId = "";

            // Read channelID if already set, otherwise set (So notifytimertrigger can access it)
            if (new FileInfo(@"context.json").Length != 0)
            {
                JObject jObject = JObject.Parse(File.ReadAllText(@"context.json")); // Refer dynamically

                channelId = jObject["channelId"].Value<string>();
            }
            else
            {
                TeamDetails teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, null, cancellationToken);

                File.WriteAllText(@"context.json", ("{ \"channelId\":\"") + teamDetails.Id + ("\" }"));

                channelId = teamDetails.Id;
            }

            return channelId;
        }

        public static async Task<HttpResponseMessage> SetupAction(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            TeamDetails teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, null, cancellationToken);

            return DAO.Instance.PostTeam(teamDetails.Id);
        }

        public static async Task<string> VoteAction(ITurnContext turnContext, CancellationToken cancellationToken, string messageText, string userId, string channelId)
        {
            int numberOfPossibleAnswers = 0;

            var pollResponse = await DAO.Instance.GetActivePoll(channelId);
            if (pollResponse.IsSuccessStatusCode)
            {
                numberOfPossibleAnswers = Int32.Parse(await pollResponse.Content.ReadAsStringAsync());
            }

            // We evaluate if the response makes sense
            // First we check if it can be turned into an integer

            if (int.TryParse(messageText, out _))
            {
                // Then we check if the number is within the range allowed
                if (Int32.Parse(messageText) > 0 && Int32.Parse(messageText) <= numberOfPossibleAnswers)
                {
                    // POST vote to API. If the user already has voted, tell him that his vote has been updated, if not tell him that it has been cast
                    var voteResponse = await DAO.Instance.Vote(channelId, userId, Int32.Parse(messageText)); // READ FROM JSON
                    if (voteResponse.IsSuccessStatusCode)
                    {
                        return "Your vote has succesfully been registered! The results will be unveiled at next Sociolite event";
                    } else
                    {
                        return "Failed to register vote. Format was correct, please try again later.";
                    }
                }
                else
                {
                    return "Your vote '" + messageText + "' is outside the allowed range of 1-" + numberOfPossibleAnswers + ". Please try again!";
                }
            }
            else
            {
                return "Sorry, we could not understand your vote attempt :( Please just write the number of the answer you want to give, eg.: '1'";
            }
        }

        public static async Task ShowPollResults(ITurnContext context, CancellationToken cancellationToken, PollResultDisplayObject results)
        {
            var adaptiveCardFilePath = Path.Combine("Resources", "PollResults.json");
            var cardTemplate = await File.ReadAllTextAsync(adaptiveCardFilePath, cancellationToken);

                // Build and send adaptive card
                var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand
                (
                    new PollResultModel
                    {
                        PollTitle = results.PollQuestion,
                        PollQuestion = results.PollQuestion,
                        AnswersList = results.AnswersAndRespondants,
                        PossibleAnswersList = results.PossibleAnswers
                    }
                );

            var reply = MessageFactory.Attachment(new Microsoft.Bot.Schema.Attachment { Content = cardContent }); //pass adaptive card to content
            var result = await context.SendActivityAsync(reply, cancellationToken);
        }

        public static async Task HandleMessage(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string channelId = await GetChannelId(turnContext, cancellationToken);

            // Get the text of the message if a user sends a message to @bot
            var messageText = turnContext.Activity.Text;

            // Get the id of the user who sent the message
            var userId = turnContext.Activity.From.AadObjectId;

            // We remove @Bot from text
            messageText = messageText.Split(new string[] { "</at>" }, StringSplitOptions.None).Last();

            string messageType = await EvaluateBotMessageAsync(turnContext, cancellationToken);

            string replyMessage = "Something went wrong :( try again please";

            if (messageType.Equals("setup"))
            {
                HttpResponseMessage setupResponse = await SetupAction(turnContext, cancellationToken);

                if (setupResponse.IsSuccessStatusCode)
                {
                    replyMessage = await setupResponse.Content.ReadAsStringAsync();
                }
            }

            if (messageType.Equals("vote"))
            {
                try
                {
                    // If last activity was NOT a poll, we reject (get from API)
                    bool lastWasPoll = false;

                HttpResponseMessage response = await DAO.Instance.GetLastActivityType(channelId);
                if (response.IsSuccessStatusCode)
                {
                    string type = await response.Content.ReadAsStringAsync();
                    lastWasPoll = type.Equals("poll");
                } else
                    {
                        throw new Exception();
                    }

                    if (!lastWasPoll)
                    {
                        replyMessage = "No votes can be cast at the moment, as the current activity is not a poll.";
                    } else
                    {
                        replyMessage = await VoteAction(turnContext, cancellationToken, messageText, userId, channelId);
                    }

                } catch (Exception e)
                {
                    replyMessage = e.Message;
                }
            }

            if (messageType.Equals("poll"))
            {
                HttpResponseMessage resultsResponse = await DAO.Instance.GetLastPollResults(channelId);

                if (resultsResponse.IsSuccessStatusCode)
                {

                    PollResultDisplayObject results = JsonConvert.DeserializeObject<PollResultDisplayObject>(await resultsResponse.Content.ReadAsStringAsync());
                    await ShowPollResults(turnContext, cancellationToken, results);
                    return; // Don't send reply message, as we already do on top
                }
            }

            await turnContext.SendActivityAsync(replyMessage);
        }
    }
}
