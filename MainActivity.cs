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
using TempCollector;
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

            plotview.Model = CreatePlotModel();

            new Thread(new ThreadStart(() =>
            {
                string data = "";
                bool run = true;

                var series = plotview.Model.Series[0] as LineSeries;
                double x = 0;
                double y = 0;
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
                        view.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss") +": "+ data ;//文本显示温度值

                        y = Convert.ToDouble(data);
                        series.Points.Add(new DataPoint(x, y));
                        plotview.Model.InvalidatePlot(true);
                        x += 1;
                    }
                    Thread.Sleep(100);
                }
            })).Start();
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

            var serie = new LineSeries

            {

                Title = "体温",

                MarkerType = MarkerType.Circle,

                MarkerSize = 2,

                MarkerStroke = OxyColors.White

            };
            plotModel.Series.Add(serie);

            return plotModel;

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}