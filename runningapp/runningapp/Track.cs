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
        //Lijst om de locaties in de track bij te houden
        public List<Location> LocationList { get; private set; }

        //Variabele om de afstand van een Track bij te houden
        public float Distance { get; private set; }

        //Variabele om de lijn van de Track bij te houden
        private PolylineOptions polylineOptions;

        //Constructor
        public Track()
        {
            polylineOptions = new PolylineOptions().Clickable(false);
            LocationList = new List<Location>();
        }

        //Methode om een punt toe te voegen aan de Location, returnt het punt dat is toegevoegd, wanneer niks toegevoegd, return null.
        public LatLng AddPoint(Location p)
        {
            float[] results = new float[1];

            //Wanneer er een vorig punt is.
            if (LocationList.Count > 0)
            {
                //Bereken de afstand tussen het vorige punt en het huidige punt en store het resultaat in results[0].
                Location prevLocation = LocationList.Last();
                Location.DistanceBetween(p.Latitude, p.Longitude, prevLocation.Latitude, prevLocation.Longitude, results);
                Log.Info("Distance", "Distance is: " + results[0]);
                Distance += results[0];

                //Als de afstand groter is dan 2 meter 
                if (results[0] > 2) 
                {
                    //Voeg het punt toe aan de lijst van locaties en aan de lijn en return het punt.
                    LocationList.Add(p);
                    LatLng point = new LatLng(p.Latitude, p.Longitude);
                    polylineOptions.Add(point);
                    Log.Info("Distance", "Distance is: " + Distance.ToString());
                    return point;
                }
                //Als de afstand niet meer dan 2 meter is, return null.
                else
                {
                    return null;
                }
            }
            //Als er geen vorig punt is, voeg het eerste punt toe en return dat.
            else
            {
                LocationList.Add(p);
                LatLng point = new LatLng(p.Latitude, p.Longitude);
                polylineOptions.Add(point);
                return point;
            }
        }

        //Methode die de polyline returnt.
        public PolylineOptions GetPolyLine()
        {   
            return polylineOptions; 
        }
    }
}