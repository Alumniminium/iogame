using System;

namespace server.Simulation
{
    public class TimedThing
    {
        public float TotalSecondsSinceLastExecution;
        public float IntervalSeconds;
        public Action Action;

        public TimedThing(TimeSpan interval, Action action)
        {
            IntervalSeconds = (float)interval.TotalSeconds;
            Action = action;
        }

        public void Update(float dt)
        {
            TotalSecondsSinceLastExecution += dt;
            if (TotalSecondsSinceLastExecution >= IntervalSeconds)
            {
                TotalSecondsSinceLastExecution = 0;
                Action.Invoke();
                // FConsole.WriteLine("TimedThing w/ interval "+IntervalSeconds);
            }
        }
    }
}