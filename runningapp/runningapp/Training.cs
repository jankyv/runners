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
using System.Timers;

namespace runningapp
{
    public class Training
    {
        // Variable voor de naam
        private string name;
        // Lijst voor de verschillende tracks.
        private List<Track> tracks;

        // Variabele om het aantal tracks bij te houden.
        private int trackCount;

        //Variabalen voor de timer om de tijd bij te houden.
        private MyTimer timer;
        int hour, min, sec;
        private string timerText;

        //Constructor, wordt aangeroepen bij start van de Training.
        public Training()
        {
            //Track lijst aanmaken en de eerste Track toevoegen.
            Tracks = new List<Track>();
            Tracks.Add(new Track());
            trackCount = 0;

            //Timer instellen en starten
            timer = new MyTimer();
            
            timer.Start();
        }

        //Getters en setters voor de TrackList en de TimerText
        internal List<Track> Tracks { get => tracks; set => tracks = value; }
        public string TimerText { get => timerText; set => timerText = value; }

        //Methode om een punt aan de huidige track toe te voegen
        public LatLng AddPoint(Location p)
        {
            return Tracks[trackCount].AddPoint(p);
        }

        //Methode om de training te pauseren
        public void Pause()
        {
            //Nieuwe track toevoegen voor een eventueel vervolg van de training.
            Tracks.Add(new Track());
            trackCount++;

            //Stop de timer
            timer.Stop();
        }

        //Methode die de gecombineerde afstand van alle tracks returnt.
        public float GetCurrentDistance()
        {
            float d = 0;
            foreach(Track track in tracks){
                d += track.Distance;
            }
            return d;
        }

        //Methode die de huidige track returnt.
        public Track CurrentTrack()
        {
            return Tracks[trackCount];
        }

        //Methode die alle lijnen van alle tracks in een list returnt (voor eventueel terugkijken van trainingen).
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