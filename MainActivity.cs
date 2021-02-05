using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
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
        //Enthernet et = new Enthernet();

        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource//设置界面来源
            SetContentView(Resource.Layout.activity_main);



            TextView view = FindViewById<TextView>(Resource.Id.Receive);
            PlotView plotview = FindViewById<PlotView>(Resource.Id.Chart_View);
            ToggleButton start = FindViewById<ToggleButton>(Resource.Id.Start_Stop);
            ToggleButton connect = FindViewById<ToggleButton>(Resource.Id.connect);


            plotview.Model = CreatePlotModel();
            plotview.SetCursorType(CursorType.ZoomRectangle);

            new Thread(new ThreadStart(() =>
            {
                string data = "";
                bool run = true;
                int state = 0;

                var series = plotview.Model.Series[0] as LineSeries;
                double y = 0.0;
                while (run)
                {
                    switch (state)
                    {
                        case 0://尝试连接服务器
                            try
                            {
                                RunOnUiThread(() => { start.Checked=false; });
                                RunOnUiThread(() => { connect.Checked = false; });

                                server.TCP_Close_Listener();
                                server.TCP_Close_Client();
                                server.TCP_Close_Stream();

                                
                                server.TCP_Connect("192.168.0.124", 11066, 11067);

                                RunOnUiThread(() => { connect.Checked = true; });
                                state = 1;
                            }
                            catch
                            {
                                Thread.Sleep(500);
                            }
                            break;
                        case 1://读取数据
                            try
                            {
                                server.TCP_Write(" ");//发送数据验证客户端是否在线
                                data = server.TCP_Read(1024, 3);
                            }
                            catch
                            {
                                state = 0;//通讯有错误重新侦听客户端
                            }
                            if (data != ""&& start.Checked)
                            {
                                RunOnUiThread(() => { view.Text = data; });//文本显示温度值
                                y = Convert.ToDouble(data);
                                try
                                {
                                    series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), y));
                                    plotview.Model.InvalidatePlot(true);
                                }
                                catch (Exception e)
                                {
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(20);
                }
            })).Start();
        }
        private PlotModel CreatePlotModel()

        {
            
            //创建图表模式

            var plotModel = new PlotModel

            {
                //Title = "体温监测"
                PlotAreaBorderColor= OxyColors.White,
            };

            //添加坐标轴
            plotModel.Axes.Add(new DateTimeAxis { 
                Position = AxisPosition.Bottom ,
                StringFormat = "dd-hh:mm:ss",
                Minimum = DateTimeAxis.ToDouble(DateTime.Now),
                Maximum= DateTimeAxis.ToDouble(DateTime.Now.AddMinutes(1)),

                Title = "时间",
                TitlePosition = 0.5,//整个坐标轴为0-1，0.5为中间
                TitleColor = OxyColors.White,

                TextColor = OxyColors.White,//刻度文本
                AxislineColor= OxyColors.White,//坐标轴线
                TicklineColor= OxyColors.White,//刻度线

                MinorIntervalType = DateTimeIntervalType.Seconds,
                IntervalType = DateTimeIntervalType.Seconds,
                //MajorGridlineStyle = LineStyle.Solid,//主网格
                //MinorGridlineStyle = LineStyle.None,//次网格
            });
            plotModel.Axes.Add(new LinearAxis { 
                Position = AxisPosition.Left, 
                Maximum = 42, 
                Minimum = 0,

                Title = "体温",
                TitlePosition = 0.5,//整个坐标轴为0-1，0.5为中间
                TitleColor = OxyColors.White,

                TextColor = OxyColors.White,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,

                MajorGridlineStyle = LineStyle.Solid,//主网格样式
                MajorGridlineColor = OxyColors.SlateGray,//主网格颜色
            });;;
            ////创建数据列
            var serie = new LineSeries
            {
                Color = OxyColors.White,

                Title = "体温",
                
                MarkerType = MarkerType.Circle,

                MarkerSize = 1,

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