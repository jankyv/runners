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

namespace runningapp
{
    public class Training
    {
        private List<Track> tracks;
        private int trackCount;

        public Training()
        {
            Tracks = new List<Track>();
            Tracks.Add(new Track());
            trackCount = 0;
        }

        internal List<Track> Tracks { get => tracks; set => tracks = value; }

        public void AddPoint(Location p)
        {
            Tracks[trackCount].AddPoint(p);
        }

        public void Pause()
        {
            Tracks.Add(new Track());
            trackCount++;
        }

        public Track CurrentTrack()
        {
            return Tracks[trackCount];
        }

        public List<PolylineOptions> GetTrainingPolylines()
        {
            List<PolylineOptions> list = new List<PolylineOptions>();
            for(int i = 0; i < tracks.Count; i++)
            {
                PolylineOptions polylineOptions = new PolylineOptions().Clickable(false);
                Track track = tracks[i];
                for(int j = 0; j < track.LocationList.Count; j++)
                {
                    Location locaction = track.LocationList.ElementAt(j);
                    LatLng l = new LatLng(locaction.Latitude, locaction.Longitude);
                    polylineOptions.Add(l);
                }
                list.Add(polylineOptions);
            }
            return list;
        }
    }
}