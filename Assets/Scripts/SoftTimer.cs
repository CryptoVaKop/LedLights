using System;


//namespace AsyncClient
//{
    /// <summary>
    /// Soft timer class.
    /// </summary>
    public struct SoftTimer
    {
        int StartTime; // Timer start time  
        int MatchTime; // Timer match time


        /// <summary>
        /// Start the timer for a specified number of milliseconds.
        /// </summary>
        /// <param name="timeout"> Specified number of milliseconds. </param>
        public void Start(int timeout = 0)
        {
            StartTime = Environment.TickCount;
            MatchTime = StartTime + timeout;
        }


        /// <summary>
        /// Stop timer.
        /// </summary>
        public void Stop()
        {
            MatchTime = int.MaxValue;
        }


        /// <summary>
        /// Check that the timer is matched.
        /// </summary>
        /// <returns> True - timer is matched, otherwise - false. </returns>
        public bool Match()
        {
            return (Environment.TickCount >= MatchTime);
        }


        /// <summary>
        /// Check that the timer is stopped.
        /// </summary>
        /// <returns> True - timer is stopped, otherwise - false. </returns>
        public bool Stopped()
        {
            return (MatchTime == int.MaxValue);
        }


        /// <summary>
        /// Check that the timer is started.
        /// </summary>
        /// <returns> True - timer is started, otherwise - false. </returns>
        public bool Started()
        {
            return (MatchTime != int.MaxValue);
        }


        /// <summary>
        /// Get the number of milliseconds elapsed after the timer matches.
        /// </summary>
        /// <returns> Number of milliseconds. </returns>
        public int OverTime()
        {
            return (Environment.TickCount - MatchTime);
        }


        /// <summary>
        /// Getting the number of milliseconds remaining before the timeout expires.
        /// </summary>
        /// <returns> Number of milliseconds. </returns>
        public int RemainingTime()
        {
            return (MatchTime - Environment.TickCount);
        }

        /// <summary>
        /// Get the number of milliseconds elapsed after the timer starts.
        /// </summary>
        /// <returns> Number of milliseconds. </returns>
        public int PassingTime()
        {
            return (Environment.TickCount - StartTime);
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeout"> Specified number of milliseconds. </param>
        public SoftTimer(int timeout = 0)
        {
            StartTime = Environment.TickCount;
            MatchTime = StartTime + timeout;
        }
    }
//}
