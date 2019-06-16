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
        private SerialPort data_SerialPort = new SerialPort(); //【读取数据】串口
        #endregion

        #region 扫描串口
        public string[] CheckSerialPortCount()//扫描本地计算机获取可用串口名
        {
            IsOpenSerialPortCount = SerialPort.GetPortNames();
            return IsOpenSerialPortCount;
        }
        #endregion

        #region 串口初始化
        public void Data_SerialPort_Init(string comstring)//【读取数据】串口初始化
        {
            if (data_SerialPort != null) //换端口号时执行
            {
                if (data_SerialPort.IsOpen)
                {
                    data_SerialPort.Close(); // 若串口已占用，则先关闭串口
                }
            }

            data_SerialPort = new SerialPort(); // 串口实例
            data_SerialPort.PortName = comstring; // 串口名
            data_SerialPort.BaudRate = 115200; // 比特率
            data_SerialPort.DataBits = 8; // 数据位
            data_SerialPort.Parity = Parity.None; // 校检位
            data_SerialPort.StopBits = StopBits.One; // 停止位
            data_SerialPort.Open();
            data_SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(DataReceived);
        }
        #endregion

        #region 串口通信接收下位机数据
        private void DataReceived(object sender, SerialDataReceivedEventArgs e)//【读取数据】串口接收数据（算法）
        {
            try
            {
                int bufferlen = data_SerialPort.BytesToRead; //先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
                if (bufferlen >= 27) //一个电机有使能，方向，转速，电流4个参数，前两个各占1个字节（2个十六进制位），后两个各占2个字节，故一个电机数据占6个字节，加上一个开始位，两个停止位，故总有1+6*4+2=27字节
                {

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
