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

namespace runningapp
{
    public class GoogleMapFragment : Fragment, IOnMapReadyCallback, IOnCameraChangeListener
    {
        private MapView mMapView;
        private GoogleMap googleMap;


        private static int ZOOM = 18;
        private DisplayMetrics metrics;
        private Button recenter;
        private ImageButton stopButton;
        private LinearLayout contentLayout;
        private RelativeLayout mapsLayout;

        private ImageButton startButton;
        private OnMapControlClick mListener;
        private PolylineOptions currentTrainingLine;
        private Circle circle;

        private LinearLayout leftLayout;
        private LinearLayout rightLayout;

        private LinearLayout container;

        private bool inTraining;

        private void SetUpVariables()
        {
            inTraining = false;
            bool firstStart = true;
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



            LayoutToStart();



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
                }
                else
                {
                    if (firstStart)
                    {
                        LayoutTraining();
                        mListener.OnStartTrainingClick();
                        inTraining = true;
                        firstStart = false;
                    }
                    else
                    {
                        LayoutTraining();
                        mListener.OnResumeTrainingClick();
                        inTraining = true;
                    }
                   
                }
            };

            stopButton.Click += delegate {
                mListener.OnStartTrainingClick();
                firstStart = true;
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
            currentTrainingLine.InvokeWidth(15);
            currentTrainingLine.InvokeColor(Resource.Color.secondary_color);
        }

        public void AddPolylinePoint(Location location)
        {
            currentTrainingLine.Add(new LatLng(location.Latitude, location.Longitude));
            googleMap.AddPolyline(currentTrainingLine);
            
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
            mListener = (OnMapControlClick)context;        
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

        public void OnCameraChange(CameraPosition position)
        {
            if(circle != null)
            {
                circle.Radius = 100;
                
            }
        }

        public interface OnMapControlClick
        {
            void OnRecenterClick();
            void OnStartTrainingClick();
            void OnPauseTrainingClick();
            void OnStopTrainingClick();
            void OnResumeTrainingClick();
        } 
    }
}
