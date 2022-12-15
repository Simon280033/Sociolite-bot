namespace MyTeamsApp2
{
    internal class RecurranceStringEvaluator
    {
        private Dictionary<string, int> Days = new Dictionary<string, int>()
        {
            ["Monday"] = 0,
            ["Tuesday"] = 1,
            ["Wednesday"] = 2,
            ["Thursday"] = 3,
            ["Friday"] = 4,
            ["Saturday"] = 5,
            ["Sunday"] = 6
        };

        public bool RunNow(string reccuranceString)
        {
            DateTime now = DateTime.Now;

            string dayToday = now.DayOfWeek.ToString();
            string hour = now.Hour.ToString();
            string minute = now.Minute.ToString();

            // 1 if true, 0 if false
            int runToday = (int)reccuranceString[Days[dayToday]];
            string runAtHour = getHoursFromString(reccuranceString);
            string runAtMinute = getMinutesFromString(reccuranceString);

            if (runToday > 0)
            {
                if (runAtHour.Equals(hour))
                {
                    if (runAtMinute.Equals(minute))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string getHoursFromString(string reccuranceString)
        {
            string time = reccuranceString.Remove(0, 7);
            return time.Substring(0, time.Length - 2);
        }
        private string getMinutesFromString(string reccuranceString)
        {
            return reccuranceString.Remove(0, 9);
        }
    }
}
