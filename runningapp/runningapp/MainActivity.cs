using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Content;
using Android.Locations;
using System;
using Android.Runtime;
using Android.Gms.Maps.Model;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Util;
using System.Collections.Generic;

namespace Running
{
    [Activity(Label = "Running", MainLauncher = true, Theme = "@style/Theme.DesignDemo")]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback, ILocationListener
    {
        // Google map variable
        private GoogleMap gMap;

        // Location manager
        protected LocationManager _locationManager = (LocationManager)Application.Context.GetSystemService(LocationService);

        // Location variable
        private Location _location;

        //Training
        Training training;

        // Layout variables
        private Button recenter;
        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        DisplayMetrics metrics;
        private LinearLayout contentLayout;
        private Button startButton;

        private bool recording = false;




        // Static variables
        private static Accuracy ACCURACY = Accuracy.Fine;
        private static Power POWER = Power.Medium;
        private static int ZOOM = 20;
        private static int MAPSIZE = 65;


        // Override OnCreate
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set Content View to layout Main.axml
            SetContentView(Resource.Layout.Main);

            // Set up the Layout variables and handlers
            SetUpVariables();

            // Set up the side menu
            SetUpSideMenu();

            //Set up the Google Map
            SetUpMap();
        }

        // Start location updates again OnResume 
        protected override void OnResume()
        {
            base.OnResume();
            StartLocationUpdates();
        }

        // Stop location updates OnPause to save battery 
        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        //Method to set up the variables and handlers (LAYOUT)
        private void SetUpVariables()
        {
            metrics = Resources.DisplayMetrics;
            recenter = FindViewById<Button>(Resource.Id.zoomToLoc);
            contentLayout = FindViewById<LinearLayout>(Resource.Id.content_layout);
            startButton = FindViewById<Button>(Resource.Id.startTraining);
            training = new Training();

            recenter.Click += delegate
            {
                if (_location != null)
                {
                    ZoomToLocation(_location);
                }
            };

            startButton.Click += delegate
            {
                if (recording == false)
                {
                    startButton.Text = "Pause";
                    Toast.MakeText(this, "Training Started!", ToastLength.Short).Show();

                }
                else
                {
                    startButton.Text = "Start";
                    Toast.MakeText(this, "Training Paused!", ToastLength.Short).Show();
                    training.PauseTraining();


                }
                recording = !recording;



            };




        }

        // Method to sey up the side menu (SIDEMENU)
        private void SetUpSideMenu()
        {
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            // Create ActionBarDrawerToggle button and add it to the toolbar  
            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            var drawerToggle = new Android.Support.V7.App.ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.drawer_open, Resource.String.drawer_close);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            SetupDrawerContent(navigationView); //Calling Function  
        }

        // Method to set up the drawer content (SIDEMENU)
        private void SetupDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (sender, e) =>
            {
                e.MenuItem.SetChecked(true);
                drawerLayout.CloseDrawers();
            };
        }

        // Override OnCreateOptionsMenu to implement side menu (SIDEMENU)
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            navigationView.InflateMenu(Resource.Menu.nav_menu);
            return true;
        }

        // Method to display current location on Google Map (GOOGLEMAP)
        private void DisplayLocation(Location l)
        {
            if (l != null)
            {
                if (gMap != null)
                {
                    MarkerOptions m = new MarkerOptions().SetPosition(new LatLng(l.Latitude, l.Longitude));
                    gMap.AddMarker(m);
                }
            }
        }

        // Method to zoom to current location in Google Map (GOOGLEMAP)
        private void ZoomToLocation(Location l)
        {
            LatLng location = new LatLng(l.Latitude, l.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(location);
            builder.Zoom(ZOOM);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            if (gMap != null)
            {
                gMap.AnimateCamera(cameraUpdate);
            }
        }

        // Method to set up the Google Map (GOOGLEMAP)
        private void SetUpMap()
        {
            if (gMap == null)
            {
                //get the Google Map asynchronously and set it to the fragment with id Map in Main layout
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
                var mapfragment = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map);
                mapfragment.View.LayoutParameters.Height = (int)(metrics.HeightPixels * .8);
                contentLayout.LayoutParameters.Height = (int)(metrics.HeightPixels * 0.2);



            }
        }

        // OnMapReady to implement IOnMapReadyCallback class (GOOGLEMAP)
        public void OnMapReady(GoogleMap googleMap)
        {
            gMap = googleMap;

            //Request Location Updates;
            StartLocationUpdates();
        }

        // Method to start requesting location updates (LOCATION)
        public void StartLocationUpdates()
        {
            Criteria criteriaForGPSService = new Criteria
            {
                Accuracy = ACCURACY,
                PowerRequirement = POWER
            };

            CheckLocationSettings();
            var locationProvider = _locationManager.GetBestProvider(criteriaForGPSService, true);
            Location currentLocation = _locationManager.GetLastKnownLocation(locationProvider);
            _location = currentLocation;
            DisplayLocation(currentLocation);
            _locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
        }

        // Method to check the location settings, if disabled Build Alertdialog to prompt user to enable location, if enabled, do nothing (LOCATION)
        private void CheckLocationSettings()
        {
            if (!_locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                //set alert for executing the task
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Location Settings");
                alert.SetMessage("Location is required");
                alert.SetPositiveButton("Settings", (senderAlert, args) => {
                    this.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings));
                });

                alert.SetNegativeButton("Cancel", (senderAlert, args) => {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();

                });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        // OnLocationChanged to implement ILocationListener Class (LOCATION)
        public void OnLocationChanged(Location location)
        {
            _location = location;
            DisplayLocation(location);
            if (recording == true)
            {
                training.AddPoint(location);
                Toast.MakeText(this, "Locatie: " + location.ToString() + " toegevoegd aan: " + training.CurrentTrack().LocationList.ToString(), ToastLength.Short).Show();

            }

            Console.WriteLine(training.Tracks.ToString());

        }

        // OnProviderDisabled to implement ILocationListener Class (LOCATION)
        public void OnProviderDisabled(string provider)
        {
            if (!_locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                //set alert for executing the task
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Location Settings");
                alert.SetMessage("Location is required");
                alert.SetPositiveButton("Settings", (senderAlert, args) => {
                    this.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings));
                });

                alert.SetNegativeButton("Cancel", (senderAlert, args) => {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();

                });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        // OnProviderEnabled to implement ILocationListener Class (LOCATION)
        public void OnProviderEnabled(string provider)
        {
            StartLocationUpdates();
        }

        //OnStatusChanged to implement ILocationListener Class (LOCATION)
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }
    }
}

