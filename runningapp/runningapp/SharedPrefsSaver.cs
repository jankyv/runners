using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System.IO;
using Android.Util;

namespace runningapp
{
    class SharedPrefsSaver
    {
        public static List<Training> GetTrainingFromPreferences()
        {
            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences("training", FileCreationMode.Private);

            // read exisiting value
            var trainings = pref.GetString("Training",null);

            // if preferences return null, initialize listOfCustomers
            if (trainings == null)
            {
                Log.Info("prefs","preferences returned null");
                return null;

            }

            var listOfCustomers = JsonConvert.DeserializeObject<List<Training>>(trainings);

            if (listOfCustomers == null)
            {
                Log.Info("prefs", "list returned null");

                return null;
            }

            Log.Info("prefs", "Training data found");
            Log.Info("prefs", listOfCustomers.ToString());
            return listOfCustomers;
        }
            
        public static void SaveTraining(Training t)
        {
            List<Training> trainingsList = GetTrainingFromPreferences();
            if(trainingsList == null)
            {
                Log.Info("prefs", "no training data");
                trainingsList = new List<Training>();
                trainingsList.Add(t);
                SaveTrainingListToPreferences(trainingsList);
            }
            else
            {
                Log.Info("prefs", "trainingslist found");
                trainingsList.Add(t);
                SaveTrainingListToPreferences(trainingsList);
            }
        }

        private static void SaveTrainingListToPreferences(List<Training> list)
        {
            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences("training", FileCreationMode.Private);

            // read exisiting value
            var trainings = pref.Edit();
            string listJson = JsonConvert.SerializeObject(list);
            trainings.PutString("Training", listJson);
            Log.Info("prefs", "training saved");
            trainings.Commit();
        }
    }
}