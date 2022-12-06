namespace MyTeamsApp2.Models
{
    public class PollResultDisplayObject
    {
        public string PollQuestion { get; set; }
        public List<Tuple<int, string>> AnswersAndRespondants { get; set; }
        public List<string> PossibleAnswers { get; set; }
    }
}
