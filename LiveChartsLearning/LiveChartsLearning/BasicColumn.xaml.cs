﻿using System;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace LiveChartsLearning
{
    /// <summary>
    /// BasicColumn.xaml 的交互逻辑
    /// </summary>
    public partial class BasicColumn : UserControl
    {
        public BasicColumn()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 10, 50, 39, 50 }
                }
            };

            //adding series will update and animate the chart automatically
            SeriesCollection.Add(new ColumnSeries
            {
                Title = "2016",
                Values = new ChartValues<double> { 11, 56, 42, 48 }
            });

            //also adding values updates and animates the chart automatically
            //SeriesCollection[1].Values.Add(48d);

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            Formatter = value => value.ToString("N");

            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void update_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var r = new Random();

            SeriesCollection[0].Values = new ChartValues<double> { r.Next(0,100), r.Next(0, 100), r.Next(0, 100), r.Next(0, 100) };
            SeriesCollection[1].Values = new ChartValues<double> { r.Next(0, 100), r.Next(0, 100), r.Next(0, 100), r.Next(0, 100) };
        }
    }
}
