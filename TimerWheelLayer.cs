using System.Collections.Generic;

namespace TimerWheel
{
    class TimerWheelLayer
    {
        public int TimerTickIndex;
        public readonly int LayerId;
        public readonly int TimerEnd;
        public readonly int TimerStart;
        public Queue<TimerBase>[] TimerQueueArray;
        private TimerWheelScheduler BindTimerWheelScheduler;
        public TimerWheelLayer(TimerWheelScheduler scheduler, int id, int count, int start, int end)
        {
            this.LayerId = id;
            this.TimerEnd = end;
            this.TimerStart = start;
            BindTimerWheelScheduler = scheduler;
            TimerQueueArray = new Queue<TimerBase>[count];
            for (int index = 0; index < TimerQueueArray.Length; index++)
            {
                TimerQueueArray[index] = new Queue<TimerBase>();
            }
        }

        public bool MoveTickIndex()
        {
            ++this.TimerTickIndex;
            if (this.TimerTickIndex == this.TimerQueueArray.Length)
            {
                this.TimerTickIndex = 0;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"id={LayerId}  count={this.TimerQueueArray.Length}  start={this.TimerStart}  end={this.TimerEnd}";
        }

        public int GetSoltCount()
        {
            return this.TimerQueueArray.Length;}

        public bool AddTimer(TimerBase timer)
        {
            if (timer.TimerTick >= this.TimerStart && timer.TimerTick < this.TimerEnd)
            {
                int idx = this.TimerStart == 0
                    ? timer.TimerTick
                    : (timer.TimerTick - this.TimerStart) / this.TimerStart;


                if (idx + this.TimerTickIndex < this.TimerQueueArray.Length)
                {
                    idx += this.TimerTickIndex;
                    this.TimerQueueArray[idx].Enqueue(timer);
                }
                else
                {
                    idx = this.TimerTickIndex + idx - TimerQueueArray.Length;
                    TimerQueueArray[idx].Enqueue(timer);
                }
                //Console.WriteLine($"id={timer.TimerId} ms={timer.Ms} tick={timer.TimerTick} layer={this.LayerId} layertick={this.TimerTickIndex} index={idx}");
                return true;
            }
            return false;
        }

        public Queue<TimerBase> GetCurrenTimers()
        {
            return this.TimerQueueArray[this.TimerTickIndex];
        }
    }
}
