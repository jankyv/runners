using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Java.Lang;
using Android.Gms.Maps.Model;
using static Android.Gms.Maps.GoogleMap;
using Android.Locations;
using System.Drawing;
using Android.Content.Res;
using Android.Animation;
using System.Diagnostics;
using System.Timers;

namespace runningapp
{
    public class GoogleMapFragment : Fragment, IOnMapReadyCallback
    {
        // Variabele voor de Google Map
        private MapView mMapView;
        private GoogleMap googleMap;
        private static int ZOOM = 15;

        // Variabele voor afmetingen van het scherm
        private DisplayMetrics metrics;

        // UI Variabelen
        private Button recenter;
        private ImageButton stopButton;
        private LinearLayout contentLayout;
        private RelativeLayout mapsLayout;
        private ImageButton startButton;
        private LinearLayout leftLayout;
        private LinearLayout rightLayout;
        private TextView stopWatchText;
        private LinearLayout bottomLayout;
        private RelativeLayout masterLayout;
        private TextView distanceText;


        private MyTimer timer;

        //variabele voor de training
        Training training;

        // Interface variabele
        private IOnMapControlClick mListener;


        // bool variabelen voor het bepalen van de eerste start en of de training gaande is of niet
        public bool inTraining;
        public bool firstStart;

        //variabelen voor de stopwatch
        private int sec;
        private int min;
        private int hour;
        public string TimerText { get; private set; }

        // Methode om alle variabelen toe te wijzen en de UI op te zetten
        private void SetUpVariables()
        {
            // bool variabelen
            inTraining = false;
            firstStart = true;

            // UI variabelen
            metrics = Resources.DisplayMetrics;
            contentLayout = Activity.FindViewById<LinearLayout>(Resource.Id.content_layout);
            mapsLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.maps_layout);

            recenter = Activity.FindViewById<Button>(Resource.Id.zoomToLoc);
            startButton = Activity.FindViewById<ImageButton>(Resource.Id.startTraining);

            contentLayout.LayoutParameters.Height = ViewGroup.LayoutParams.WrapContent;      
            mapsLayout.LayoutParameters.Height = (int)(metrics.HeightPixels - contentLayout.LayoutParameters.Height);

            leftLayout = Activity.FindViewById<LinearLayout>(Resource.Id.layout_left);
            rightLayout = Activity.FindViewById<LinearLayout>(Resource.Id.layout_left);
            stopButton = Activity.FindViewById<ImageButton>(Resource.Id.stopTraining);
            distanceText = Activity.FindViewById<TextView>(Resource.Id.distanceText);
            
            stopWatchText = Activity.FindViewById<TextView>(Resource.Id.stopWatchView);
            bottomLayout = Activity.FindViewById<LinearLayout>(Resource.Id.stats_layout);

            masterLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.master_layout);
            masterLayout.RemoveView(bottomLayout);
                
            // Roep method aan om de layout te organiseren om een run te kunnen starten
            LayoutToStart();

            // Timer opzetten
            timer = new MyTimer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            ResetTimer();


            recenter.Click += delegate
            {
                mListener.OnRecenterClick();             
            };

            startButton.Click += delegate
            {         
                if (inTraining)
                {
                    this.PauseTraining();
                }
                else
                {
                    if (firstStart)
                    {
                        if (mListener.LocationIsOn())
                        {
                            this.StartTraining();
                        }
                    }
                    else
                    {
                        if (mListener.LocationIsOn())
                        {
                            this.ResumeTraining();
                        }                       
                    }
                   
                }
            };

            stopButton.Click += delegate {
                // notify user
                Android.Support.V7.App.AlertDialog.Builder dialog = new Android.Support.V7.App.AlertDialog.Builder(Activity);
                dialog.SetMessage("Delete Training?");
                dialog.SetPositiveButton("Delete", delegate
                {
                    this.StopTraining();
                });

                dialog.SetNegativeButton("Cancel", delegate {

                });

                dialog.Show();
                
            };


        }

       


        private void LayoutToStart()
        {
            leftLayout.LayoutParameters = new LinearLayout.LayoutParams(0, WindowManagerLayoutParams.WrapContent, 2f);
            startButton.SetImageResource(Resource.Mipmap.ic_play_arrow_black_24dp);      
        }

        private void LayoutPaused()
        {

            leftLayout.LayoutParameters = new LinearLayout.LayoutParams(0, WindowManagerLayoutParams.WrapContent, 1f);
            leftLayout.LayoutParameters = new LinearLayout.LayoutParams(0, WindowManagerLayoutParams.WrapContent, 1f);
            startButton.SetImageResource(Resource.Mipmap.ic_play_arrow_black_24dp);
        }

        private void LayoutTraining()
        {
            leftLayout.LayoutParameters = new LinearLayout.LayoutParams(0, WindowManagerLayoutParams.WrapContent, 2f);
            startButton.SetImageResource(Resource.Mipmap.ic_pause_black_24dp);
        }

        private void StartTraining()
        {
            LayoutTraining();
            masterLayout.AddView(bottomLayout);
            timer.Start();
            firstStart = false;
            training = new Training();
            inTraining = true;
            ToastText("Training Started");

        }

        private void PauseTraining()
        {
            LayoutPaused();
            inTraining = false;
            timer.Stop();
            training.Pause();
            ToastText("Training Paused");

        }

        private void ResumeTraining()
        {
            LayoutTraining();
            inTraining = true;
            timer.Start();
            ToastText("Training Resumed");

        }

        private void StopTraining()
        {
            ToastText("Training Stopped");
            LayoutToStart();
            inTraining = false;
            timer.Stop();
            stopWatchText.Text = "00:00:00";
            googleMap.Clear();
            masterLayout.RemoveView(bottomLayout);
        }

        //Methode om vast te stellen of een verandering van locatie significant is, zo ja, voeg het punt toe aan de training en teken een lijn
        public void AddLocation(Location location)
        {
            //Wanneer in training, geef locatie door aan training
            if (inTraining)
            {
                //Wanneer de locatie verandering significant is, is current de LatLng van de locatie
                LatLng current = training.AddPoint(location);

                //Wanneer de verandering significant is, voeg een lijn toe van het vorige punt naar het huidige punt
                if (current != null)
                {
                    AddLine(current);
                }
            }
        }

        private void AddLine(LatLng current)
        {
            PolylineOptions polyLineOptions = new PolylineOptions();
            polyLineOptions.InvokeWidth(20);
            polyLineOptions.InvokeColor(Resource.Color.line_color_1);
            List<Location> list = training.CurrentTrack().LocationList;
            if (list.Count > 1)
            {

                LatLng prevPoint = new LatLng(list[list.Count - 2].Latitude, list[list.Count - 2].Longitude);
                Log.Info("plek vorige", prevPoint.ToString());
                Log.Info("plek huidige", current.ToString());

                polyLineOptions.Add(prevPoint);
                polyLineOptions.Add(current);
                googleMap.AddPolyline(polyLineOptions);
                Log.Info("plek kleur", polyLineOptions.Color.ToString());
                SetDistanceText(training.GetCurrentDistance());
            }
        }

        public void ZoomToLocation(Location loc)
        {
            LatLng location = new LatLng(loc.Latitude, loc.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(location);
            builder.Zoom(ZOOM);
            builder.Bearing(1);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            if (googleMap != null)
            {
                googleMap.AnimateCamera(cameraUpdate);
            }
        }


      

        public void SetDistanceText(float d)
        {
            distanceText.Text = Java.Lang.String.ValueOf((int)d) + " meter ";
        }

       
        
     
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            SetUpVariables();
        }

        public override void OnAttach(Activity context)
        {
            base.OnAttach(context);
            mListener = (IOnMapControlClick)context;        
        }

        public override void OnDetach()
        {
            base.OnDetach();
            mListener = null;
        }



        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_maps, container, false);

            mMapView = (MapView)rootView.FindViewById(Resource.Id.mapView);
            mMapView.OnCreate(savedInstanceState);

            mMapView.OnResume(); 

            try
            {
                MapsInitializer.Initialize(Activity.ApplicationContext);
            }
            catch (Throwable e)
            {
                e.PrintStackTrace();
            }

            mMapView.GetMapAsync(this);
            // Get the button view 
            View par = ((View)mMapView.FindViewById(1).Parent);
            View LocationButton = par.FindViewById(2);
            LocationButton.LayoutParameters.Height = 0;
            return rootView;
        }


        

        public void OnMapReady(GoogleMap mMap)
        {
            googleMap = mMap;

            try
            {
                // Customise the styling of the base map using a JSON object defined
                // in a raw resource file.
                bool success = googleMap.SetMapStyle(
                        MapStyleOptions.LoadRawResourceStyle(
                                Activity, Resource.Raw.style_json));

                if (!success)
                {
                    Log.Info("Error", "Style parsing failed.");
                }
            }
            catch (Resources.NotFoundException e)
            {
                Log.Info("Error", "Can't find style. Error: ");
            }

            googleMap.MyLocationEnabled = true;
            googleMap.UiSettings.CompassEnabled = true;
            googleMap.SetPadding(0,500,0,0);
         
        }



        public override void OnPause()
        {
            base.OnPause();
            mMapView.OnPause();
        }

        public override void OnResume()
        {
            base.OnResume();
            mMapView.OnResume();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            mMapView.OnDestroy();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            mMapView.OnLowMemory();
        }

        public void DisplayFullTraining(Training training)
        {
            List<PolylineOptions> list = training.GetTrainingPolylines();

            foreach (PolylineOptions l in list)
            {
                googleMap.AddPolyline(l);
            }
        }


        //Method om de Timer te updaten
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            sec++;
            if (sec == 60)
            {
                min++;
                sec = 00;
            }
            if (min == 60)
            {
                hour++;
                min = 00;
            }
            Activity.RunOnUiThread(() => stopWatchText.Text = hour + " : " + min + " : " + sec);
        }

        //Method om de timer te resetten
        private void ResetTimer()
        {
            hour = 00;
            min = 00;
            sec = 00;
        }


        public interface IOnMapControlClick
        {
            void OnRecenterClick();
            bool LocationIsOn();
        }

        private void ToastText(string text)
        {
            Toast.MakeText(Activity, text, ToastLength.Short);
        }
    }
}
