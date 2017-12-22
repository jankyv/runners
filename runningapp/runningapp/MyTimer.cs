using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Timers;

namespace runningapp
{
    class MyTimer : Timer
    {
        private int sec;
        private int min;
        private int hour;

        public MyTimer()
        {
            Interval = 1000;
            Elapsed += Timer_Elapsed;
            ResetTimer();
        }

        public string TimerText { get; private set; }


        //Method om de Timer te updaten
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            sec++;
            if (sec == 60)
            {
                min++;
                sec = 0;
            }
            if (min == 60)
            {
                hour++;
                min = 0;
            }
            TimerText = hour + " : " + min + " : " + sec;
        }

        //Method om de timer te resetten
        private void ResetTimer()
        {
            hour = 0;
            min = 0;
            sec = 0;
        }
    }
}