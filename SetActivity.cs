﻿
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Widget;
using Custom_Files;
using TempCollector_APP;

namespace TempCollector
{
    [Activity(Label = "set")]
    public class SetActivity : Activity
    {
        XML config = new XML();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_set);

            TextInputEditText v_min=FindViewById<TextInputEditText>(Resource.Id.V_min);
            TextInputEditText t_min = FindViewById<TextInputEditText>(Resource.Id.T_min);
            TextInputEditText v_max = FindViewById<TextInputEditText>(Resource.Id.V_max);
            TextInputEditText t_max = FindViewById<TextInputEditText>(Resource.Id.T_max);

            TextInputEditText warn_temp = FindViewById<TextInputEditText>(Resource.Id.Warn_Temp);

            Button ok= FindViewById<Button>(Resource.Id.Ok);
            Button cancel = FindViewById<Button>(Resource.Id.Cancel);

            config.Open();
            v_min.Text = config.Read("Parameters/Calibration/V_min");
            t_min.Text = config.Read("Parameters/Calibration/T_min");
            v_max.Text = config.Read("Parameters/Calibration/V_max");
            t_max.Text = config.Read("Parameters/Calibration/T_max");
            warn_temp.Text= config.Read("Parameters/Warn/Warn_Temp");
            ok.Click += (e, t) =>
            {
                config.Update("Parameters/Calibration/V_min", v_min.Text);
                config.Update("Parameters/Calibration/T_min", t_min.Text);
                config.Update("Parameters/Calibration/V_max", v_max.Text);
                config.Update("Parameters/Calibration/T_max", t_max.Text);
                config.Update("Parameters/Warn/Warn_Temp", warn_temp.Text);

                Finish();
            };
            cancel.Click += (e, t) =>
            {
                Finish();
            };

        }
    }
}