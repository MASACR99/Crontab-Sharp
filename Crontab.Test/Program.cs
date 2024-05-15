using Crontab.Test;
using System.Diagnostics;

class CrontabTest
{
    static readonly List<string> TestCrontabs = new List<string>
    {
        "* * * * *",
        "4/8 1/5 1/4 1/3 1/2",
        "*/5 */5 */5 */5 */5",
        "0-5 1 2,3,4 * *",
        "5 0 * 8 *",
        "15 14 1 * *",
        "0 22 * * 1-5",
        "23 0-20 * * *",
        "0 0,12 1 */2 *",
        "0 4 8-14 * *"
    };

    static async Task Main(string[] args)
    {
        List<Guid> ids = new List<Guid>();
        for (int i = 0; i < 10000000; i++)
        {
            object[] param = [i];
            ids.Add(CrontabStandard.Crontab.AddProcess("0 15 * * * *", true, null, typeof(TestClass).GetMethod(nameof(TestClass.Running)), param));
        }

        Thread.Sleep(3600000);
    }
}
