using System;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Custom_Communiations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;

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

            TextView view = FindViewById<TextView>(Resource.Id.Receive);
            PlotView plotview = FindViewById<PlotView>(Resource.Id.Chart_View);
            Button start= FindViewById<Button>(Resource.Id.Start_Stop);
            Button quiet = FindViewById<Button>(Resource.Id.Quiet);

            start.Click += (sender, e) =>
            {
                //发送以后就无法接收到数据，UDP发送有BUG
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

            quiet.Click += (sender, e) =>
            {
                com.Udp_Close();
            };

            plotview.Model = CreatePlotModel();

            new Thread(new ThreadStart(() =>
            {
                string data = "";
                bool run = true;

                //var series1 = new LineSeries

                //{

                //    Title = "体温",

                //    MarkerType = MarkerType.Circle,

                //    MarkerSize = 2,

                //    MarkerStroke = OxyColors.White

                //};
                //double x = 0;
                //double y = 0;
                while (run)
                {
                    try
                    {
                        data = com.UDP_Read();
                    }
                    catch(Exception e)
                    {
                        view.Text = e.Message;
                        run = false;
                    }
                    if (data != "")
                    {
                        view.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss") + data ;//文本显示温度值

                        //y = Convert.ToDouble(data);
                        //series1.Points.Add(new DataPoint(x, y));
                        //plotview.Model.Series.Add(series1);
                        //x += 1;
                    }
                    Thread.Sleep(100);
                }
            })).Start();

            //------------------------------------------------------------------------------
            //System.Timers.Timer timer = new System.Timers.Timer(100);
            //timer.Elapsed += delegate
            //{
            //    string data = "";
            //    string ip = "";
            //    int port;
            //    RunOnUiThread(() =>
            //    {
            //        com.UDP_Read(out data,out ip,out port);
            //        if (data != "")
            //        {
            //            view.Text += DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss") + data + "\n";

            //        }
            //    });
            //};
            //timer.Enabled = true;
            //------------------------------------------------------------------------------
        }
        private PlotModel CreatePlotModel()

        {

            //创建图表模式

            var plotModel = new PlotModel

            {

                Title = "体温监测"

            };

            //添加坐标轴

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 42, Minimum = 32 });

            ////创建数据列

            //var series1 = new LineSeries

            //{

            //    Title = "体温",

            //    MarkerType = MarkerType.Circle,

            //    MarkerSize = 2,

            //    MarkerStroke = OxyColors.White

            //};

            ////添加数据点

            //series1.Points.Add(new DataPoint(0, 36.5));

            //series1.Points.Add(new DataPoint(1, 37.5));

            //series1.Points.Add(new DataPoint(2, 38.5));

            //series1.Points.Add(new DataPoint(3, 39.5));

            //series1.Points.Add(new DataPoint(4, 39.8));

            //series1.Points.Add(new DataPoint(5, 38.5));

            //series1.Points.Add(new DataPoint(6, 37.5));

            ////添加数据列

            //plotModel.Series.Add(series1);

            return plotModel;

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}