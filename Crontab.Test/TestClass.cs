using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crontab.Test
{
    public class TestClass
    {
        private long count = 0;
        private StringBuilder StringBuilder = new StringBuilder();
        private DateTime OriginalTime;
        private static SemaphoreSlim Semaphore = new SemaphoreSlim(1,1);

        public static void Running(int i)
        {
            //count++;
            //if (count % 10000 == 0)
            if (i % 1000000 == 0)
                Console.WriteLine($"Counter = {i}");
            //StringBuilder.AppendLine($"{count}");
        }

        public static void Running1()
        {
            //count++;
            //if (count % 10000 == 0)
            //Console.WriteLine(StringBuilder.ToString());
            //StringBuilder.AppendLine($"{count}");
        }

        //public static void Running1Part2()
        //{
        //    Console.WriteLine(StringBuilder.ToString());
        //    StringBuilder = new StringBuilder();
        //}

        public static void Running2(string test)
        {
            Console.WriteLine($"Test2 {DateTime.Now} {DateTime.Now.Millisecond} {test}");
        }

        public void Running3()
        {
            Console.WriteLine($"Test3 {DateTime.Now} {DateTime.Now.Millisecond}");
        }

        public static async Task RunningAsyncBitch()
        {
            Console.WriteLine($"Shadow wizard money gang {DateTime.Now} {DateTime.Now.Millisecond}");
        }
    }
}
