using System;
using Microsoft.Research.DynamicDataDisplay.Common;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace FlexibleExoskeleton
{
    public class MyPoint //定义一个点
    {
        public int Date { get; set; }
        public double _point { get; set; }
        public MyPoint(double point, int date)
        {
            this.Date = date; //横坐标
            this._point = point; //纵坐标
        }
    }

    public class PointCollection : RingArray<MyPoint> //一幅图中显示TOTAL_POINTS个点
    {
        private const int TOTAL_POINTS = 200; //图上最多保存的点数
        public PointCollection() : base(TOTAL_POINTS) { }
    }

    class ChartPlotter
    {
        #region 声明

        //声明绘制对象
        public PointCollection Total_pointcollection_Cur;

        private DispatcherTimer timer;
        private static ChartPlotter cp;
        private int count;
        private int lastCount = 0; //记录上次绘图所在位置

        private Ports ports = new Ports();
        #endregion

        public ChartPlotter()
        {
            //为一堆点分配空间
            Total_pointcollection_Cur = new PointCollection();
        }

        public void plotStart(Ports portsIn)
        {
            ports = portsIn;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(plotTimer_Tick);
            timer.Start();
        }

        public void plotStop()
        {
            timer.Stop();
            timer.Tick -= new EventHandler(plotTimer_Tick);

            lastCount = count;
        }

        private void plotTimer_Tick(object sender, EventArgs e)
        {
            cp = App.Current.Resources["Cp"] as ChartPlotter;

            try
            {
                cp.Total_pointcollection_Cur.Add(new MyPoint(ports.imus[0], count));
            }
            catch (Exception)
            {
                MessageBox.Show("绘图失败！");
            }

            count++;
        }
    }
}
