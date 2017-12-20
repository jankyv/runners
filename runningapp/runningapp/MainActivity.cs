using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Gms.Location;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Util;
using Android.Gms.Maps.Model;

namespace runningapp
{
    [Activity(Label = "runningapp", MainLauncher = true, Theme = "@style/Theme.DesignDemo")]
    public class MainActivity : AppCompatActivity, 
                                    GoogleMapFragment.OnMapControlClick, 
                                    GoogleApiClient.IConnectionCallbacks,
                                    GoogleApiClient.IOnConnectionFailedListener, 
                                    Android.Gms.Location.ILocationListener

    {
        private Location _location;

        // Layout variabelen.
        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private GoogleMapFragment mapFragment;

        // Google location api variabelen.
        GoogleApiClient apiClient;
        LocationRequest locRequest;

        //bool variabele om te controleren of google play services zijn geinstalleerd.
        bool _isGooglePlayServicesInstalled;

        bool recordingTraining;

        private Training training;

        // Override oncreate.
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Log.Debug("OnCreate", "OnCreate aangeroepen");
            recordingTraining = false;

            // Set content view to layout Main.axml.
            SetContentView(Resource.Layout.Main);

            /* Opzetten van side menu. */ /// <see cref="SetUpSideMenu()"/>
            SetUpSideMenu();

            /* Opzetten en weergeven van GoogleMapFragment */ /// <see cref="GoogleMapFragment.cs"/> 
            mapFragment = new GoogleMapFragment();

            /* Weergeven van GoogleMapFragment d.m.v. methode */ /// <see cref="ShowFragment(Android.App.Fragment)"/>
            ShowFragment(mapFragment);

            /* Controleer of de Google api's aanwezig zijn op het apparaat, zo niet, installeer ze */ /// <see cref="CheckAndInstallGoogleApi()"/>
            CheckAndInstallGoogleApi();
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
    
        // Asynchrone methode om locatie updates aan te vragen
        private async void RequestLocationUpdates() {
            // als de apiClient is verbonden
            if (apiClient.IsConnected)
            {

                // Prioriteit naar 100 zetten (Hoog)
                locRequest.SetPriority(100);

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

        // Methode om de locatie op de map te updaten
        private void UpdateLocation(Location location)
        {
            mapFragment.DisplayLocation(location);
            _location = location;

            if(recordingTraining == true)
            {
                training.AddPoint(location);
            }
           
            if(training != null)
            {
                mapFragment.DisplayTraining(training);
            }
        }

        //  Methode om fragments te weergeven
        private void ShowFragment(Android.App.Fragment fragment)
        {
            Android.App.FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, fragment)                 
                    .AddToBackStack(null)
                    .Commit();
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

        // Methode om het menu op te zetten
        private void SetUpSideMenu()
        {
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            var drawerToggle = new Android.Support.V7.App.ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.drawer_open, Resource.String.drawer_close);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            SetupDrawerContent(navigationView); //Calling Function  
        }

        // Metgode om de navigatie op te zetten
        private void SetupDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (sender, e) =>
            {
                e.MenuItem.SetChecked(true);
                drawerLayout.CloseDrawers();
            };
        }

        // Override OnCreateOptionsMenu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            navigationView.InflateMenu(Resource.Menu.nav_menu);
            return true;
        }

        /* Interface */ /// <see cref="GoogleMapFragment.OnMapControlClick"/>
        public void OnRecenterClick()
        {
            Toast.MakeText(this, "Recenter Clicked", ToastLength.Short).Show();

            if (_location != null)
            {
                mapFragment.ZoomToLocation(_location);
            }
            else
            {
                Toast.MakeText(this, "Location not available", ToastLength.Short).Show();
            }
        }

        /* Interface */ /// <see cref="GoogleMapFragment.OnMapControlClick"/>
        public void OnStartTrainingClick()
        {
            if (recordingTraining)
            {
                training.Pause();
                recordingTraining = false;
            }
            else
            {
                if(training != null)
                {
                    training = new Training();
                    recordingTraining = true;
                }
                else
                {
                    recordingTraining = true;
                }
            }
            Toast.MakeText(this, "Start Clicked", ToastLength.Short).Show();
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
            Log.Debug("LocationClient", "Locatie veranderd");

            UpdateLocation(location);
        }

        /* Interface */ /// <see cref="GoogleApiClient.IConnectionCallbacks"/>
        public void OnConnectionSuspended(int i)
        {

        }
    }
}

