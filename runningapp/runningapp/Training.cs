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
    class Training
    {
        private List<Track> tracks;


        public Training()
        {
            Tracks = new List<Track>();
            Tracks.Add(new Track());
        }

        internal List<Track> Tracks { get => tracks; set => tracks = value; }

        public void AddPoint(Location p)
        {
            Tracks[Tracks.Count-1].AddPoint(p);
        }

        public void PauseTraining()
        {
            Tracks.Add(new Track());
        }

        public Track CurrentTrack()
        {
            return Tracks[Tracks.Count - 1];
        }
    }
}