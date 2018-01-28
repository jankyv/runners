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
using runningapp;

namespace runningapp
{
    //static class om Trainingen op te slaan
    class SharedPrefsSaver
    {
        // Methode om de lijst van trainingen op te halen
        public static List<Training> GetTrainingFromPreferences()
        {
            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences("training1", FileCreationMode.Private);

            // read exisiting value
            var trainings = pref.GetString("Training",null);

            // if preferences return null, initialize list
            if (trainings == null)
            {
                Log.Info("prefs","preferences returned null");
                return new List<Training>();
            }

            var trainingList = JsonConvert.DeserializeObject<List<Training>>(trainings);

            if (trainingList == null)
            {
                Log.Info("prefs", "list returned null");
                return new List<Training>();
            }

            Log.Info("prefs", "Training data found");
            Log.Info("prefs", trainingList.ToString());
            return trainingList;
        }
            
        // Methode om 1 training op te slaan
        public static void SaveTraining(Training t)
        {
            List<Training> trainingsList = GetTrainingFromPreferences();
            trainingsList.Add(t);
            SaveTrainingListToPreferences(trainingsList);      
        }

        // Method om de lijst van trainingen in het geheugen op te slaan
        private static void SaveTrainingListToPreferences(List<Training> list)
        {
            // get shared preferences
            ISharedPreferences pref = Application.Context.GetSharedPreferences("training1", FileCreationMode.Private);

            // read exisiting value
            var trainings = pref.Edit();
            string listJson = JsonConvert.SerializeObject(list);
            trainings.PutString("Training", listJson);
            Log.Info("prefs", "training saved");
            trainings.Commit();
        }
    }
}