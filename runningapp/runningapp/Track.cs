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
    public class Track
    {
        private List<Location> locationList;

        public List<Location> LocationList { get => locationList; set => locationList = value; }

        public Track()
        {
            LocationList = new List<Location>();
        }

        public void AddPoint(Location p)
        {
            LocationList.Add(p);
        }
    }
}