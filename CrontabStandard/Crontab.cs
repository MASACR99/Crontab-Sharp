using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace CrontabStandard
{
    /// <summary>
    /// Crontab class to parse cron format strings and
    /// returns the closest execution time from now
    /// </summary>
    public class Crontab
    {
        private static List<CrontabTimerData> _Callers = new List<CrontabTimerData>();

        /// <summary>
        /// Adds the given method to the callers queue to get called following crontab string,
        /// caller will wait for execution if callAsynchronously is false and won't wait if callAsynchronously is true
        /// </summary>
        /// <param name="crontabString"></param>
        /// <param name="callAsynchronously">Defines if the method given should be called synchronously or asynchronously</param>
        /// <param name="classInstance"></param>
        /// <param name="method"></param>
        /// <param name="methodParameters"></param>
        /// <returns>Guid of the caller</returns>
        /// <exception cref="ArgumentException">If crontab string is invalid or unsupported</exception>
        public static Guid AddProcess(string crontabString, bool callAsynchronously, object classInstance, MethodInfo method, object[] methodParameters)
        {
            double timeToExec = CrontabTimeParser(crontabString, null);
            CrontabTimerData timerData = new CrontabTimerData();
            Timer timer;

            timerData.ClassInstance = classInstance;
            timerData.Method = method;
            timerData.MethodParameters = methodParameters;
            timerData.OriginalCrontabString = crontabString;
            timerData.CallAsynchronously = callAsynchronously;
            timerData.Guid = Guid.NewGuid();

            if (timeToExec >= Int32.MaxValue)
                timer = GetMaxTimer(timerData);
            else
                timer = new Timer(ExecuteMethod, timerData, (int)Math.Round(timeToExec), -1);

            _Callers.Add(timerData);

            return timerData.Guid;
        }

        /// <summary>
        /// Stops and deletes the caller with given Guid
        /// </summary>
        /// <param name="callerID">Guid given by AddProcess method</param>
        /// <returns>true if success, false if caller not found or failed to remove from queue</returns>
        public static bool DeleteProcess(Guid callerID)
        {
            var caller = _Callers.Where(c => c.Guid == callerID).FirstOrDefault();

            if (caller == null) return false;

            caller.Dispose();
            return _Callers.Remove(caller);
        }

        /// <summary>
        /// Executes given method based on parameters given at creation
        /// and checks if next operation is too far away for timer
        /// </summary>
        /// <param name="sender"></param>
        /// <exception cref="ArgumentException">If crontab string is invalid or unsupported</exception>
        private static void ExecuteMethod(object sender)
        {
            var timerData = (CrontabTimerData)sender;

            if (timerData.CallAsynchronously)
                Task.Run(() => timerData.Method.Invoke(timerData.ClassInstance, timerData.MethodParameters));
            else
                timerData.Method.Invoke(timerData.ClassInstance, timerData.MethodParameters);

            double timeToExec = CrontabTimeParser(timerData.OriginalCrontabString, null);

            if (timeToExec >= Int32.MaxValue)
            {
                timerData.Timer = GetMaxTimer(timerData);
            }
            else
            {
                timerData.Timer = new Timer(ExecuteMethod, timerData, (int)Math.Round(timeToExec), Timeout.Infinite);
            }
        }

        /// <summary>
        /// Checks if next iteration is sooner than Int32 milliseconds and changes event to ExecuteMethod
        /// </summary>
        /// <param name="sender"></param>
        /// <exception cref="ArgumentException">If crontab string is invalid or unsupported</exception>
        private static void RequeueProcess(object sender)
        {
            var timerData = (CrontabTimerData)sender;
            double timeToExec = CrontabTimeParser(timerData.OriginalCrontabString, null);

            if (timeToExec < Int32.MaxValue)
            {
                timerData.Timer.Dispose();

                timerData.Timer = new Timer(ExecuteMethod, timerData, (int)Math.Round(timeToExec), Timeout.Infinite);
            }
        }

        /// <summary>
        /// Returns timer with max amount possible for Timer
        /// </summary>
        /// <param name="timerSender"></param>
        /// <returns>Max value timer with same periodicity</returns>
        private static Timer GetMaxTimer(object timerSender)
        {
            return new Timer(RequeueProcess, timerSender, Int32.MaxValue - 1, Int32.MaxValue - 1);
        }

        /// <summary>
        /// Parses the crontab string given and retrieves the closest 
        /// execution based on the crontab rules given.
        /// </summary>
        /// <param name="crontabString"></param>
        /// <param name="initialTimeToCheck">DateTime used to set the starting date to check, useful for checking future executions, if null, DateTime.Now is used</param>
        /// <returns>Total milliseconds to next execution, to be used on timers</returns>
        /// <exception cref="ArgumentException">If crontab string is invalid or unsupported</exception>
        public static double CrontabTimeParser(string crontabString, DateTime? initialTimeToCheck)
        {
            var separatedValues = crontabString.Split(new char[] { ' ' });

            if (separatedValues.Length != 5)
                throw new ArgumentException("Malformed string");

            var minutesList = ParseCrontabChunk(separatedValues[0], 0, 59);
            var hoursList = ParseCrontabChunk(separatedValues[1], 0, 23);
            var daysMonthList = ParseCrontabChunk(separatedValues[2], 1, 31);
            var monthsList = ParseCrontabChunk(separatedValues[3], 1, 12);
            var daysWeekList = ParseCrontabChunk(separatedValues[4], 0, 6);

            DateTime initialDate = initialTimeToCheck ?? DateTime.Now;

            //After that combine and calculate closest execution time delay
            return GetClosestExecutionTimer(initialDate, monthsList, daysMonthList, daysWeekList, hoursList, minutesList);
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
                            date = new DateTime(date.Year, date.Month, date.Day, hour, minutesList.First(), 0);
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
            var chunks = v.Split(',');

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
