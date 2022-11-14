using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
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
            Test(turnContext, cancellationToken);

        public static Task Test(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var test = turnContext.Activity.TeamsGetTeamInfo().Id;

            File.WriteAllText(@"C:\Users\simon\source\repos\MyTeamsApp2\test.json", ("{ \"teamsid\":\"") + test + ("\" }"));

            return Task.CompletedTask;
        }
    }
}
