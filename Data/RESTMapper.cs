using MyTeamsApp2.Models;
using Newtonsoft.Json;

namespace MyTeamsApp2.Data
{
    public class RESTMapper
    {
        public static SociolitePoll RESTPollToPoll(RESTSociolitePoll poll)
        {
            List<string> answers = JsonConvert.DeserializeObject<List<string>>(poll.PollOptions);

            return new SociolitePoll
            {
                Id = "" + poll.Id,
                CreatedById = poll.CreatedBy,
                CreationTime = poll.CreatedAt.ToShortDateString(),
                Question = poll.Question,
                Answers = answers
            };
        }
    }
}
