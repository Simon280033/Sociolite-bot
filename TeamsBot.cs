using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Graph;
using Microsoft.TeamsFx;
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
            //var channelId = turnContext.Activity.TeamsGetTeamInfo().Id; // Get's the channel ID - might be useful later?

            // CHANNEL ID is probably useful because we only want ONE bot per team - so we can check if a bot is added to a channel
            // belonging to a team which already has a bot in another channel

            //File.WriteAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\test.json", ("{ \"teamsid\":\"") + test + ("\" }"));

            // Unfortunately, this doesn't seem to work on button press for adaptive card
            var val = turnContext.Activity.Value;

            // Reply to message to bot when vote
            await EvaluateBotMessageAsync(turnContext);

            File.WriteAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\test.json", ("{ \"test\":\"") + Convert.ToString(turnContext.Activity.Text) + ("\" }"));

            TeamDetails teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, turnContext.Activity.TeamsGetTeamInfo().Id, cancellationToken);

            File.WriteAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\context.json", ("{ \"teamId\":\"") + teamDetails.Id + ("\" }"));

            return Task.CompletedTask;
        }

        public static async Task EvaluateBotMessageAsync(ITurnContext turnContext)
        {
            // Get the text of the message if a user sends a message to @bot
            var messageText = turnContext.Activity.Text;

            // Get the id of the user who sent the message
            var userId = turnContext.Activity.CallerId;

            // We remove @Bot from text
            messageText = messageText.Split(new string[] { "</at>" }, StringSplitOptions.None).Last();

            // If last activity was NOT a poll, we reject (get from API)
            bool lastWasPoll = true;

            string replyMessage = "Something went wrong :( try again please";

            int numberOfPossibleAnswers = 3; // Get from API

            if (lastWasPoll)
            {
                // We evaluate if the response makes sense
                // First we check if it can be turned into an integer
                if (int.TryParse(messageText, out _)) {
                    // Then we check if the number is within the range allowed
                    if (Int32.Parse(messageText) > 0 && Int32.Parse(messageText) <= numberOfPossibleAnswers)
                    {
                        // POST vote to API. If the user already has voted, tell him that his vote has been updated, if not tell him that it has been cast
                        replyMessage = "Your vote has succesfully been registered! The results will be unveiled at next Sociolite event";
                    }
                    else
                    {
                        replyMessage = "Your vote '" + messageText + "' is outside the allowed range of 1-" + numberOfPossibleAnswers + ". Please try again!";
                    }
                } else
                {
                    replyMessage = "Sorry, we could not understand your vote attempt :( Please just write the number of the answer you want to give, eg.: '1'";
                }
            }
            else
            {
                replyMessage = "No votes can be cast at the moment, as the current activity is not a poll.";
            }
            await turnContext.SendActivityAsync(replyMessage);
        }
    }
}
