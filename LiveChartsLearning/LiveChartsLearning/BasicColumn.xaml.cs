using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace LiveChartsLearning
{
    /// <summary>
    /// BasicColumn.xaml 的交互逻辑
    /// </summary>
    public partial class BasicColumn : UserControl, INotifyPropertyChanged
    {
        private double _trend1;
        private double _trend2;

        public BasicColumn()
        {
            InitializeComponent();

            ChartValues1 = new ChartValues<double>();
            ChartValues2 = new ChartValues<double>();

            //SeriesCollection = new SeriesCollection
            //{
            //    new ColumnSeries
            //    {
            //        Title = "2015",
            //        Values = new ChartValues<double> { 10, 50, 39, 50 }
            //    }
            //};

            ////adding series will update and animate the chart automatically
            //SeriesCollection.Add(new ColumnSeries
            //{
            //    Title = "2016",
            //    Values = new ChartValues<double> { 11, 56, 42, 48 }
            //});

            //also adding values updates and animates the chart automatically
            //SeriesCollection[1].Values.Add(48d);

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            Formatter = value => value.ToString("N");

            DataContext = this;

            Task.Factory.StartNew(Read);
        }

        public ChartValues<double> ChartValues1 { get; set; }
        public ChartValues<double> ChartValues2 { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void Read()
        {
            var r = new Random();

            while (true)
            {
                SeriesCollection = new SeriesCollection { };

                Thread.Sleep(250); // 画图频率

                _trend1 += r.Next(-8, 10); // y轴取值
                double temp1 = _trend1;

                ChartValues1 = new ChartValues<double> { _trend1, _trend1 + r.Next(-5, 5) };

                _trend2 += r.Next(-8, 10); // y轴取值
                double temp2 = _trend2;

                ChartValues2 = new ChartValues<double> { _trend2, _trend2 + r.Next(-5, 5) };
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
