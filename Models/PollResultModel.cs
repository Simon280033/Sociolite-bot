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

                List<List<string>> masterList = new List<List<string>>();

                // We make a list for the answers at each index
                for (int i = 0; i < PossibleAnswersList.Count; i++)
                {
                    masterList.Add(new List<string>());
                }

                // We add the names of the people who answered the number to this
                for (int i = 0; i < AnswersList.Count; i++)
                {
                    int indexInMasterList = AnswersList[i].Item1 - 1;

                    masterList[indexInMasterList].Add(AnswersList[i].Item2);
                }

                // We build the string
                for (int i = 0; i < masterList.Count; i++)
                {
                    answersAsString += "(" + (PossibleAnswersList[i]) + ") ";

                    // We iterate over the responders for each answer
                    for (int j = 0; j < masterList[i].Count; j++)
                    {
                        answersAsString += masterList[i][j];
                        if (j != masterList[i].Count - 1)
                        {
                            answersAsString += ", ";
                        }
                    }

                    // Percentage for answer
                    Double pct = 0.0;
                    if (masterList[i].Count > 0)
                    {
                        pct = masterList[i].Count / AnswersList.Count;
                    }
                    answersAsString += " (" + pct + "%)\n\r";
                }
                return answersAsString;
            }
        }

        public List<Tuple<int, string>> AnswersList { get; set; }

        public List<string> PossibleAnswersList { get; set; }

    }
}