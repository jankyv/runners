using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Gms.Location;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Util;
using System;
using Android.Content;
using Android.Support.V7.App;
using Android.Gms.Maps.Model;
using System.Collections.Generic;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using System.Collections.Generic;
using static Android.Widget.AdapterView;

namespace runningapp
{
    [Activity(Label = "runningapp", MainLauncher = true, Theme = "@style/Theme.MyTheme")]
    public class MainActivity : ActionBarActivity, 
                                    GoogleMapFragment.IOnMapControlClick, 
                                    GoogleApiClient.IConnectionCallbacks,
                                    GoogleApiClient.IOnConnectionFailedListener, 
                                    Android.Gms.Location.ILocationListener

    {
        
        // Variabele voor de huidige locatie
        private Location _location;

        private GoogleMapFragment mapFragment;
        private HistoryFragment historyFragment;

        // Google location api variabelen.
        GoogleApiClient apiClient;
        LocationRequest locRequest;

        // bool variabele om te controleren of google play services zijn geinstalleerd.
        bool _isGooglePlayServicesInstalled;

        private SupportToolbar mToolbar;
        private MyActionBarDrawerToggle mDrawerToggle;
        private DrawerLayout mDrawerLayout;
        private ListView mLeftDrawer;
        private ArrayAdapter mLeftAdapter;
        private List<string> mLeftDataSet;

        // override OnCreate.
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Log.Debug("OnCreate", "OnCreate aangeroepen");


            mapFragment = new GoogleMapFragment();
            historyFragment = new HistoryFragment();

            SetContentView(Resource.Layout.Main);

            SetUpNavigation(savedInstanceState);
            
           



            /* Weergeven van GoogleMapFragment d.m.v. methode */ /// <see cref="ShowFragment(Android.App.Fragment)"/>
            ShowFragment(mapFragment);

            /* Controleer of de Google api's aanwezig zijn op het apparaat, zo niet, installeer ze */ /// <see cref="CheckAndInstallGoogleApi()"/>
            CheckAndInstallGoogleApi();
        }


        // Method om de navigatie UI op te zetten
        private void SetUpNavigation(Bundle bundle)
        {
            mToolbar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            mLeftDrawer = FindViewById<ListView>(Resource.Id.left_drawer);

            mLeftDrawer.Tag = 0;

            SetSupportActionBar(mToolbar);

            mLeftDataSet = new List<string>();
            mLeftDataSet.Add("Run");
            mLeftDataSet.Add("History");
            mLeftAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mLeftDataSet);
            
            mLeftDrawer.Adapter = mLeftAdapter;

            mLeftDrawer.ItemClick += (sender, e) => 
            {
                Log.Info("itemclick", e.ToString());
                Log.Info("itemclick", sender.ToString());

                switch (e.Position)
                {
                    case 0:
                        ShowFragment(mapFragment);
                        mDrawerLayout.CloseDrawers();

                        break;
                    case 1:
                        ShowFragment(historyFragment);
                        mDrawerLayout.CloseDrawers();
                        break;
                }
            };


            mDrawerToggle = new MyActionBarDrawerToggle(
                this,                           //Host Activity
                mDrawerLayout,                  //DrawerLayout
                Resource.String.openDrawer,     //Opened Message
                Resource.String.closeDrawer     //Closed Message
            );

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            mDrawerLayout.SetDrawerListener(mDrawerToggle);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            mDrawerToggle.SyncState();

            if (bundle != null)
            {
                if (bundle.GetString("DrawerState") == "Opened")
                {
                    SupportActionBar.SetTitle(Resource.String.openDrawer);
                }

                else
                {
                    SupportActionBar.SetTitle(Resource.String.closeDrawer);
                }
            }

            else
            {
                //This is the first the time the activity is ran
                SupportActionBar.SetTitle(Resource.String.closeDrawer);
            }

        }

       
    public override bool OnOptionsItemSelected(IMenuItem item)
        {
            mDrawerToggle.OnOptionsItemSelected(item);
            return base.OnOptionsItemSelected(item);
        }




        // Override onresume.
        protected override void OnResume()
        {
            base.OnResume();
            Log.Debug("OnResume", "OnResume aangeroepen, verbinden met locationApi client");

            // Verbind de apiClient
            apiClient.Connect();
        }

        // Methode om de laatst bekende locatie van het apparaat te verkrijgen
        private void GetLastLocation()
        {      
                // als de apiClient is verbonden
                if (apiClient.IsConnected)
                {
                    Location location = LocationServices.FusedLocationApi.GetLastLocation(apiClient);
                    if (location != null)
                    {
                        /* Update locatie op de google map */ /// <see cref="UpdateLocationOnMap(Location)"/>
                        UpdateLocation(location);
                        mapFragment.ZoomToLocation(location);
                }
            }
                else
                {
                    Log.Info("LocationClient", "Client is niet verbonden");
                }
        }

        private bool LocationEnabled()
        {
            LocationManager lm = (LocationManager)this.GetSystemService(LocationService);
            bool gps_enabled = false;

            try
            {
                gps_enabled = lm.IsProviderEnabled(LocationManager.GpsProvider);
            }
            catch (Exception ex) { }
            return gps_enabled;
        }

        private void CheckLocationSettings()
        {
            if (!LocationEnabled())
            {
                // notify user
                Android.Support.V7.App.AlertDialog.Builder dialog = new Android.Support.V7.App.AlertDialog.Builder(this);
                dialog.SetMessage("Location is not enabled");
                dialog.SetPositiveButton("Location Settings", delegate
                {
                    Intent myIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                    this.StartActivity(myIntent);
                });

                dialog.SetNegativeButton("Cancel", delegate {

                });
      
                 dialog.Show();      
            }
        }

        private void UpdateLocation(Location location)
        {
            Log.Debug("LocationClient", "Locatie veranderd");
            _location = location;
            mapFragment.AddLocation(location);
        }

        //  Methode om fragments te weergeven
        private void ShowFragment(Android.App.Fragment fragment)
        {
            Android.App.FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, fragment)
                    .AddToBackStack(null)
                    .Commit();
        }

        // Asynchrone methode om locatie updates aan te vragen
        private async void RequestLocationUpdates() {

            CheckLocationSettings();
            // als de apiClient is verbonden
            if (apiClient.IsConnected)
            {
                // Prioriteit naar 100 zetten (Hoog)
                locRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
                
                // Interval en snelste interval instellen in milliseconden
                locRequest.SetFastestInterval(500);
                locRequest.SetInterval(1000);

                Log.Debug("LocationRequest", "Request prioriteit ingesteld op {0}, interval ingesteld op {1} ms",
                    locRequest.Priority.ToString(), locRequest.Interval.ToString());

                // "await" locatie updates --> OnLocationChanged wordt aangeroepen zodra de locatie veranderd is
                await LocationServices.FusedLocationApi.RequestLocationUpdates(apiClient, locRequest, this);
            }
            else
            {
                Log.Info("LocationClient", "Please wait for Client to connect");
            }
        }

        // Methode om te ontroleren of de Google api's aanwezig zijn op het apparaat, zo niet, installeer ze
        private void CheckAndInstallGoogleApi()
        {
            _isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();

            if (_isGooglePlayServicesInstalled)
            {
                // pass in the Context, ConnectionListener and ConnectionFailedListener
                apiClient = new GoogleApiClient.Builder(this, this, this)
                    .AddApi(LocationServices.API).Build();

                // generate a location request that we will pass into a call for location updates
                locRequest = new LocationRequest();
                Log.Info("MainActivity", "apiClient created");
            }
            else
            {
                Log.Error("OnCreate", "Google Play Services is not installed");
                Toast.MakeText(this, "Google Play Services is not installed", ToastLength.Long).Show();
                Finish();
            }
        }

        // Metohde om te controleren of de Google Api's op het apparaat zijn geinstalleerd
        bool IsGooglePlayServicesInstalled()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("ManActivity", "There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);

                // Show error dialog to let user debug google play services
            }
            return false;
        }

      

      

        /* Interface */ /// <see cref="GoogleMapFragment.OnMapControlClick"/>
        public void OnRecenterClick()
        {     
            CheckLocationSettings();
            if (_location != null)
            {
                mapFragment.ZoomToLocation(_location);
            }
            else
            {         
                Toast.MakeText(this, "Location not available", ToastLength.Short).Show();
            }
        }


        public bool LocationIsOn()
        {
            bool ret = LocationEnabled();
            CheckLocationSettings();
            return ret;
        }

        /* Interface */ /// <see cref="GoogleApiClient.IConnectionCallbacks"/>
        public void OnConnected(Bundle bundle)
        {
            /* Vraag de laats bekende locatie op */ /// <see cref="GetLastLocation()"/>
            GetLastLocation();

            /* Vraag locatie updates aan */ /// <see cref="RequestLocationUpdates()"/>
            RequestLocationUpdates();
            Log.Info("LocationClient", "Verbonden met GoogleApi client");
        }

        /* Interface */ /// <see cref="GoogleApiClient.IConnectionCallbacks"/>
        public void OnDisconnected()
        {        
            Log.Info("LocationClient", "Verbinding met GoogleApi client verbroken");
        }

        /* Interface */ /// <see cref="GoogleApiClient.IOnConnectionFailedListener"/>
        public void OnConnectionFailed(ConnectionResult bundle)
        {     
            Log.Info("LocationClient", "Verbinden met GoogleApi clien mislukt");
        }

        /* Interface */ /// <see cref="Android.Gms.Location.ILocationListener"/>
        public void OnLocationChanged(Location location)
        {
            UpdateLocation(location);
        }

        /* Interface */ /// <see cref="GoogleApiClient.IConnectionCallbacks"/>
        public void OnConnectionSuspended(int i)
        {

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if (mDrawerLayout.IsDrawerOpen((int)GravityFlags.Left))
            {
                outState.PutString("DrawerState", "Opened");
            }

            else
            {
                outState.PutString("DrawerState", "Closed");
            }

            base.OnSaveInstanceState(outState);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            mDrawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            mDrawerToggle.OnConfigurationChanged(newConfig);
        }


    }
}

