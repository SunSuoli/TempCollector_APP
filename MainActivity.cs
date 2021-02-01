using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Custom_Communiations;
using System.Timers;
using System;

namespace TempCollector_APP
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        UDP com = new UDP();
        Enthernet et = new Enthernet();

        bool Is_collectining = false;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource//设置界面来源
            SetContentView(Resource.Layout.activity_main);

           
            com.UDP_Open(et.GetLocalIp(), 11066);//打开UDP连接

            Button start= FindViewById<Button>(Resource.Id.Start_Stop);
            start.Click += (sender, e) =>
            {
                if (Is_collectining)
                {
                    com.UDP_Write("stop", "255.255.255.255", 11067);//向端口号11067广播数据
                    Is_collectining = false;
                }
                else
                {
                    com.UDP_Write("start", "255.255.255.255", 11067);//向端口号11067广播数据
                    Is_collectining = true;
                }
                
            };


            TextView view = FindViewById<TextView>(Resource.Id.Receive);
            Timer timer = new Timer(100);
            timer.Elapsed += delegate
            {
                string data = "";
                string ip = "";
                int port;
                RunOnUiThread(() =>
                {
                    com.UDP_Read(out data,out ip,out port);
                    if (data != "")
                    {
                        ////view.Text += DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss") + data + "\n";

                    }
                });
            };
            timer.Enabled = true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}