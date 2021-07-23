using System;
using System.Threading;

namespace TimerWheel
{
    class Program
    {
        static void Main(string[] args)
        {
            TimerWheelScheduler timerWheelScheduler = new TimerWheelScheduler();
            long now = TimerWheelScheduler.GetNowTime();
            Random random = new Random();
          
            long starTime = TimerWheelScheduler.GetNowTime();

            int count = 0;
            while (true)
            {
                Thread.Sleep(1);
                timerWheelScheduler.Update();

                long nowTime = TimerWheelScheduler.GetNowTime();
                if (nowTime - starTime >= 1000)
                {
                    starTime = TimerWheelScheduler.GetNowTime();
                    for (int index = 0; index < 10; index++)
                    {
                        count++;
                        timerWheelScheduler.AddTimeoutTimer(() =>
                        {
                            //Console.WriteLine($"index = {(TimerWheelScheduler.GetNowTime() - now) / 1000.0f}s");
                        }, random.Next(index * 100, index * 1000));
                    }
                    timerWheelScheduler.DebugInfo();
                }
            }
        }
    }
}
