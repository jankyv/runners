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

namespace runningapp
{
    public class GoogleMapFragment : Fragment, IOnMapReadyCallback
    {
        private MapView mMapView;
        private GoogleMap googleMap;


        private static int ZOOM = 20;
        private DisplayMetrics metrics;
        private Button recenter;
        private LinearLayout contentLayout;
        private RelativeLayout mapsLayout;

        private Button startButton;
        private OnMapControlClick mListener;



        //Method to set up the variables and handlers (LAYOUT)
        private void SetUpVariables()
        {
           
            metrics = Resources.DisplayMetrics;
            contentLayout = Activity.FindViewById<LinearLayout>(Resource.Id.content_layout);
            mapsLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.maps_layout);
            mapsLayout.LayoutParameters.Height = (int)(0.9 * metrics.HeightPixels);
            recenter = Activity.FindViewById<Button>(Resource.Id.zoomToLoc);

            startButton = Activity.FindViewById<Button>(Resource.Id.startTraining);
            contentLayout.LayoutParameters.Height = (int)(0.1 * metrics.HeightPixels);
            recenter.LayoutParameters.Width = (int)(0.5 * metrics.WidthPixels);
            startButton.LayoutParameters.Width = (int)(0.5 * metrics.WidthPixels);



            recenter.Click += delegate
            {
                Console.WriteLine("delegate is called");    
                mListener.OnRecenterClick();
            };
            

            startButton.Click += delegate
            {
                mListener.OnStartTrainingClick();

            };
        }

        // Method to zoom to current location in Google Map (GOOGLEMAP)
        public void ZoomToLocation(LatLng location)
        {
            
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

            mMapView.OnResume(); // needed to get the map to display immediately

            try
            {
                MapsInitializer.Initialize(Activity.ApplicationContext);
            }
            catch (Throwable e)
            {
                e.PrintStackTrace();
            }

            mMapView.GetMapAsync(this);
            return rootView;
        }


        

        public void OnMapReady(GoogleMap mMap)
        {
            googleMap = mMap;

            // For showing a move to my location button
            googleMap.MyLocationEnabled = false;

           
         
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



        public interface OnMapControlClick
        {
            void OnRecenterClick();
            void OnStartTrainingClick();
        }

        
    }
}
