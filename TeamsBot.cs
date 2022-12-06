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
using Newtonsoft.Json;
using Properties;
using REST.Model.ExchangeClasses;
using System;
using System.Text;
using System.Text.Json;
using File = System.IO.File;

namespace MyTeamsApp2
{
    /// <summary>
    /// An empty bot handler.
    /// You can add your customization code here to extend your bot logic if needed.
    /// </summary>
    public class TeamsBot : IBot
    {
        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default) =>
            SetTeamId(turnContext, cancellationToken);

        public static async Task<Task> SetTeamId(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Text.Length  > 0) { 
            // Reply to message to bot when vote
            await EvaluateBotMessageAsync(turnContext, cancellationToken);
            }

            /*
            TeamDetails teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, null, cancellationToken);

            File.WriteAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\context.json", ("{ \"teamId\":\"") + teamDetails.Id + ("\" }"));
            */

            return Task.CompletedTask;
        }

        public static async Task EvaluateBotMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Get the text of the message if a user sends a message to @bot
            var messageText = turnContext.Activity.Text;

            // Get the id of the user who sent the message
            var userId = turnContext.Activity.From.AadObjectId;

            // We remove @Bot from text
            messageText = messageText.Split(new string[] { "</at>" }, StringSplitOptions.None).Last();

            // If last activity was NOT a poll, we reject (get from API)
            bool lastWasPoll = false;

            HttpResponseMessage response = await DAO.Instance.GetLastActivityType("19:5d175fc71c154b1dbde3b8ee066c5131@thread.tacv2"); // READ FROM JSON
            if (response.IsSuccessStatusCode)
            {
                string type = await response.Content.ReadAsStringAsync();
                lastWasPoll = type.Equals("poll");
            }

            bool setupMessage = false;

            string replyMessage = "Something went wrong :( try again please";

            if (messageText.ToLower().Contains("hello"))
            {
                TeamDetails teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, null, cancellationToken);

                HttpResponseMessage setupResponse = DAO.Instance.PostTeam(teamDetails.Id);

                if (setupResponse.IsSuccessStatusCode)
                {
                    replyMessage = await setupResponse.Content.ReadAsStringAsync();

                    setupMessage = true;
                    lastWasPoll = false;
                }
            }

            if (lastWasPoll && !setupMessage)
            {
                int numberOfPossibleAnswers = 0;

                var pollResponse = await DAO.Instance.GetActivePoll("19:5d175fc71c154b1dbde3b8ee066c5131@thread.tacv2"); // READ FROM JSON
                if (pollResponse.IsSuccessStatusCode)
                {
                    numberOfPossibleAnswers = Int32.Parse(await pollResponse.Content.ReadAsStringAsync());
                }

                // We evaluate if the response makes sense
                // First we check if it can be turned into an integer
                try
                {
                if (int.TryParse(messageText, out _)) {
                    // Then we check if the number is within the range allowed
                    if (Int32.Parse(messageText) > 0 && Int32.Parse(messageText) <= numberOfPossibleAnswers)
                    {
                        // POST vote to API. If the user already has voted, tell him that his vote has been updated, if not tell him that it has been cast
                        var voteResponse = await DAO.Instance.Vote("19:5d175fc71c154b1dbde3b8ee066c5131@thread.tacv2", userId, Int32.Parse(messageText)); // READ FROM JSON
                        if (voteResponse.IsSuccessStatusCode)
                        {
                            replyMessage = "Your vote has succesfully been registered! The results will be unveiled at next Sociolite event";
                        }
                        replyMessage = await voteResponse.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        replyMessage = "Your vote '" + messageText + "' is outside the allowed range of 1-" + numberOfPossibleAnswers + ". Please try again!";
                    }
                } else
                {
                    replyMessage = "Sorry, we could not understand your vote attempt :( Please just write the number of the answer you want to give, eg.: '1'";
                }
            } catch (Exception e)
                {
                    replyMessage = e.Message;
                }
        }
            else if (!lastWasPoll && !setupMessage)
            {
                replyMessage = "No votes can be cast at the moment, as the current activity is not a poll.";
            }
            await turnContext.SendActivityAsync(replyMessage);
        }
    }
}
