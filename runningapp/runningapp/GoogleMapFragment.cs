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

namespace runningapp
{
    public class GoogleMapFragment : Fragment, IOnMapReadyCallback, IOnCameraChangeListener
    {
        private MapView mMapView;
        private GoogleMap googleMap;


        private static int ZOOM = 18;
        private DisplayMetrics metrics;
        private Button recenter;
        private LinearLayout contentLayout;
        private RelativeLayout mapsLayout;

        private Button startButton;
        private OnMapControlClick mListener;
        LatLng temp_location;
        private Circle circle;


        
        private void SetUpVariables()
        {
           
            metrics = Resources.DisplayMetrics;
            contentLayout = Activity.FindViewById<LinearLayout>(Resource.Id.content_layout);
            mapsLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.maps_layout);
            
            recenter = Activity.FindViewById<Button>(Resource.Id.zoomToLoc);

            startButton = Activity.FindViewById<Button>(Resource.Id.startTraining);
            contentLayout.LayoutParameters.Height = ViewGroup.LayoutParams.WrapContent;
            recenter.LayoutParameters.Width = (int)(0.5 * metrics.WidthPixels);
            startButton.LayoutParameters.Width = (int)(0.5 * metrics.WidthPixels);
            mapsLayout.LayoutParameters.Height = (int)(metrics.HeightPixels - contentLayout.LayoutParameters.Height);



            recenter.Click += delegate
            {
                mListener.OnRecenterClick();
            };
            

            startButton.Click += delegate
            {        
                mListener.OnStartTrainingClick();
            };

        }

        public void ChangeStartButtonText()
        {
            if (startButton.Text == "Start")
            {
                startButton.Text = "Stop";
            }
            else
            {
                startButton.Text = "Start";
            }
        }

        public void DisplayTraining(Training training)
        {
            List<PolylineOptions> list = training.GetTrainingPolylines();

            foreach(PolylineOptions l in list)
            {
                googleMap.AddPolyline(l);
            }
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
        } 
    }
}
