using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTeamsApp2
{
    internal class QuickChartUrlBuilder
    {
        public static string GenerateChartUrl(List<Tuple<int, string>> AnswersList, List<string> PossibleAnswersList)
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
    }
}
