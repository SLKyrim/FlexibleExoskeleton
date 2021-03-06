﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.IO;
using System.Windows.Threading;
using System.Xml.Serialization;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace FlexibleExoskeleton
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region 声明
        // 声明串口类实例
        private Ports ports = new Ports();

        // 扫描串口
        private string[] SPCount = null; //用来存储计算机串口名称数组
        private int comcount = 0; //用来存储计算机可用串口数目，初始化为0
        private bool flag = false;

        // 串口参数(默认)
        private string portName = null; // 串口名
        private int baudRate = 115200; // 波特率
        private Parity parity = Parity.None; // 校检位
        private int dataBits = 8; // 数据位
        private StopBits stopBits = StopBits.One; // 停止位

        // 绘图
        private bool IsReading = false; // 动态绘图的标记：true为开始绘图，false为停止绘图
        private int NUM_POINTS = 60; // ，每条动态曲线的最大点数

        // 人体助力指示灯
        private bool IsLight = false;
        #endregion

        #region 界面初始化
        private void Window_Loaded(object sender, RoutedEventArgs e)//打开程序窗口时执行
        {
            DispatcherTimer showCurTimer = new DispatcherTimer();
            showCurTimer.Tick += new EventHandler(ShowCurTimer); //Tick是超过计时器间隔时发生事件，此处为Tick增加了一个叫ShowCurTimer的取当前时间并扫描串口的委托
            showCurTimer.Interval = new TimeSpan(0, 0, 0, 1, 0); //设置刻度之间的时间值，设定为1秒（即文本框会1秒改变一次输出文本）
            showCurTimer.Start();

            DispatcherTimer showDataTimer = new DispatcherTimer();
            showDataTimer.Tick += new EventHandler(ShowDataTimer); //增加了一个叫ShowSenderTimer的在电机和传感器的只读文本框中输出信息的委托
            showDataTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);  //文本变化间隔是??毫秒(并不准确)
            showDataTimer.Start();

            //DispatcherTimer LedTimer = new DispatcherTimer();
            //LedTimer.Tick += new EventHandler(LedTimer);
            //LedTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); 
            //LedTimer.Start();
        }

        private void ShowCurTimer(object sender, EventArgs e)//取当前时间并扫描可用串口的委托
        {
            string timeDateString = "";
            DateTime now = DateTime.Now;
            timeDateString = string.Format("{0}年{1}月{2}日 {3}:{4}:{5}",
                now.Year,
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));

            timeDateTextBlock.Text = timeDateString;

            ScanPorts();//扫描可用串口
        }

        private void Window_Closed(object sender, EventArgs e) // 关闭程序时执行
        {
            ports.SerialPortClose(); // 关闭串口
        }
        #endregion

        #region LED（及文本框数据显示）
        public void ShowDataTimer(object sender, EventArgs e)//电机状态，压力，倾角，角度传感器状态的文本输出
        {
            //4个压力传感器的文本框输出
            Pressure1_Textbox.Text = ports.ActualForce[0].ToString("F");
            Pressure2_Textbox.Text = ports.ActualForce[1].ToString("F");
            Pressure3_Textbox.Text = ports.ActualForce[2].ToString("F");
            Pressure4_Textbox.Text = ports.ActualForce[3].ToString("F");

            //2个IMU的文本框输出
            IMU1_Textbox.Text = ports.imus[0].ToString("F");
            IMU2_Textbox.Text = ports.imus[1].ToString("F");

            // LED显示
            if ((int)(ports.leds[0]) == 1) // 对左前髋助力
            {
                LeftFhipLight_Ellipse.Fill = new SolidColorBrush(Color.FromArgb(230, 20, 200, 20));
                LeftFhip_Text.Foreground = Brushes.Red;
            }
            else
            {
                LeftFhipLight_Ellipse.Fill = Brushes.Gray;
                LeftFhip_Text.Foreground = Brushes.Gray;
            }

            if ((int)(ports.leds[1]) == 1) // 对左后髋助力
            {
                LeftBhipLight_Ellipse.Fill = new SolidColorBrush(Color.FromArgb(230, 20, 200, 20));
                LeftBhip_Text.Foreground = Brushes.Red;
            }
            else
            {
                LeftBhipLight_Ellipse.Fill = Brushes.Gray;
                LeftBhip_Text.Foreground = Brushes.Gray;
            }

            if ((int)(ports.leds[2]) == 1) // 对右前髋助力
            {
                RightFhipLight_Ellipse.Fill = new SolidColorBrush(Color.FromArgb(230, 20, 200, 20));
                RightFhip_Text.Foreground = Brushes.Red;
            }
            else
            {
                RightFhipLight_Ellipse.Fill = Brushes.Gray;
                RightFhip_Text.Foreground = Brushes.Gray;
            }

            if ((int)(ports.leds[3]) == 1) // 对右后髋助力
            {
                RightBhipLight_Ellipse.Fill = new SolidColorBrush(Color.FromArgb(230, 20, 200, 20));
                RightBhip_Text.Foreground = Brushes.Red;
            }
            else
            {
                RightBhipLight_Ellipse.Fill = Brushes.Gray;
                RightBhip_Text.Foreground = Brushes.Gray;
            }
        }
        #endregion

        #region 串口扫描
        private void ScanPorts()//扫描可用串口
        {
            SPCount = ports.CheckSerialPortCount(); //获得计算机可用串口名称数组

            ComboBoxItem tempComboBoxItem = new ComboBoxItem();

            if (comcount != SPCount.Length) //SPCount.length其实就是可用串口的个数
            {
                //当可用串口计数器与实际可用串口个数不相符时
                //初始化串口选择下拉窗口并将flag初始化为false
                DataComboBox.Items.Clear();

                tempComboBoxItem = new ComboBoxItem();
                tempComboBoxItem.Content = "请选择端口";
                DataComboBox.Items.Add(tempComboBoxItem);
                DataComboBox.SelectedIndex = 0;

                portName = null;

                flag = false;

                if (comcount != 0)
                {
                    //在操作过程中增加或减少串口时发生
                    //MessageBox.Show("串口数目已改变，请重新选择串口");
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "端口数目已改变，请重新选择端口！";
                }

                comcount = SPCount.Length;     //将可用串口计数器与现在可用串口个数匹配
            }

            if (!flag)
            {
                if (SPCount.Length > 0) //有可用串口时执行
                {

                    comcount = SPCount.Length;

                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                    statusInfoTextBlock.Text = "检测到" + SPCount.Length + "个端口!";

                    for (int i = 0; i < SPCount.Length; i++)
                    {
                        //分别将可用串口添加到三个下拉窗口中
                        string tempstr = SPCount[i];

                        tempComboBoxItem = new ComboBoxItem();
                        tempComboBoxItem.Content = tempstr;
                        DataComboBox.Items.Add(tempComboBoxItem);
                    }

                    flag = true;
                }
                else
                {
                    comcount = 0;
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "未检测到端口!";
                }
            }
        }
        #endregion

        #region 按钮
        private void Start_Button_Click(object sender, RoutedEventArgs e)// 点击【开始监控】按钮时执行
        {
            Button bt = sender as Button;
            DispatcherTimer LedTimer = new DispatcherTimer();

            if (bt.Content.ToString() == "开始监控")
            {
                ports.ReadData_SerialPort_Init(portName, baudRate, parity, dataBits, stopBits); // 串口初始化

                IsReading = !IsReading;
                IsLight = !IsLight;

                if (IsReading)
                {
                    Task.Factory.StartNew(Read); // 动态曲线绘图开始
                    Task.Factory.StartNew(EnergeRead); // 能量监控开始
                    // 环状图监控开始
                    Task.Factory.StartNew(LeftFhipPieRead);                   
                }

                bt.Content = "停止监控";
                bt.Background = Brushes.Red;

                //// 测试绘图开始
                //cp.plotStart(ports);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    ports.leds[i] = 0;
                ports.SerialPortClose();

                IsReading = !IsReading;
                IsLight = !IsLight;

                bt.Content = "开始监控";
                bt.Background = Brushes.GreenYellow;
               
                //// 测试绘图停止
                //cp.plotStop();
            }
        }
        #endregion

        #region 下拉选项框
        private void DataComboBox_DropDownClosed(object sender, EventArgs e) // 【端口号】下拉选项框选择好后执行
        {
            ComboBoxItem item = DataComboBox.SelectedItem as ComboBoxItem; //下拉窗口当前选中的项赋给item
            string tempstr = item.Content.ToString(); //将选中的项目转为字串存储在tempstr中

            for (int i = 0; i < SPCount.Length; i++)
            {
                if (tempstr == SPCount[i])
                {
                    Start_Button.IsEnabled = true; // 配置好端口后使能【开始监控按钮】
                    portName = SPCount[i];
                    //ports.Data_SerialPort_Init(SPCount[i]); //串口初始化
                }
            }
        }

        private void BaudRateComboBox_DropDownClosed(object sender, EventArgs e) // 【波特位】下拉选项框选择好后执行
        {
            int.TryParse(BaudRateComboBox.Text, out baudRate);
        }

        private void ParityComboBox_DropDownClosed(object sender, EventArgs e) // 【校验位】下拉选项框选择好后执行
        {
            string select = ParityComboBox.Text;

            if (select.Contains("Odd"))
            {
                parity = Parity.Odd;
            }
            else if (select.Contains("Even"))
            {
                parity = Parity.Even;
            }
            else if (select.Contains("Space"))
            {
                parity = Parity.Space;
            }
            else if (select.Contains("Mark"))
            {
                parity = Parity.Mark;
            }
        }

        private void DataBitsComboBox_DropDownClosed(object sender, EventArgs e) // 【数据位】下拉选项框选择好后执行
        {
            int.TryParse(DataBitsComboBox.Text, out dataBits);
        }

        private void StopBitsComboBox_DropDownClosed(object sender, EventArgs e) // 【停止位】下拉选项框选择好后执行
        {
            string select = StopBitsComboBox.Text.Trim();

            if (select.Equals("1"))
            {
                stopBits = StopBits.One;
            }
            else if (select.Equals("1.5"))
            {
                stopBits = StopBits.OnePointFive;
            }
            else if (select.Equals("2"))
            {
                stopBits = StopBits.Two;
            }
        }
        #endregion

        #region 绘图

        #region 绘图控件声明
        // 界面属性变更通知接口
        public event PropertyChangedEventHandler PropertyChanged;

        // 动态曲线参数
        private double _axisMax;
        private double _axisMin;
        private double _trend1;
        private double _trend2;
        private double _trend3;
        private double _trend4;

        //动态能量显示板参数
        private double _lastLecture; // 跟随_trend显示的数值
        private double _trend;  // 监控数值（即该绘图的输入）

        //测试绘图
        private ChartPlotter cp;

        public MainWindow()
        {
            InitializeComponent();      

            #region 动态曲线声明
            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the values property will store our values array
            ChartValues1 = new ChartValues<MeasureModel>();
            //ChartValues2 = new ChartValues<MeasureModel>();
            ChartValues3 = new ChartValues<MeasureModel>();
            //ChartValues4 = new ChartValues<MeasureModel>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss"); // HH:24小时制，hh:12小时制

            //AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            //AxisUnit forces lets the axis know that we are plotting seconds
            //this is not always necessary, but it can prevent wrong labeling
            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);

            //The next code simulates data changes every 300 ms

            //IsReading = false;

            DataContext = this;
            #endregion

            #region 动态能量显示板声明
            LastHourSeries = new SeriesCollection
            {
                new LineSeries
                {
                    AreaLimit = -10,
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(3),
                        new ObservableValue(5),
                        new ObservableValue(6),
                        new ObservableValue(7),
                        new ObservableValue(3),
                        new ObservableValue(4),
                        new ObservableValue(2),
                        new ObservableValue(5),
                        new ObservableValue(8),
                        new ObservableValue(3),
                        new ObservableValue(5),
                        new ObservableValue(6),
                        new ObservableValue(7),
                        new ObservableValue(3),
                        new ObservableValue(4),
                        new ObservableValue(2),
                        new ObservableValue(5),
                        new ObservableValue(8)
                    }
                }
            };
            //_trend = 8;

            DataContext = this;
            #endregion

            #region 测试绘图声明
            //EnumerableDataSource<MyPoint> total_Cur;

            //cp = App.Current.Resources["Cp"] as ChartPlotter;

            //total_Cur = new EnumerableDataSource<MyPoint>(cp.Total_pointcollection_Cur);
            //total_Cur.SetXMapping(x => CurAx_total.ConvertToDouble(x.Date));
            //total_Cur.SetYMapping(y => y._point);
            //CurPlot_total.AddLineGraph(total_Cur, Colors.Red, 2, "电机总电流");
            #endregion

            #region 饼状图声明
            LeftFhipPie_SeriesCollection = new SeriesCollection
            {
                // 参数解释：标签名，值，是否在饼中显示名，显示百分比
                new PieSeries
                {
                    Title = "左前髋实际", 
                    Values = new ChartValues<ObservableValue> { new ObservableValue(8) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
                new PieSeries
                {
                    Title = "左前髋预测",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(6) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
            };

            LeftBhipPie_SeriesCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "左后髋实际",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(8) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
                new PieSeries
                {
                    Title = "左后髋预测",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(6) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
            };

            RightFhipPie_SeriesCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "右前髋实际",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(8) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
                new PieSeries
                {
                    Title = "右前髋预测",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(6) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
            };

            RightBhipPie_SeriesCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "右后髋实际",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(8) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
                new PieSeries
                {
                    Title = "右后髋预测",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(6) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("({0:N2}) ({1:P})", chartPoint.Y, chartPoint.Participation) // string.Format格式化小数位:({0:N2})表示第一个数保留2位小数,({1:P})表示第二个数以百分数显示
                },
            };

            //adding values or series will update and animate the chart automatically
            //SeriesCollection.Add(new PieSeries());
            //SeriesCollection[0].Values.Add(5);

            DataContext = this;
            #endregion
        }
        #endregion

        #region 动态曲线
        public ChartValues<MeasureModel> ChartValues1 { get; set; }
        //public ChartValues<MeasureModel> ChartValues2 { get; set; }
        public ChartValues<MeasureModel> ChartValues3 { get; set; }
        //public ChartValues<MeasureModel> ChartValues4 { get; set; }

        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged_ContantChangesChart("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged_ContantChangesChart("AxisMin");
            }
        }

        //public bool IsReading { get; set; }

        private void Read()
        {
            var r = new Random();

            while (IsReading)
            {
                Thread.Sleep(150);
                var now = DateTime.Now;

                // 随机生成数，测试绘图效果；将ChartValues中的Value替换成想显示的值即可
                _trend1 += r.Next(-8, 10);
                _trend2 += r.Next(-8, 10);
                _trend3 += r.Next(-8, 10);
                _trend4 += r.Next(-8, 10);

                ChartValues1.Add(new MeasureModel
                {
                    DateTime = now,
                    Value = ports.imus[0] // 左腿IMU
                    //Value = _trend1
                });

                //ChartValues2.Add(new MeasureModel
                //{
                //    DateTime = now,
                //    Value = _trend2
                //});

                ChartValues3.Add(new MeasureModel
                {
                    DateTime = now,
                    Value = ports.imus[1] // 右腿IMU
                    //Value = _trend3
                });

                //ChartValues4.Add(new MeasureModel
                //{
                //    DateTime = now,
                //    Value = _trend4
                //});

                SetAxisLimits(now);

                //lets only use the last NUM_POINTS values
                if (ChartValues1.Count > NUM_POINTS) ChartValues1.RemoveAt(0);
                //if (ChartValues2.Count > NUM_POINTS) ChartValues2.RemoveAt(0);
                if (ChartValues3.Count > NUM_POINTS) ChartValues3.RemoveAt(0);
                //if (ChartValues4.Count > NUM_POINTS) ChartValues4.RemoveAt(0);
            }
        }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
        }

        protected virtual void OnPropertyChanged_ContantChangesChart(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region 动态能量显示板
        public SeriesCollection LastHourSeries { get; set; }

        public double LastLecture // 实时显示的能量消耗值
        {
            get { return _lastLecture; }
            set
            {
                _lastLecture = value;
                OnPropertyChanged_MaterialCards("LastLecture");
            }
        }

        private void EnergeRead()
        {
            // 能量监控绘图
            var r = new Random();

            Action action = delegate
            {
                //double temp = _trend;
                LastHourSeries[0].Values.Add(new ObservableValue(_trend));
                //LastHourSeries[0].Values.Add(_trend);
                LastHourSeries[0].Values.RemoveAt(0);
                SetLecture();
            };

            while (IsReading)
            {
                //double temp = _trend;

                Thread.Sleep(500);
                //_trend = ports.imus[0];
                _trend += (r.NextDouble() > 0.35 ? 1 : -1) * r.Next(0, 100); // a=(b>0)?b:-b即b>0时,a=b,否则a=-b // 这里让_trend上升的几率为70%
                //_trend += r.Next(-5, 5);
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
        }

        private void SetLecture()
        {
            // 用Var类型预先不用知道变量的类型:必须在定义时初始化;一旦初始化完成，不能再给变量赋与初始化不同的变量;必须是局部变量
            var target = ((ChartValues<ObservableValue>)LastHourSeries[0].Values).Last().Value;
            var step = (target - _lastLecture) / 4;

            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 4; i++)
                {
                    Thread.Sleep(150);
                    LastLecture += step;
                }
                LastLecture = target;
            });
            double temp3 = LastLecture;
        }

        protected virtual void OnPropertyChanged_MaterialCards(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region 饼状图
        public SeriesCollection LeftFhipPie_SeriesCollection { get; set; } // 左前髋环状图
        public SeriesCollection LeftBhipPie_SeriesCollection { get; set; } // 左后髋环状图
        public SeriesCollection RightFhipPie_SeriesCollection { get; set; } // 右前髋环状图
        public SeriesCollection RightBhipPie_SeriesCollection { get; set; } // 右后髋环状图

        public Func<ChartPoint, string> PointLabel { get; set; } // 显示百分比

        private void LeftFhipPieRead()
        {
            while (IsReading)
            {
                Thread.Sleep(1000);

                bool flag = true;

                var r = new Random();

                foreach (var series in LeftFhipPie_SeriesCollection)
                {
                    foreach (var observable in series.Values.Cast<ObservableValue>())
                    {
                        flag = !flag;

                        //if (flag) observable.Value = Math.Abs(_trend1); // 左前髋预测                          
                        //else observable.Value = Math.Abs(_trend2); // 左前髋实际     

                        if (flag) observable.Value = Math.Abs(ports.IdealForce[0]); // 左前髋预测                          
                        else observable.Value = Math.Abs(ports.ActualForce[0]); // 左前髋实际                                   
                    }
                }

                foreach (var series in LeftBhipPie_SeriesCollection)
                {
                    foreach (var observable in series.Values.Cast<ObservableValue>())
                    {
                        flag = !flag;

                        if (flag) observable.Value = Math.Abs(ports.IdealForce[1]); // 左后髋预测                          
                        else observable.Value = Math.Abs(ports.ActualForce[1]); // 左后髋实际    
                    }
                }

                foreach (var series in RightFhipPie_SeriesCollection)
                {
                    foreach (var observable in series.Values.Cast<ObservableValue>())
                    {
                        flag = !flag;

                        if (flag) observable.Value = Math.Abs(ports.IdealForce[2]); // 右前髋预测                          
                        else observable.Value = Math.Abs(ports.ActualForce[2]); // 右前髋实际    
                    }
                }

                foreach (var series in RightBhipPie_SeriesCollection)
                {
                    foreach (var observable in series.Values.Cast<ObservableValue>())
                    {
                        flag = !flag;

                        if (flag) observable.Value = Math.Abs(ports.IdealForce[3]); // 右后髋预测                          
                        else observable.Value = Math.Abs(ports.ActualForce[3]); // 右后髋实际   
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
