using System;
using System.Reflection;
using System.Threading;

namespace CrontabFramework
{
    internal class CrontabTimerData
    {
        internal Guid Guid { get; set; }
        internal object ClassInstance { get; set; }
        internal MethodInfo Method { get; set; }
        internal object[] MethodParameters { get; set; }
        internal string OriginalCrontabString { get; set; }
        internal Timer Timer { get; set; }
        internal bool CallAsynchronously { get; set; }

        internal void Dispose()
        {
            Timer?.Dispose();
        }
    }
}
