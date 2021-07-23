using System;

namespace TimerWheel
{
    public class TimerBase
    {
        public int Ms;
        public int TimerTick;
        public readonly int TimerId;
        public readonly long StartTime;
        public readonly long TargetTime;

        public TimerBase(int id, int ms)
        {
            this.Ms = ms;
            this.TimerId = id;
            this.StartTime = TimerWheelScheduler.GetNowTime();
            this.TargetTime = this.StartTime + ms;
        }
        public virtual void Invoke() { }

        public bool IsTimeOut()
        {
            long t = TimerWheelScheduler.GetNowTime() - StartTime;
            return Math.Abs(t - this.Ms) >= 20;
        }

        public void  DebugTimerInfo()
        {
            long t = TimerWheelScheduler.GetNowTime() - StartTime;
            if (Math.Abs(t - this.Ms) > 20)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{this.TimerId} ms = {this.Ms}  error value = {t - this.Ms}ms");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine($"{this.TimerId} ms = {this.Ms}  error value = {t - this.Ms}ms");
            }
        }
    }

    public class Timer : TimerBase
    {
        private readonly Action BindAction;
        public Timer(int id, int ms, Action action) : base(id, ms)
        {
            this.BindAction = action;
        }

        public override void Invoke()
        {
            this.BindAction?.Invoke();
        }
    }


    public class Timer<T> : TimerBase
    {
        private readonly T Paramate;
        private readonly Action<T> BindAction;
        public Timer(int id, int ms, Action<T> action, T paramate) : base(id, ms)
        {
            this.Paramate = paramate;
            this.BindAction = action;
        }

        public override void Invoke()
        {
            this.BindAction?.Invoke(Paramate);
        }
    }

    public class Timer<T1, T2> : TimerBase
    {
        private readonly T1 Paramate1;
        private readonly T2 Paramate2;
        private readonly Action<T1, T2> BindAction;
        public Timer(int id, int ms, Action<T1, T2> action, T1 par1, T2 par2) : base(id, ms)
        {
            this.Paramate1 = par1;
            this.Paramate2 = par2;
            this.BindAction = action;
        }

        public override void Invoke()
        {
            this.BindAction?.Invoke(Paramate1, Paramate2);
        }
    }
}
