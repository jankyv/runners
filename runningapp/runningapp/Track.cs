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
using Android.Gms.Maps.Model;
using Android.Gms.Maps;
using Android.Util;

namespace runningapp
{
    public class Track
    {
        private List<Location> locationList;

        public List<Location> LocationList { get => locationList; set => locationList = value; }
        public float Distance { get => distance; set => distance = value; }

        private PolylineOptions polylineOptions;

        private float distance;

        public Track()
        {
            polylineOptions = new PolylineOptions().Clickable(false);
            LocationList = new List<Location>();
        }

        public void AddPoint(Location p)
        {
            float[] results = new float[1];
            if (LocationList.Count > 0)
            {            
                Location prevLocation = LocationList.Last();
                Location.DistanceBetween(p.Latitude, p.Longitude, prevLocation.Latitude, prevLocation.Longitude, results);

                Log.Info("Distance", "Distance is: " + results[0]);
                Distance += results[0];
            }
            else
            {
                LocationList.Add(p);
                polylineOptions.Add(new LatLng(p.Latitude, p.Longitude));

            }

            if (results[0] > 2) // groter dan 1 meter
            {
                LocationList.Add(p);
                polylineOptions.Add(new LatLng(p.Latitude, p.Longitude));
                Log.Info("Distance", "Distance is: " + Distance.ToString());
            }

            Console.WriteLine("Track is called");
            Console.WriteLine(LocationList.Count);

        }

        public PolylineOptions GetPolyLine()
        {   
            return polylineOptions; 
        }
    }
}