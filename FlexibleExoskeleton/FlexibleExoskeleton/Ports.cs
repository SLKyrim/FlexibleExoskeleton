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

        //传感器数据 
        public float[] pressures = new float[4]; // 4个压力传感器
        public Single[] imus = new Single[2]; // 2个IMU
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
                int bufferlen = ReadData_SerialPort.BytesToRead; //先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
                if (bufferlen >= 45) //一个电机有使能，方向，转速，电流4个参数，前两个各占1个字节（2个十六进制位），后两个各占2个字节，故一个电机数据占6个字节，加上一个开始位，两个停止位，故总有1+6*4+2=27字节
                {
                    byte[] bytes = new byte[bufferlen]; //声明一个临时数组存储当前来的串口数据
                    ReadData_SerialPort.Read(bytes, 0, bufferlen); //读取串口内部缓冲区数据到bytes数组
                    ReadData_SerialPort.DiscardInBuffer(); //清空串口内部缓存
                                    
                    UInt16 frame_head = BitConverter.ToUInt16(bytes, 0); // 帧头：(AAAA)H = (43690)D
                   
                    if (frame_head == 43690)
                    {
                        byte[] test = new byte[4];
                        test[0] = bytes[7];
                        test[1] = bytes[6];
                        test[2] = bytes[5];
                        test[3] = bytes[4];

                        pressures[0] = BitConverter.ToSingle(test, 0);

                        //for (int f = 0; f < 8; f++)
                        //{
                        //    tempPress[f] = BitConverter.ToInt16(bytes, f * 2);    //1个byte（字节）是8位，tempPress是Int16，即16位，所以一个Int16占byte数组两个位   
                        //    tempAngle[f] = BitConverter.ToInt16(bytes, f * 2 + 16);
                        //    dirangle[f] = (Convert.ToDouble(tempAngle[f]) * (3.3 / 4096) - 0.7444) / 1.5 * 180;
                        //}
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
