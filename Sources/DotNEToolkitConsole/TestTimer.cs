using DotNEToolkit.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public static class TestTimer
    {
        public static void CreateTimer()
        {
            TimerHandle timer = TimerUtils.Context.CreateTimer("任务1", TimerGranularities.Second, 10, Task1Callback, "123");

            Thread.Sleep(5 * 60 * 1000);

            TimerUtils.Context.DeleteTimer(timer);
        }

        private static void Task1Callback(TimerHandle timer, object userData)
        {
            Console.WriteLine(string.Format("{0}被执行, userData = {1}", timer.Name, userData));
        }
    }
}
