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
using Android.Util;

namespace runningapp
{
    public class Training
    {
        private List<Track> tracks;
        private int trackCount;
        

        public Training()
        {
            Log.Info("Training" , "Training is made");
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

        public float GetCurrentDistance()
        {
            float d = 0;
            foreach(Track track in tracks){
                d += track.Distance;
            }
            return d;
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
                Track track = tracks[i];             
                list.Add(track.GetPolyLine());
            }
            return list;
        }
    }
}