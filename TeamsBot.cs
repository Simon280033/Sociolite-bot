using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.TeamsFx;
using System.Text;
using System.Text.Json;

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

            //File.WriteAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\test.json", ("{ \"teamsid\":\"") + test + ("\" }"));

            TeamDetails teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, turnContext.Activity.TeamsGetTeamInfo().Id, cancellationToken);

            File.WriteAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\context.json", ("{ \"teamId\":\"") + teamDetails.Id + ("\" }"));

            return Task.CompletedTask;
        }
    }
}
