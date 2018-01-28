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

namespace runningapp
{
    public class HistoryFragment : Fragment
    {

        private LinearLayout linearLayout;
        private TextView t;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            linearLayout = new LinearLayout(Activity);
            ListView l = new ListView(Activity);
            l.Adapter = new TrainingAdapter(SharedPrefsSaver.GetTrainingFromPreferences());

            linearLayout.AddView(l);
           
          
            return linearLayout;
        }
    }
}