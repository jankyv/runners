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

            // Handler voor recenter
            recenter.Click += delegate
            {
                mListener.OnRecenterClick();             
            };

            // Handler voor start
            startButton.Click += delegate
            {         
                //Als de training bezig is, pauzeer de training
                if (inTraining)
                {
                    this.PauseTraining();
                }
                //Als de training niet bezig is, hervat of start deze
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

            // Handler voor stop
            stopButton.Click += delegate {
                //Alert
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

        // Layout methodes
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

        // Method voor het starten van de training
        private void StartTraining()
        {
            // Organiseer de layout voor training
            LayoutTraining();

            // Voeg het gedeelte voor de Stopwatch en Afstand toe
            masterLayout.AddView(bottomLayout);

            // Start de timer
            timer.Start();

            // Eerste keer starten is gebeurt
            firstStart = false;

            // Training aanmaken
            training = new Training();

            // Aan het trainen
            inTraining = true;

            // Toast
            ToastText("Training Started");
        }

        // Methode voor het pauzeren van de training
        private void PauseTraining()
        {
            // Organiseer Layout voor pauze
            LayoutPaused();

            // Niet meer aan het trainen
            inTraining = false;

            // Stop de timer
            timer.Stop();

            // Pauzeer de training
            training.Pause();

            //Toast
            ToastText("Training Paused");

        }

        // Methode voor het hervatten van de training.
        private void ResumeTraining()
        {
            // Organiseer de layout voor trainen
            LayoutTraining();

            // Aan het trainen
            inTraining = true;

            // Start de timer
            timer.Start();

            //Toast
            ToastText("Training Resumed");
        }

        private void StopTraining()
        {
            //Organiseer de layout voor Opnieuw Starten
            LayoutToStart();

            // niet meer aan het trainen
            inTraining = false;

            // stop de timer
            timer.Stop();

            //Reset de Timer en de Stopwatch
            ResetTimer();
            stopWatchText.Text = "00:00:00";

            //Clear de Map
            googleMap.Clear();

            // Verberg de stopwatch en afstand
            masterLayout.RemoveView(bottomLayout);

            //Toast
            ToastText("Training Stopped");

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

        // Methode om lijn toe te voegen aan de Google Map
        private void AddLine(LatLng current)
        {
            // Nieuwe Lijn
            PolylineOptions polyLineOptions = new PolylineOptions();
            polyLineOptions.InvokeWidth(20);
            polyLineOptions.InvokeColor(Resource.Color.line_color_1);

            //Lijst van locaties uit de huidige training / track
            List<Location> list = training.CurrentTrack().LocationList;

            //Als er zich in de lijst meer dan 1 locatie bevindt
            if (list.Count > 1)
            {
                // Voeg een lijn toe tussen het 1 na laatste punt en het laatste punt in de lijst
                LatLng prevPoint = new LatLng(list[list.Count - 2].Latitude, list[list.Count - 2].Longitude);
                polyLineOptions.Add(prevPoint);
                polyLineOptions.Add(current);
                googleMap.AddPolyline(polyLineOptions);

                // Verander de tekst van de afstand in de huidige afstand
                SetDistanceText(training.GetCurrentDistance());
            }
        }

        // Methode om naar een locatie op de map te zoomen
        public void ZoomToLocation(Location loc)
        {
            // Maak een camera positie met de locatie
            LatLng location = new LatLng(loc.Latitude, loc.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(location);
            builder.Zoom(ZOOM);
            builder.Bearing(1);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            // Als de googleMap bestaat, animeer naar de locatie.
            if (googleMap != null)
            {
                googleMap.AnimateCamera(cameraUpdate);
            }
        }

        // Methode om de Afstand tekst te veranderen naar de string van een afgeronde float waarde.
        public void SetDistanceText(float d)
        {
            distanceText.Text = Java.Lang.String.ValueOf((int)d) + " meter ";
        }

        // Override OnViewCreated 
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            // Roep SetUpVariables aan
            SetUpVariables();
        }

        // Override OnAttach om de interface aan de Activity te koppelen
        public override void OnAttach(Activity context)
        {
            base.OnAttach(context);
            mListener = (IOnMapControlClick)context;        
        }

        // Override OnDetach
        public override void OnDetach()
        {
            base.OnDetach();
            mListener = null;
        }

        // override onCreateView
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_maps, container, false);

            //instantieer de Google Map
            mMapView = (MapView)rootView.FindViewById(Resource.Id.mapView);
            mMapView.OnCreate(savedInstanceState);

            // Roep OnResume aan op de map om hem zo snel mogelijk te weergeven
            mMapView.OnResume(); 
            try
            {
                MapsInitializer.Initialize(Activity.ApplicationContext);
            }
            catch (Throwable e)
            {
                e.PrintStackTrace();
            }

            // Laad de map asynchroon
            mMapView.GetMapAsync(this);

            // Verberg de locatie button van Google
            View par = ((View)mMapView.FindViewById(1).Parent);
            View LocationButton = par.FindViewById(2);
            LocationButton.LayoutParameters.Height = 0;
            return rootView;
        }  

        // Implement OnMapReady
        public void OnMapReady(GoogleMap mMap)
        {
            googleMap = mMap;

            //Custom style voor de Google Map instellen
            try
            {
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

            //UIcontrols Google Map instellen
            googleMap.MyLocationEnabled = true;
            googleMap.UiSettings.CompassEnabled = true;
            googleMap.SetPadding(0,500,0,0);
         
        }

        // Override OnPause
        public override void OnPause()
        {
            base.OnPause();
            // roep OnPause aan op de MapView
            mMapView.OnPause();
        }

        // Override OnResume
        public override void OnResume()
        {
            base.OnResume();
            // roep OnResume aan op de MapView
            mMapView.OnResume();
        }

        // Override OnDestroy
        public override void OnDestroy()
        {
            base.OnDestroy();
            // roep OnDestroy aan op de MapView
            mMapView.OnDestroy();
        }

        // Override OnLowMemory
        public override void OnLowMemory()
        {
            base.OnLowMemory();
            // roep OnLowMemory aan op de MapView
            mMapView.OnLowMemory();
        }

        // Methode om een volledige training te weergeven (straks voor de analyseer opgaven in practicum 3)
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

        // interface om met de activity te communiceren
        public interface IOnMapControlClick
        {
            void OnRecenterClick();
            bool LocationIsOn();
            void CustomToast(string text);
        }

        // Methode om wat te toasten
        private void ToastText(string text)
        {
            mListener.CustomToast(text);
        }
    }
}
