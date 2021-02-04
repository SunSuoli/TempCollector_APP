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
using System;
using System.Threading;
using TempCollector;
namespace TempCollector_APP
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TCP server = new TCP();
        Enthernet et = new Enthernet();

        bool Is_collectining = false;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource//设置界面来源
            SetContentView(Resource.Layout.activity_main);


            server.TCP_Listener_Create(et.GetLocalIp(), 11066);//创建TCP侦听器

            TextView view = FindViewById<TextView>(Resource.Id.Receive);
            PlotView plotview = FindViewById<PlotView>(Resource.Id.Chart_View);
            Button start= FindViewById<Button>(Resource.Id.Start_Stop);
            TextView msg = FindViewById<TextView>(Resource.Id.message);

            start.Click += (sender, e) =>
            {
                if (Is_collectining)
                {
                    server.TCP_Write("stop");
                    Is_collectining = false;
                }
                else
                {
                    server.TCP_Write("start");
                    Is_collectining = true;
                }

            };

            plotview.Model = CreatePlotModel();

            new Thread(new ThreadStart(() =>
            {
                string data = "";
                bool run = true;
                int state = 0;
                int client_number = 0;

                var series = plotview.Model.Series[0] as LineSeries;
                double x = 0;
                double y = 0;
                while (run)
                {
                    switch (state)
                    {
                        case 0://等待客户端接入
                            msg.Text = "等待客户端接入";
                            client_number = server.TCP_Listener_Wait(50);
                            if (client_number >0)
                            {
                                state = 1;
                                msg.Text = "有客户端接入";
                            }
                            break;
                        case 1://读取数据
                            try
                            {
                                server.TCP_Write(".");//发送数据验证客户端是否在线
                                data = server.TCP_Read(2048, 10);
                                if (data != "")
                                {
                                    view.Text = data;//文本显示温度值

                                    y = Convert.ToDouble(data);
                                    series.Points.Add(new DataPoint(x, y));
                                    plotview.Model.InvalidatePlot(true);
                                    x += 1;
                                }
                            }
                            catch
                            {
                                state = 0;//有错误重新侦听客户端
                            }
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(10);
                }
            })).Start();
        }
        private PlotModel CreatePlotModel()

        {

            //创建图表模式

            var plotModel = new PlotModel

            {

                //Title = "体温监测"

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