using System;
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
        private MapView mMapView;
        private GoogleMap googleMap;


        private static int ZOOM = 15;
        private DisplayMetrics metrics;
        private Button recenter;
        private ImageButton stopButton;
        private LinearLayout contentLayout;
        private RelativeLayout mapsLayout;

        private ImageButton startButton;
        private IOnMapControlClick mListener;
        private PolylineOptions currentTrainingLine;
        private Circle circle;

        private LinearLayout leftLayout;
        private LinearLayout rightLayout;

        private LinearLayout container;
        private TextView stopWatchText;
        private LinearLayout bottomLayout;
        private RelativeLayout masterLayout;
        private TextView distanceText;
        private Timer timer;
        int hour, min, sec;


        private bool inTraining;
        public bool firstStart;

        private void SetUpVariables()
        {
            ResetTimer();
            inTraining = false;
            firstStart = true;
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

            

            LayoutToStart();


            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            

            recenter.Click += delegate
            {
                mListener.OnRecenterClick();
            };

            startButton.Click += delegate
            {
              
                if (inTraining)
                {
                    LayoutPaused();
                    mListener.OnPauseTrainingClick();
                    inTraining = false;
                    timer.Stop();
                }
                else
                {
                    if (firstStart)
                    {
                        LayoutTraining();
                        mListener.OnStartTrainingClick();
                        inTraining = true;
                        masterLayout.AddView(bottomLayout);
                        timer.Start();

                    }
                    else
                    {
                        LayoutTraining();
                        mListener.OnResumeTrainingClick();
                        inTraining = true;
                        timer.Start();
                    }
                   
                }
            };

            stopButton.Click += delegate {
                mListener.OnStopTrainingClick();
            };


        }

        private void ResetTimer()
        {
            hour = 0;
            min = 0;
            sec = 0;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            sec++;
            if (sec == 60)
            {
                min++;
                sec = 0;
            }
            if (min == 60)
            {
                hour++;
                min = 0;
            }
            Activity.RunOnUiThread(() => stopWatchText.Text = hour + " : " + min + " : " + sec);
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



        public void DisplayTraining(Training training)
        {
            List<PolylineOptions> list = training.GetTrainingPolylines();

            foreach(PolylineOptions l in list)
            {
                googleMap.AddPolyline(l);
            }
        }

        public void StartViewTraining()
        {
            currentTrainingLine = new PolylineOptions();
            currentTrainingLine.InvokeWidth(20);
            currentTrainingLine.InvokeColor(Resource.Color.secondary_color);
        }

        public void AddPolylinePoint(Location location)
        {
            currentTrainingLine.Add(new LatLng(location.Latitude, location.Longitude));
            googleMap.AddPolyline(currentTrainingLine);
            
        }

        public void SetDistanceText(float d)
        {
            distanceText.Text = Java.Lang.String.ValueOf(d);
        }

       
        
        public void ZoomToLocation(Location loc)
        {
            LatLng location = new LatLng(loc.Latitude, loc.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(location);
            builder.Zoom(ZOOM);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            if (googleMap != null)
            {
                googleMap.AnimateCamera(cameraUpdate);
            }
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
           // googleMap.SetOnCameraChangeListener(this);
         
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

     

        public interface IOnMapControlClick
        {
            void OnRecenterClick();
            void OnStartTrainingClick();
            void OnPauseTrainingClick();
            void OnStopTrainingClick();
            void OnResumeTrainingClick();
        } 
    }
}
