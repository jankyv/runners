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
using Android.Locations;

namespace runningapp
{
    class Track
    {
        private int TrackId;
        private List<Location> locationList;

        public List<Location> LocationList { get => locationList; set => locationList = value; }

        public void AddPoint(Location p)
        {
            LocationList = new List<Location>();
            LocationList.Add(p);
            Console.WriteLine("GIT");
        }
    }
}