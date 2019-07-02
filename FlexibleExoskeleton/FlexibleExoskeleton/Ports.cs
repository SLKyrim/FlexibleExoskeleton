using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace FlexibleExoskeleton
{
    // 集成串口功能的类
    class Ports
    {
        #region 声明
        //获取可用串口名
        private string[] IsOpenSerialPortCount = null;

        //声明串口实例
        private SerialPort ReadData_SerialPort = new SerialPort(); //【读取数据】串口

        //下位机数据 
        public float[] ActualForce = new float[4]; // 4个实际助力
        public float[] IdealForce = new float[4]; // 4个预测助力
        public float[] imus = new float[2]; // 2个IMU
        public float[] leds = new float[4]; // 4个LED触发信号
        #endregion

        #region 扫描串口
        public string[] CheckSerialPortCount()//扫描本地计算机获取可用串口名
        {
            IsOpenSerialPortCount = SerialPort.GetPortNames();
            return IsOpenSerialPortCount;
        }
        #endregion

        #region 串口初始化
        public void ReadData_SerialPort_Init(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)//【读取数据】串口初始化
        {
            if (ReadData_SerialPort != null) //换端口号时执行
            {
                if (ReadData_SerialPort.IsOpen)
                {
                    ReadData_SerialPort.Close(); // 若串口已占用，则先关闭串口
                }
            }

            ReadData_SerialPort = new SerialPort(); // 串口实例
            ReadData_SerialPort.PortName = portName; // 串口名
            ReadData_SerialPort.BaudRate = baudRate; // 比特率
            ReadData_SerialPort.Parity = parity; // 校检位
            ReadData_SerialPort.DataBits = dataBits; // 数据位     
            ReadData_SerialPort.StopBits = stopBits; // 停止位
            ReadData_SerialPort.Open();
            ReadData_SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(DataReceived);

            // 使空载时可视化绘图窗口有读数
            for (int i = 0; i < 4; i++)
            {
                ActualForce[i] = 10.7F;
                IdealForce[i] = 10.2F;
                leds[i] = 1.2F;
            }
            imus[0] = 10;
            imus[1] = 10;
            //led = 1;
        }

        public bool SerialPortClose()//关闭串口
        {
            if (ReadData_SerialPort != null)
            {
                ReadData_SerialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(DataReceived);
                ReadData_SerialPort.Close();
            }
            return true;
        } 
        #endregion

        #region 串口通信接收下位机数据
        private void DataReceived(object sender, SerialDataReceivedEventArgs e)//【读取数据】串口接收数据（算法）
        {
            try
            {
                // 一个16进制是4位，2个16进制即1个字节
                // 0-1两字节是帧头AAAA
                // 2-3两字节不管
                // 4-7四字节是第1个实际助力值(0-3)
                // 8-11四字节是第2个实际助力值(4-7)
                // 12-15四字节是第3个实际助力值(8-11)
                // 16-19四字节是第4个实际助力值(12-15)
                // 20-23四字节是第1个预测助力值(16-19)
                // 24-27四字节是第2个预测助力值(20-23)
                // 28-31四字节是第3个预测助力值(24-27)
                // 32-35四字节是第4个预测助力值(28-31)
                // 36-39四字节是左腿IMU(32-35)
                // 40-43四字节是右腿IMU(36-39)
                // 44-47四字节是左前髋LED触发信号(40-43)(为1时助力)
                // 48-51四字节是左后髋LED触发信号(44-47)(为1时助力)
                // 52-55四字节是右前髋LED触发信号(48-51)(为1时助力)
                // 56-59四字节是右后髋LED触发信号(52-55)(为1时助力)
                int bufferlen = ReadData_SerialPort.BytesToRead; //先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
                if (bufferlen >= 57) 
                {
                    byte[] bytes = new byte[bufferlen]; //声明一个临时数组存储当前来的串口数据
                    ReadData_SerialPort.Read(bytes, 0, bufferlen); //读取串口内部缓冲区数据到bytes数组
                    ReadData_SerialPort.DiscardInBuffer(); //清空串口内部缓存
                                    
                    UInt16 frame_head = BitConverter.ToUInt16(bytes, 0); // 帧头：(AAAA)H = (43690)D
                   
                    if (frame_head == 43690)
                    {
                        
                        byte[] temp = new byte[44];

                        for (int i = 0; i < 12; i++) // 似乎BitConverter读取数据的高低位顺序与数组相反，所以这里需要调转顺序
                        {
                            temp[i * 4] = bytes[i * 4 + 7];
                            temp[i * 4 + 1] = bytes[i * 4 + 6];
                            temp[i * 4 + 2] = bytes[i * 4 + 5];
                            temp[i * 4 + 3] = bytes[i * 4 + 4];
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            ActualForce[i] = BitConverter.ToSingle(temp, i * 4);
                            IdealForce[i] = BitConverter.ToSingle(temp, i * 4 + 16);
                            leds[i] = BitConverter.ToSingle(temp, i * 4 + 40);
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            imus[i] = BitConverter.ToSingle(temp, i * 4 + 32);                          
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("端口读取数据失败！");
            }
        }
        #endregion
    }
}
