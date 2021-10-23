namespace iogame.Simulation
{
    public class TimedThing
    {
        public float TotalSecondsSinceLastExecution = 0f;
        public float IntervalSeconds = 0f;
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
            }
        }
    }
}