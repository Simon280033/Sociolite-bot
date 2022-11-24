namespace MyTeamsApp2.Models
{
    public class PollResultModel
    {
        public string PollTitle { get; set; }

        public string PollQuestion { get; set; }

        public string Answers { 
            get
            {
                string answersAsString = "";

                for (int i = 0; i < AnswersList.Count; i++)
                {
                    answersAsString += "(" + (i+1) + ")" +  AnswersList[i] + "\n\r";
                }
                return answersAsString;
            }
        }

        public List<string> AnswersList { get; set; }

    }
}