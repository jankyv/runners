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

namespace runningapp
{
    [Activity(Label = "runningapp", MainLauncher = true, Theme = "@style/Theme.DesignDemo")]
    public class MainActivity : AppCompatActivity, ILocationListener, GoogleMapFragment.OnMapControlClick
    {
        // Location manager
        protected LocationManager _locationManager = (LocationManager)Application.Context.GetSystemService(LocationService);

        // Location variable
        private Location _location;

        //Training
        Training training;

      

        private bool recording = false;




        // Static variables
        private static Accuracy ACCURACY = Accuracy.Fine;
        private static Power POWER = Power.Medium;
        private static int ZOOM = 20;
        private static int MAPSIZE = 65;
        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private GoogleMapFragment mapFragment;


        // Override OnCreate
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set Content View to layout Main.axml
            SetContentView(Resource.Layout.Main);

          

            // Set up the side menu
            SetUpSideMenu();

            mapFragment = new GoogleMapFragment();
            ShowFragment(mapFragment);

            StartLocationUpdates();

            
        }

        // Start location updates again OnResume 
        protected override void OnResume()
        {
            base.OnResume();
            StartLocationUpdates();
        }

        // Method to show Fragment
        private void ShowFragment(Android.App.Fragment fragment)
        {
            Android.App.FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, fragment)                 
                    .AddToBackStack(null)
                    .Commit();
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
            //StartLocationUpdates();
        }

        //OnStatusChanged to implement ILocationListener Class (LOCATION)
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }

        public void OnRecenterClick()
        {          
            Toast.MakeText(this, "Recenter Clicked", ToastLength.Short).Show();
            //mapFragment.DisplayLocation(ToLatLng(_location));
            mapFragment.ZoomToLocation(ToLatLng(_location));
        }

        public void OnStartTrainingClick()
        {
            Toast.MakeText(this, "Start Clicked", ToastLength.Short);
        }

        private LatLng ToLatLng(Location l)
        {
            return new LatLng(l.Latitude, l.Longitude);
        }
    }
}

