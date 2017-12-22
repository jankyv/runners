using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Net.HttpWebRequest;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using Java.Lang;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Locations;
using Android.Util;
using Android.Gms.Maps.Model;
using System.Threading;

namespace runningapp
{
    public class getSnappedPoints
    {
        public List<SnappedPoint> result;
        public bool Loaded = false;
        public System.Threading.Thread thread;


        public void getData(PolylineOptions p) {
            string res = "";
            if (p != null)
            {
                for (int i = 0; i < p.Points.Count; i++)
                {
                    res += p.Points[i].Latitude + "," + p.Points[i].Longitude;
                    if (i != p.Points.Count)
                    {
                        res += "|";
                    }

                }
            }
            Log.Info("SnappedPoint", "doinbackground");
            Uri myUri = new Uri(
                                  "https://roads.googleapis.com/v1/snapToRoads?path=" + res + "&interpolate=true&key=AIzaSyASJzv77xkOmtcpdcE7pNVHK7UZSrQmC3o");
            WebRequest request = WebRequest.Create(myUri);
            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            reader.Close();

            var data = Welcome.FromJson(responseFromServer);
            result = data.SnappedPoints;
            Log.Info("SnappedPoint", data.SnappedPoints[0].Location.Latitude.ToString());
            Loaded = true;
            
        }

        public getSnappedPoints(PolylineOptions p)
        {
            ThreadStart newThread = new ThreadStart(delegate { getData(p); });

            thread = new System.Threading.Thread(newThread);
            thread.Start();

        }



    }



    /*public class MyTask : AsyncTask<string, List<SnappedPoint>, List<SnappedPoint>>
    {
        protected MyTask(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {

        }
        public MyTask()
        {

        }

        protected override List<SnappedPoint> RunInBackground(params string[] @params)
        {
             Log.Info("SnappedPoint", "doinbackground");
             Uri myUri = new Uri(
                                   "https://roads.googleapis.com/v1/snapToRoads?path=-35.28194,149.13003&interpolate=true&key=AIzaSyASJzv77xkOmtcpdcE7pNVHK7UZSrQmC3o");
             WebRequest request = WebRequest.Create(myUri);
             WebResponse response = request.GetResponse();

             Stream dataStream = response.GetResponseStream();
             StreamReader reader = new StreamReader(dataStream);

             // Read the content.  
             string responseFromServer = reader.ReadToEnd();

             var data = Welcome.FromJson(responseFromServer);

             return data.SnappedPoints;
        }

        protected override void OnPostExecute(List<SnappedPoint> result)
        {
            Log.Info("Async", result.ToString());

            base.OnPostExecute(result);
        }
    }*/

    

    public partial class Welcome
    {
        [JsonProperty("snappedPoints")]
        public List<SnappedPoint> SnappedPoints { get; set; }
    }

    public partial class SnappedPoint
    {
        [JsonProperty("location")]
        public SnappedLocation Location { get; set; }

        [JsonProperty("originalIndex")]
        public long? OriginalIndex { get; set; }

        [JsonProperty("placeId")]
        public string PlaceId { get; set; }
    }

    public partial class SnappedLocation
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public partial class Welcome
    {
        public static Welcome FromJson(string json) => JsonConvert.DeserializeObject<Welcome>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}