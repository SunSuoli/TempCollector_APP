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

namespace TempCollector.Classes
{
    class GlobalData
    {
        public static double v_min, t_min, v_max, t_max;//标定参数
        public static double warn_temp;//报警温度
    }
}