using System.Timers;

namespace Baco.Debugging
{
    /// <summary>
    /// Status bar string-type. Then the interval elapses it autodisposes
    /// </summary>
    public static partial class StatusBar
    {

        private const int DEFAULT_LIFETIME_MS = 3000;

        private class VolatileString
        {
            public string Text { get; private set; }
            private Timer Timer { get; set; }

            public VolatileString(string text)
            {
                Text = text;
                Timer = new Timer
                {
                    Interval = DEFAULT_LIFETIME_MS
                };
                Timer.Elapsed += TimerElapsed;
                Timer.Start();
            }

            public VolatileString(string text, int lifeTime)
            {
                Text = text;
                Timer = new Timer
                {
                    Interval = lifeTime
                };
                Timer.Elapsed += TimerElapsed;
                Timer.Start();
            }

            private void TimerElapsed(object sender, ElapsedEventArgs e)
            {
                StringStatus.Remove(this);
                UpdateStatus();
            }

        }
    }
}
