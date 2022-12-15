namespace MyTeamsApp2.Models
{
    public class PollResultModel
    {
        public string PollTitle { get; set; }

        public string PollQuestion { get; set; }

        public string ChartImage => GenerateChartUrl();

        public string GenerateChartUrl()
        {
            string defaultUrlRoot = "https://quickchart.io/chart?c=";

            string labels = "";

            // We build labels section:
            foreach (var possibleAnswer in PossibleAnswersList)
            {
                labels += "'" + possibleAnswer + "'";

                // If not last, we put comma
                if (PossibleAnswersList.IndexOf(possibleAnswer) != PossibleAnswersList.Count - 1)
                {
                    labels += ", ";
                }
            }

            string labelsUrlPart = "{type:'bar',data:{labels:[" + labels + "], ";

            string dataSets = "";

            // We make a list holding lists with answers for each question
            List<int> VotesForAnswers = new List<int>();

            // We go over the answers, and add 0 every time there is a new answer number
            for (int i = 0; i < PossibleAnswersList.Count; i++)
            {
                VotesForAnswers.Add(0);
            }

            // Then we append to the appropriate index
            for (int i = 0; i < AnswersList.Count; i++)
            {
                VotesForAnswers[AnswersList[i].Item1 - 1]++;
            }

            // Then we build the string
            for (int i = 0; i < VotesForAnswers.Count; i++)
            {
                dataSets += "" + VotesForAnswers[i];

                // If not last, we put comma
                if (i != VotesForAnswers.Count - 1)
                {
                    dataSets += ",";
                }
            }

            string datasetsUrlPart = "datasets:[{label:'Votes',data:[" + dataSets + "]}]}}";

            return defaultUrlRoot + labelsUrlPart + datasetsUrlPart;
        }

        public string Answers
        {
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
                    answersAsString += "**" + (PossibleAnswersList[i]) + "** - ";

                    // If nobody voted for this option
                    if (masterList[i].Count < 1)
                    {
                        answersAsString += "No votes!\n\r";
                    }
                    else
                    {
                        // We iterate over the responders for each answer
                        for (int j = 0; j < masterList[i].Count; j++)
                        {
                            answersAsString += masterList[i][j];
                            if (j != masterList[i].Count - 1)
                            {
                                answersAsString += ", ";
                            }
                        }
                        answersAsString += "\n\r";
                    }
                }
                return answersAsString;
            }
        }

        public List<Tuple<int, string>> AnswersList { get; set; }

        public List<string> PossibleAnswersList { get; set; }

    }
}