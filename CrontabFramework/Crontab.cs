using System;
using System.Collections.Generic;

namespace CrontabFramework
{
    /// <summary>
    /// Crontab class to parse cron format strings and
    /// returns the closest execution time from now
    /// </summary>
    public class Crontab
    {
        /// <summary>
        /// Parses the crontab string given and retrieves the closest 
        /// execution based on the crontab rules given.
        /// </summary>
        /// <param name="crontabString"></param>
        /// <returns>Total milliseconds to next execution, to be used on timers</returns>
        /// <exception cref="ArgumentException">If crontab string is invalid or unsupported</exception>
        public static double CrontabTimeParser(string crontabString)
        {
            var separatedValues = crontabString.Split(new char[] { ' ' });

            if (separatedValues.Length != 5)
                throw new ArgumentException("Malformed string");

            var minutesList = ParseCrontabChunk(separatedValues[0], 0, 59);
            var hoursList = ParseCrontabChunk(separatedValues[1], 0, 23);
            var daysMonthList = ParseCrontabChunk(separatedValues[2], 1, 31);
            var monthsList = ParseCrontabChunk(separatedValues[3], 1, 12);
            var daysWeekList = ParseCrontabChunk(separatedValues[4], 0, 6);

            //After that combine and calculate closest execution time delay
            return GetClosestExecutionTimer(DateTime.Now, monthsList, daysMonthList, daysWeekList, hoursList, minutesList);
        }

        /// <summary>
        /// Recursive function used to find the milliseconds to next execution
        /// </summary>
        /// <param name="date"></param>
        /// <param name="monthsList"></param>
        /// <param name="daysMonthList"></param>
        /// <param name="daysWeekList"></param>
        /// <param name="hoursList"></param>
        /// <param name="minutesList"></param>
        /// <returns>Time in milliseconds to closest execution</returns>
        private static double GetClosestExecutionTimer(DateTime date, List<int> monthsList, List<int> daysMonthList, List<int> daysWeekList, List<int> hoursList, List<int> minutesList)
        {
            if (monthsList.Contains(date.Month))
            {
                if (daysMonthList.Contains(date.Day) && daysWeekList.Contains((int)date.DayOfWeek))
                {
                    foreach (var hour in hoursList)
                    {
                        if (date.Hour < hour)
                        {
                            date = new DateTime(date.Year, date.Month, date.Day, hour, minutesList[0], 0);
                            var result = date - DateTime.Now;
                            return result.TotalMilliseconds;
                        }
                        else if (date.Hour == hour)
                        {
                            foreach (var minute in minutesList)
                            {
                                if (date.Minute < minute)
                                {
                                    date = new DateTime(date.Year, date.Month, date.Day, date.Hour, minute, 0);
                                    var result = date - DateTime.Now;
                                    return result.TotalMilliseconds;
                                }
                            }
                        }
                    }
                }
                date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                return GetClosestExecutionTimer(date.AddDays(1), monthsList, daysMonthList, daysWeekList, hoursList, minutesList);
            }
            date = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            return GetClosestExecutionTimer(date.AddMonths(1), monthsList, daysMonthList, daysWeekList, hoursList, minutesList);
        }

        /// <summary>
        /// Each chunk of the string has different possibilities, either just a number to be added or a special character
        /// this function will take care of the special cases and just return a List of those cases
        /// </summary>
        /// <param name="v"></param>
        /// <param name="sequenceStart"></param>
        /// <param name="maxValue"></param>
        /// <returns>List with the different possible values</returns>
        /// <exception cref="Exception"></exception>
        private static List<int> ParseCrontabChunk(string v, int sequenceStart, int maxValue)
        {
            if (int.TryParse(v, out var intResult))
                return ProcessBaseCase(intResult, maxValue);
            else if (v.Length == 1 && v[0] == '*')
                return ProcessStarCase(sequenceStart, maxValue);
            else if (v.Contains('/'))
                return ProcessDivisorCase(v, sequenceStart, maxValue);
            else if (v.Contains('-'))
                return ProcessRangeCase(v, sequenceStart, maxValue);
            else if (v.Contains(','))
                return ProcessListCase(v, sequenceStart, maxValue);
            else
                throw new Exception("Malformed crontab string");
        }

        private static List<int> ProcessBaseCase(int intResult, int maxValue)
        {
            var result = new List<int>();

            if (intResult > maxValue)
                throw new Exception("Malformed crontab string");

            result.Add(intResult);
            return result;
        }

        private static List<int> ProcessStarCase(int sequenceStart, int maxValue)
        {
            var result = new List<int>();

            for (int i = sequenceStart; i <= maxValue; i++)
            {
                result.Add(i);
            }

            return result;
        }

        private static List<int> ProcessDivisorCase(string v, int sequenceStart, int maxValue)
        {
            var result = new List<int>();
            var chunks = v.Split('/');

            int divisor;
            if (chunks[0].Length == 1 && chunks[0] == "*")
            {
            }
            else if (int.TryParse(chunks[0], out int parseInt))
            {
                sequenceStart = parseInt;
            }
            else
            {
                throw new Exception("Malformed crontab string");
            }

            if (int.TryParse(chunks[1], out int intParse))
            {
                if (intParse > maxValue)
                    throw new Exception("Malformed crontab string");

                divisor = intParse;
            }
            else
            {
                throw new Exception("Malformed crontab string");
            }

            for (int i = sequenceStart; i <= maxValue; i++)
            {
                if (i % divisor == 0)
                    result.Add(i);
            }

            return result;
        }

        private static List<int> ProcessRangeCase(string v, int sequenceStart, int maxValue)
        {
            var result = new List<int>();
            var chunks = v.Split('-');

            if (int.TryParse(chunks[0], out int parseInt))
            {
                sequenceStart = parseInt;
            }
            else
            {
                throw new Exception("Malformed crontab string");
            }

            if (int.TryParse(chunks[1], out int intParse))
            {
                if (intParse > maxValue)
                    throw new Exception("Malformed crontab string");

                maxValue = intParse;
            }
            else
            {
                throw new Exception("Malformed crontab string");
            }

            for (int i = sequenceStart; i <= maxValue; i++)
            {
                result.Add(i);
            }

            return result;
        }

        private static List<int> ProcessListCase(string v, int sequenceStart, int maxValue)
        {
            var result = new List<int>();
            var chunks = v.Split(",");

            foreach (var chunk in chunks)
            {
                if (int.TryParse(chunk, out int parseInt))
                    result.Add(parseInt);
                else
                    throw new Exception("Malformed crontab string");
            }

            return result;
        }
    }
}
