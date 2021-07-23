using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace TimerWheel
{
    public class TimerIdHelper
    {
        private int mTimerIndex = 100;
        private Queue<int> mTimerIdQueue = new Queue<int>();

        public int CreateTimerId()
        {
            if (mTimerIdQueue.Count > 0)
            {
                return mTimerIdQueue.Dequeue();
            }

            return mTimerIndex++;
        }

        public void RecycleTimerId(int id)
        {
            if (id != 0)
            {
                mTimerIdQueue.Enqueue(id);
            }
        }
    }
}

namespace TimerWheel
{
    public class TimerWheelScheduler
    {
        private long NextWheelTime { get; set; } //下次轮询时间
        private readonly TimerWheelLayer[] TimerLayers;
        public readonly int mTimerAccuracy = 20; //精准度 毫秒
        private readonly TimerIdHelper mTimerIdHelper = new TimerIdHelper();
        private Dictionary<long, TimerBase> mTimerDict = new Dictionary<long, TimerBase>();

        public TimerWheelScheduler(int layercount = 5, int firstLayerCount = 1 << 8, int otherLayerCount = 1 << 5)
        {
            NextWheelTime = GetNowTime() + this.mTimerAccuracy;

            TimerLayers = new TimerWheelLayer[layercount];
            for (int index = 0; index < TimerLayers.Length; index++)
            {
                int count = index == 0 ? firstLayerCount : otherLayerCount;
                int start = index == 0
                    ? 0
                    : firstLayerCount *
                      (int) Math.Pow(otherLayerCount, index - 1);

                int end = firstLayerCount * (int) Math.Pow(otherLayerCount, index);

                TimerLayers[index] = new TimerWheelLayer(this, index, count, start, end);
            }
        }

        public static long GetNowTime()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        private bool AddTimer(TimerBase timer)
        {
            if (!this.mTimerDict.ContainsKey(timer.TimerId))
            {
                return false;
            }

            long ms = timer.TargetTime - GetNowTime();
            timer.TimerTick = (int) (ms / this.mTimerAccuracy); //Math.Max(0, (int) ms / this.mTimerAccuracy);
            Debug.Assert(timer.TimerTick >= 0);
            for (int index = 0; index < this.TimerLayers.Length; index++)
            {
                TimerWheelLayer layer = this.TimerLayers[index];
                if (layer.AddTimer(timer))
                {
                    return true;
                }
            }

            throw new Exception($"add timer fail ms = {timer.Ms}");
        }

        public int AddTimeoutTimer<T>(Action<T> action, T par, int ms)
        {
            if (action == null)
            {
                return 0;
            }

            if (ms < this.mTimerAccuracy)
            {
                action?.Invoke(par);
                return 0;
            }

            int timerId = mTimerIdHelper.CreateTimerId();
            TimerBase timer = new Timer<T>(timerId, ms, action, par);

            this.mTimerDict.Add(timerId, timer);
            return this.AddTimer(timer) ? timerId : 0;
        }

        public int AddTimeoutTimer<T1, T2>(Action<T1, T2> action, T1 par1, T2 par2, int ms)
        {
            if (action == null)
            {
                return 0;
            }

            if (ms < this.mTimerAccuracy)
            {
                action?.Invoke(par1, par2);
                return 0;
            }

            int timerId = mTimerIdHelper.CreateTimerId();
            TimerBase timer = new Timer<T1, T2>(timerId, ms, action, par1, par2);

            this.mTimerDict.Add(timerId, timer);
            return this.AddTimer(timer) ? timerId : 0;
        }

        public int AddTimeoutTimer(Action action, int ms)
        {
            if (action == null)
            {
                return 0;
            }

            if (ms < this.mTimerAccuracy)
            {
                action.Invoke();
                return 0;
            }

            int timerId = mTimerIdHelper.CreateTimerId();
            TimerBase timer = new Timer(timerId, ms, action);

            this.mTimerDict.Add(timerId, timer);
            return this.AddTimer(timer) ? timerId : 0;
        }

        public bool RemoveTimer(int id)
        {
            if (this.mTimerDict.ContainsKey(id))
            {
                this.mTimerDict.Remove(id);
                this.mTimerIdHelper.RecycleTimerId(id);
                return true;
            }

            return false;
        }

        public int GetTimerCount()
        {
            return this.mTimerDict.Count;
        }

        private int normalCount = 0;
        private int timeoutCount = 0;

        public void DebugInfo()
        {
            int sumCount = timeoutCount + normalCount;
            Console.ForegroundColor = ConsoleColor.Cyan;
            float value1 = timeoutCount / (float) sumCount;
            float value2 = normalCount / (float) sumCount;
            Console.WriteLine($"timeout={value1 * 100:0.00}%   normal={value2 * 100:0.00}%");
            //this.timeoutCount = this.normalCount = 0;
        }

        public void Update()
        {
            long nowTime = GetNowTime();
            long value = nowTime - this.NextWheelTime;

            if (value < (this.mTimerAccuracy - 2))
            {
                return;
            }

            long count = Math.Max(1, value / this.mTimerAccuracy);
            this.NextWheelTime = nowTime + this.mTimerAccuracy - value;
            for (int i = 0; i < count; i++)
            {
                TimerWheelLayer firstLayer = this.TimerLayers[0];

                Queue<TimerBase> currentTimers = firstLayer.GetCurrenTimers();

                while (currentTimers.Count > 0)
                {
                    TimerBase timer = currentTimers.Dequeue();
                    if (this.mTimerDict.ContainsKey(timer.TimerId))
                    {
                        timer.Invoke();
                        timer.DebugTimerInfo();
                        if (timer.IsTimeOut())
                        {
                            this.timeoutCount++;
                        }
                        else
                        {
                            this.normalCount++;
                        }

                        this.RemoveTimer(timer.TimerId);
                    }
                }

                if (firstLayer.MoveTickIndex())
                {
                    for (int index = 1; index < this.TimerLayers.Length; index++)
                    {
                        TimerWheelLayer layer = this.TimerLayers[index];

                        Queue<TimerBase> moveTimers = layer.GetCurrenTimers();

                        bool res = layer.MoveTickIndex();

                        while (moveTimers.Count > 0)
                        {
                            TimerBase timer = moveTimers.Dequeue();
                            this.AddTimer(timer);
                        }

                        if (res == false)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}