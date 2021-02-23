using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Android.Net;
using Custom_Communiations;
using Custom_Files;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using System;
using System.Threading;
using TempCollector;
using Xamarin.Essentials;
using Resource = TempCollector.Resource;
using Android.Support.V4.App;
using Android.Graphics;
using TempCollector.Classes;

namespace TempCollector_APP
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        readonly TCP client = new TCP();
        readonly UDP udp = new UDP();
        readonly Enthernet et = new Enthernet();

        readonly XML config = new XML();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource//设置界面来源
            SetContentView(Resource.Layout.activity_main);

            //打开UDP连接
            udp.UDP_Open(et.GetLocalIp(),11067);

            //读取设置参数
            config.Open();
            GlobalData.v_min = Convert.ToDouble(config.Read("Parameters/Calibration/V_min"));
            GlobalData.t_min = Convert.ToDouble(config.Read("Parameters/Calibration/T_min"));
            GlobalData.v_max = Convert.ToDouble(config.Read("Parameters/Calibration/V_max"));
            GlobalData.t_max = Convert.ToDouble(config.Read("Parameters/Calibration/T_max"));
            GlobalData.warn_temp = Convert.ToDouble(config.Read("Parameters/Warn/Warn_Temp"));

            //初始化铃声
            Android.Net.Uri notification = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);//使用闹钟声音
            Ringtone r = RingtoneManager.GetRingtone(this, notification);

            //关联控件
            TextView view = FindViewById<TextView>(Resource.Id.Receive);
            PlotView plotview = FindViewById<PlotView>(Resource.Id.Chart_View);
            ToggleButton start = FindViewById<ToggleButton>(Resource.Id.Start_Stop);
            ToggleButton connect = FindViewById<ToggleButton>(Resource.Id.connect);
            Button set= FindViewById<Button>(Resource.Id.set);
            TextView msg = FindViewById<TextView>(Resource.Id.msg);
            Button sleep = FindViewById<Button>(Resource.Id.sleep);

            //设置
            set.Click += (e, t) =>
             {

                 var intent = new Intent(this, typeof(SetActivity));
                 //设置意图传递的参数
                 //intent.PutStringArrayListExtra("phone_numbers", phoneNumbers);
                 StartActivity(intent);
             };
            //休眠
            sleep.Click += (e, t) =>
             {
                 try
                 {
                     client.TCP_Write("slp");
                 }
                 catch
                 {

                 }
             };
            
            plotview.Model = CreatePlotModel();
            plotview.SetCursorType(CursorType.ZoomRectangle);

            new Thread(new ThreadStart(() =>
            {
                string data = "";
                bool run = true;
                int state = 0;
                
                DateTime time_start = DateTime.Now;
                var series = plotview.Model.Series[0] as LineSeries;
                double y = 0.0;
                string ESP_IP = "";//ESP-32的IP
                while (run)
                {
                    switch (state)
                    {
                        case 0://获取服务器IP

                            udp.UDP_Write("app","255.255.255.255", 11068);//向ESP广播数据
                            udp.UDP_Read(out ESP_IP);
                            if (ESP_IP != "")
                            {
                                RunOnUiThread(() => { msg.Text = "发现设备：" + ESP_IP; });
                                state = 1;
                            }
                            break;
                        case 1://尝试连接服务器
                            try
                            {
                                //RunOnUiThread(() => { start.Checked=false; });//取消自动停止采集，防止断线重连无法报警
                                RunOnUiThread(() => { connect.Checked = false; });

                                //client.TCP_Close_Client();
                                //client.TCP_Close_Stream();


                                client.TCP_Connect(ESP_IP, 11066, 11060);

                                RunOnUiThread(() => { msg.Text = "连接设备：" + ESP_IP; });
                                RunOnUiThread(() => { connect.Checked = true; });
                                state = 2;
                            }
                            catch
                            {
                                Thread.Sleep(500);
                            }
                            break;
                        case 2://读取数据
                            try
                            {
                                TimeSpan ts = DateTime.Now - time_start;
                                if(ts.TotalSeconds >= 3)//每隔3秒查看一次
                                {
                                    client.TCP_Write("app");//发送数据验证服务器是否在线
                                    time_start = DateTime.Now;
                                }
                            }
                            catch
                            {
                                RunOnUiThread(() => { msg.Text ="与服务器断开"; });
                                state = 0;//通讯有错误重新连接服务器
                            }
                            try
                            {
                                data = client.TCP_Read(4, 10);//10ms内接收4字节，字节内容为“XX.X”
                                if (data != "" && start.Checked)
                                {
                                    y = Calibration(Convert.ToDouble(data), GlobalData.v_min, GlobalData.t_min, GlobalData.v_max, GlobalData.t_max);
                                    RunOnUiThread(() => { view.Text =y.ToString("f1") ; });//文本显示温度值，一位小数点
                                    try
                                    {
                                        series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), y));
                                        plotview.Model.InvalidatePlot(true);
                                    }
                                    catch
                                    {

                                    }
                                    if (y >= GlobalData.warn_temp)
                                    {
                                        try
                                        {
                                            var duration = TimeSpan.FromSeconds(0.5);
                                            Vibration.Vibrate(duration);//打开震动
                                            r.Play();//打开铃声
                                        }
                                        catch
                                        {
                                            // Feature not supported on device
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Vibration.Cancel();//关闭震动
                                            r.Stop();//关闭铃声
                                        }
                                        catch
                                        {
                                            // Feature not supported on device
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                //RunOnUiThread(() => { msg.Text = e.Message; });
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
        public double Calibration(double X,double Xmin,double Ymin,double Xmax,double Ymax)
        {
            return (X - Xmin) * (Ymax - Ymin) / (Xmax - Xmin) + Ymin;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}