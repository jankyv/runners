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
using static Android.Support.V7.Widget.RecyclerView;

namespace runningapp
{
    public class TrainingAdapter : BaseAdapter<Training>
    {
        List<Training> trainingList;

        public TrainingAdapter(List<Training> users)
        {
            this.trainingList = users;
        }

        public override Training this[int position]
        {
            get
            {
                return trainingList[position];
            }
        }

        public override int Count
        {
            get
            {
                return trainingList.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_training, parent, false);
            }

            TextView name = view.FindViewById<TextView>(Resource.Id.nameText);
            TextView distance = view.FindViewById<TextView>(Resource.Id.distanceText);
            TextView time = view.FindViewById<TextView>(Resource.Id.time);

            Training thisTraining = trainingList[position];
            name.Text = thisTraining.Name;
            distance.Text = thisTraining.GetCurrentDistance().ToString();
            time.Text = thisTraining.timeElapsed.ToString();
            return view;
        }
    }
}