using System;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using System.Threading;
using System.Threading.Tasks;

namespace LiveChartsLearning
{
    /// <summary>
    /// BasicColumn.xaml 的交互逻辑
    /// </summary>
    public partial class BasicColumn : UserControl
    {
        private double _trend1;
        private double _trend2;

        public BasicColumn()
        {
            InitializeComponent();

            //SeriesCollection = new SeriesCollection
            //{
            //    new ColumnSeries
            //    {
            //        Title = "2015",
            //        Values = new ChartValues<double> { 10, 50 }
            //    }
            //};

            ////adding series will update and animate the chart automatically
            //SeriesCollection.Add(new ColumnSeries
            //{
            //    Title = "2016",
            //    Values = new ChartValues<double> { 11, 56 }
            //});

            ////also adding values updates and animates the chart automatically
            ////SeriesCollection[1].Values.Add(48d);

            //Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            //Formatter = value => value.ToString("N");

            //DataContext = this;

            Task.Factory.StartNew(Read);
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void Read()
        {
            var r = new Random();

            while (true)
            {
                Thread.Sleep(150); // 画图频率

                _trend1 += r.Next(-8, 10); // y轴取值
                _trend2 += r.Next(-8, 10); // y轴取值

                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "2015",
                        Values = new ChartValues<double> { _trend1, _trend1 + r.Next(-5,5) }
                    }
                };

                //adding series will update and animate the chart automatically
                SeriesCollection.Add(new ColumnSeries
                {
                    Title = "2016",
                    Values = new ChartValues<double> { _trend2, _trend2 + r.Next(-5, 5) }
                });

                //also adding values updates and animates the chart automatically
                //SeriesCollection[1].Values.Add(48d);

                Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
                Formatter = value => value.ToString("N");

                DataContext = this;
            }
        }
    }
}
