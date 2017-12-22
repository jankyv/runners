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

namespace runningapp
{
    class SnapToRoadApi
    {

        static public List<SnappedPoint> SnapToRoad()
        {
            Uri myUri = new Uri(
                                    "https://roads.googleapis.com/v1/snapToRoads?path=-35.28194,149.13003&interpolate=true&key=AIzaSyASJzv77xkOmtcpdcE7pNVHK7UZSrQmC3o");
            WebRequest request = WebRequest.Create(myUri);
            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();

            var data = Welcome.FromJson(responseFromServer);

            Console.WriteLine(data.SnappedPoints);
            return data.SnappedPoints;
        }
    }

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