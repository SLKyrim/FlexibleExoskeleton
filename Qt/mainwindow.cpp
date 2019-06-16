#include "mainwindow.h"
#include "ui_mainwindow.h"

MainWindow::MainWindow(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::MainWindow),
    StartButton(createStartButton()),
    StopButton(createStopButton()),
    PortBox1(createPortBox()),
    BaudBox1(createBaudBox()),
    BitNumBox1(createBitNumBox()),
    ParityBox1(createParityBox()),
    StopBox1(createStopBox()),
    text_maxSize1(creatLineEdit())
{
    initial();
    intial_imu(&tFr,&tB,&tMa,&tMi,&tS,&tZ,&tFo);
    m_timer = new QTimer(this);
    timer_flash_axis = new QTimer(this);

    baseLayout = new QGridLayout();
    QVBoxLayout *settingsLayout1 = new QVBoxLayout();
    settingsLayout1->setContentsMargins(0, 10, 0, 0);//setContentsMargins(int left, int top, int right, int bottom);
    settingsLayout1->addWidget(new QLabel("端口号:"));
    settingsLayout1->addWidget(new QLabel("波特率:"));
    settingsLayout1->addWidget(new QLabel("数据位:"));
    settingsLayout1->addWidget(new QLabel("校验位:"));
    settingsLayout1->addWidget(new QLabel("停止位:"));
    settingsLayout1->addWidget(new QLabel("最大个数:"));
    settingsLayout1->addStretch();
    baseLayout->addLayout(settingsLayout1, 0, 1);
    settingsLayout1->setSpacing(20);

    QVBoxLayout *settingsLayout = new QVBoxLayout();
    settingsLayout->setContentsMargins(0, 7, 0, 0);
    settingsLayout->addWidget(PortBox1);
    settingsLayout->addWidget(BaudBox1);
    settingsLayout->addWidget(BitNumBox1);
    settingsLayout->addWidget(ParityBox1);
    settingsLayout->addWidget(StopBox1);
    settingsLayout->addWidget(text_maxSize1);
    settingsLayout->addWidget(StartButton);
    //settingsLayout->addWidget(StopButton);
    settingsLayout->addStretch();
    baseLayout->addLayout(settingsLayout, 0, 2);
    settingsLayout->setSpacing(12);

    foreach(const QSerialPortInfo &info, QSerialPortInfo::availablePorts())
    {
        QSerialPort serial;
        serial.setPort(info);
        if(serial.open(QIODevice::ReadWrite))
        {
            PortBox->addItem(serial.portName());
            serial.close();
        }
    }



    splinechart();
    setLayout(baseLayout);
    connect(button_start,SIGNAL(clicked(bool)),this,SLOT(on_openButton_clicked()));

    //chartView->show();

}

void MainWindow::initial(){
    a=0;
    b=0;
    countAngel=0;
    //intialAngel=0;
    //trigerAngel=false;
    //trigerForce=false;
    N=0;
    countN=0;
    //trigerStop=false;
}

MainWindow::~MainWindow()
{
    delete ui;
}
void MainWindow::splinechart(){
    splineseries = new QSplineSeries();//图表的数据
    splineseries1 = new QSplineSeries();
    splineChart = new QChart();//图表（最顶层）

    //把曲线添加到Qchart的实例中
    splineChart->addSeries(splineseries);  //显示图像
    splineChart->addSeries(splineseries1);

    //初始化并声明坐标轴
    axisX=new QValueAxis();
    axisY=new QValueAxis();
    axisY1=new QValueAxis();

    //设置坐标轴的分布
    splineChart->addAxis(axisX,Qt::AlignBottom);
    splineChart->addAxis(axisY,Qt::AlignLeft);
    splineChart->addAxis(axisY1,Qt::AlignRight);

    //设置不同曲线关联的坐标轴
    splineseries->attachAxis(axisX);
    splineseries->attachAxis(axisY);
    splineseries1->attachAxis(axisX);
    splineseries1->attachAxis(axisY1);

    //设置坐标轴的名称
    axisX->setTitleText("Time");
    axisY->setTitleText("IMU");
    axisY1->setTitleText("Force");

    //设置曲线颜色
    splineseries->setColor(QColor(Qt::darkGreen));
    splineseries1->setColor(QColor(Qt::darkBlue));
    splineseries->setName("IMU");
    splineseries1->setName("Force");
    axisY->setLinePenColor(QColor(Qt::darkGreen));
    axisY1->setLinePenColor(QColor(Qt::darkBlue));
    axisX->setLinePenColor(QColor(Qt::black));
    //    QPen penY1(Qt::darkBlue,3,Qt::SolidLine,Qt::RoundCap,Qt::RoundJoin);
    //    QPen penY2(Qt::darkGreen,3,Qt::SolidLine,Qt::RoundCap,Qt::RoundJoin);
    //    axisY->setLinePen(penY1);
    //    axisY1->setLinePen(penY2);
    //splineChart->createDefaultAxes(); //根据数据集，自动创建坐标轴，坐标轴的区间恰好完全容纳已有的数据集// 将 数据集 添加至图表中
    //splineChart->axisY()->setRange(0, 20);
    //splineChart->axisX()->setRange(0, 20);
    QChartView *chartView = new QChartView(splineChart);//QChartView 可以一步到位直接显示QChart
    baseLayout->addWidget(chartView, 0, 0, 2, 1);
    chartView->setRenderHint(QPainter::Antialiasing);

}
void MainWindow::timer1(){
    qDebug()<<time_clock_x<<time_clock_y;
    //    if(m_timer->isActive()){
    //        m_timer->stop();
    //    }
    time_clock_x+=1;
    time_clock_y+=1;
    if(counter<=100)
        counter+=1;
    splineseries->append(time_clock_x, time_clock_y);
    //    QChart *splineChart = new QChart();//图表（最顶层）
    //    splineChart->addSeries(splineseries);  // 将 数据集 添加至图表中
    //    splineChart->createDefaultAxes();//根据数据集，自动创建坐标轴，坐标轴的区间恰好完全容纳已有的数据集// 将 数据集 添加至图表中
    //    QChartView *chartView = new QChartView(splineChart);//QChartView 可以一步到位直接显示QChart
    //    baseLayout->addWidget(chartView, 0, 0);
    //    chartView->setRenderHint(QPainter::Antialiasing);
    //    setLayout(baseLayout);

}

void MainWindow::timer_flash(){

}


QLineEdit *MainWindow::creatLineEdit(){
    text_maxSize = new QLineEdit();
    text_maxSize->setFixedWidth(80);
    return text_maxSize;
}


QPushButton *MainWindow::createStartButton(){
    button_start = new QPushButton(this);
    button_start->setText("打开串口");
    return button_start;
}

QPushButton *MainWindow::createStopButton(){
    button_stop = new QPushButton();
    button_stop->setText("结束");
    return button_stop;

}

QComboBox *MainWindow::createPortBox()
{
    PortBox = new QComboBox();
    return PortBox;
}

QComboBox *MainWindow::createBaudBox()
{
    BaudBox = new QComboBox();
    //BaudBox->addItem("921600");
    BaudBox->addItem("115200");
    BaudBox->addItem("19200");
    BaudBox->addItem("9600");
    return BaudBox;
}

QComboBox *MainWindow::createBitNumBox()
{
    BitNumBox = new QComboBox();
    BitNumBox->addItem("8");
    BitNumBox->addItem("7");
    BitNumBox->addItem("6");
    BitNumBox->addItem("5");
    return BitNumBox;
}

QComboBox *MainWindow::createParityBox()
{
    ParityBox = new QComboBox();
    ParityBox->addItem("0");
    ParityBox->addItem("1");
    return ParityBox;
}

QComboBox *MainWindow::createStopBox()
{
    StopBox = new QComboBox();
    StopBox->addItem("1");
    StopBox->addItem("2");
    return StopBox;
}

void MainWindow::on_openButton_clicked()
{
    if(button_start->text()==tr("打开串口"))
    {
        serial = new QSerialPort;
        //设置串口名
        serial->setPortName(PortBox->currentText());
        //打开串口
        serial->open(QIODevice::ReadWrite);
        //设置波特率
        serial->setBaudRate(BaudBox->currentText().toInt());
        //设置数据位数
        switch(BitNumBox->currentIndex())
        {
        case 8: serial->setDataBits(QSerialPort::Data8); break;
        default: break;
        }
        //设置奇偶校验
        switch(ParityBox->currentIndex())
        {
        case 0: serial->setParity(QSerialPort::NoParity); break;
        default: break;
        }
        //设置停止位
        switch(StopBox->currentIndex())
        {
        case 1: serial->setStopBits(QSerialPort::OneStop); break;
        case 2: serial->setStopBits(QSerialPort::TwoStop); break;
        default: break;
        }
        //设置流控制
        serial->setFlowControl(QSerialPort::NoFlowControl);
        //关闭设置菜单使能
        PortBox->setEnabled(false);
        BaudBox->setEnabled(false);
        BitNumBox->setEnabled(false);
        ParityBox->setEnabled(false);
        StopBox->setEnabled(false);
        text_maxSize->setEnabled(false);
        maxSize=text_maxSize->text();
        //flashButton->setEnabled(false);
        button_start->setText(tr("关闭串口"));
        //ui->sendButton->setEnabled(true);
        //连接信号槽
        QDateTime time = QDateTime::currentDateTime();//获取系统现在的时间
        str_m1 = time.toString("m");
        int_str_m1=str_m1.toInt();
        QObject::connect(serial, &QSerialPort::readyRead, this, &MainWindow::Read_Data);

    }
    else
    {
        //关闭串口
        serial->clear();
        serial->close();
        serial->deleteLater();

        //恢复设置使能
        PortBox->setEnabled(true);
        BaudBox->setEnabled(true);
        BitNumBox->setEnabled(true);
        ParityBox->setEnabled(true);
        StopBox->setEnabled(true);
        text_maxSize->setEnabled(true);
        //flashButton->setEnabled(true);
        button_start->setText(tr("打开串口"));
        //ui->sendButton->setEnabled(false);
        //splineseries->clear();
        //splineseries1->clear();
        maxSize.clear();
        y_max=0;
        y_min=0;
        x_max=0;
        x_min=0;
        y1_max=0;
        y1_min=0;
        countAngel=0;
    }
}


void MainWindow::Read_Data(){
    float str_float;
    static QByteArray buf;
    buf = serial->readAll();
    QString str=buf.toHex();//(重要)识别出十六进制，没有转换成QString会出现乱码
    if(str.startsWith("5553"))      //&&buf.size()==48
    {
        //        QString strl = str.mid(4,2);
        //        QString strh = str.mid(6,2);
        QString strl = str.mid(4,2); // 第4个往后取2个bytes
        QString strh = str.mid(6,2); // 第6个往后取2个bytes
        QString str3 = strh+strl; // 即数据帧的第4-8个字节
        //qDebug()<<str3.toInt(0,16);
        int str_int=str3.toInt(0,16);
        if(str_int>32767) str_int-=65536;
        //float str_float1 = str_int/32768.0*2000;
        float str_float1 = str_int/32768.0*180.0;
        //float str_float1 = str_int/32768.0*16.0;

        //得到初始角度值
        if(countIntial<99){
            countIntial++;
            intialAngel+=str_float1;
            str_float=0;
        }
        else{
            str_float=str_float1-intialAngel/100;
        }

        QDateTime time1 = QDateTime::currentDateTime();//获取系统现在的时间
        QString str_m = time1.toString("m");//设置显示格式
        QString str_s = time1.toString("ss");
        QString str_z = time1.toString("zzz");
        float s=str_s.toFloat()+(str_m.toFloat()-int_str_m1)*60+str_z.toFloat()/1000;
        qDebug()<<s<<","<<str_float;
        //qDebug()<<pose(str_float,&tFr,&tB,&tMa,&tMi,&tS,&tZ);
        data.append(str_float);
        data_time.append(s);
        point.setX(s);
        point.setY(str_float);
        point_list.append(point);
        pointF.setX(s);
        pointF.setY(pose(str_float,&tFr,&tB,&tMa,&tMi,&tS,&tZ));
        //pointF.setY(generateForce1(str_float));
        point_listF.append(pointF);
        //        generateForce(str_float);
        if(data.size()<=maxSize.toInt()){
            //            for(int i=0; i<data.size(); i++){

            //                if(i==0){
            //                    y_max=data.at(i)+data.at(i)/10;
            //                    y_min=data.at(i)-data.at(i)/10;
            //                }

            //                if(y_min > data.at(i)){
            //                    if(y_min>0)
            //                        y_min = data.at(i)-data.at(i)/10;
            //                    else
            //                        y_min = data.at(i)+data.at(i)/10;

            //                }
            //                if(y_max < data.at(i)) {
            //                    if(y_max>0)
            //                        y_max = data.at(i)+data.at(i)/10;
            //                    else
            //                        y_max =data.at(i)-data.at(i)/10;

            //                }
            //            }

            for(int i=0; i<data_time.size(); i++){

                if(i==0) {
                    x_min=data_time.at(i);
                    x_max=data_time.at(i)+data_time.at(i)/10;
                    //                x_min-=5;
                }
                if(i>0) x_max=data_time.at(i)+0.2;

                //x_max+=x_max/20;
            }


            axisY->setRange(-45,45);
            axisY1->setRange(-300,300);
            //axisY->setRange(y_min,y_max);
            axisX->setRange(x_min, x_max);
            //splineseries->append(s, str_float);
            //splineseries1->append(s, generateForce1(str_float));
            splineseries1->append(s, pose(str_float,&tFr,&tB,&tMa,&tMi,&tS,&tZ));
            a++;
        }
        else{
            b++;
            //qDebug()<<a<<maxSize<<data.size();
            data.removeFirst();
            data_time.removeFirst();
            point_list.removeFirst();
            //            for(int i=0; i<data.size(); i++){

            //                if(i==0){
            //                    y_max=data.at(i)+data.at(i)/10;
            //                    y_min=data.at(i)-data.at(i)/10;
            //                }

            //                if(y_min > data.at(i)){
            //                    if(y_min>0)
            //                        y_min = data.at(i)-data.at(i)/10;
            //                    else
            //                        y_min = data.at(i)+data.at(i)/10;

            //                }
            //                if(y_max < data.at(i)) {
            //                    if(y_max>0)
            //                        y_max = data.at(i)+data.at(i)/10;
            //                    else
            //                        y_max =data.at(i)-data.at(i)/10;

            //                }
            //            }

            for(int i=0; i<data_time.size(); i++){

                if(i==0) {
                    x_min=data_time.at(i);
                    x_max=data_time.at(i)+data_time.at(i)/10;
                    //                x_min-=5;
                }
                if(i>0) x_max=data_time.at(i)+0.2;

                //x_max+=x_max/20;
            }
            axisY->setRange(-45,45 );
            axisY1->setRange(-300,300);
            //axisY->setRange(y_min,y_max);
            axisX->setRange(x_min, x_max);
            splineseries->replace(point_list);
            splineseries1->replace(point_listF);
            //splineseries->append(s, str_float);
        }

        buf.clear();
    }
}


float MainWindow::generateForce1(float buf){
    if(buf > 0 && trigerAngel==false && buf<10 && trigerForce==false) trigerAngel=true;
    if(buf > 5 && trigerAngel==true){
        angelList.append(buf);
    }
    if(!trigerAngel && trigerForce && trigerStop==false){
        angelList.append(buf);
        if(angelList.last()>0){

            if(angelList.last()-angelList.at(angelList.size()-2)>0){
                angelList.removeLast();
            }

            bufN.append(N);
            N=k1*(angelList.last()+c1);
            if(N<0) N=0;

            //            if(N<bufN.last()&&N!=0){


            //                // N=2*bufN.last()-bufN.at(bufN.size()-2);
            //                N=k1*(angelList.last()-1+c1);
            //                qDebug()<<"!!!!!!!!!!!!!";

            //            }
            //qDebug()<<"2"<<angelList.last();
        }
        else {
            bufN.clear();
            //bufN.append(N);
            N=k2*(angelList.last()+c2);
            //qDebug()<<"3"<<angelList.last();
            //            if(N>bufN.last()){
            //                if(bufN.size()<2) N=0;
            //                N=2*bufN.at(bufN.size()-2)-bufN.at(bufN.size()-3);
            //            }
        }
    }
    if(angelList.size()>=4){
        if(angelList.last()-angelList.at(angelList.size()-4)<0.04&&angelList.last()-angelList.at(angelList.size()-4)>-0.04){
            countN++;
            qDebug()<<"!!!!!!!!!!!!!!!!!!"<<countN<<","<<angelList.last()<<","<<angelList.at(angelList.size()-4);
            if(countN==2){
                N=0;
                countN=0;
                trigerStop=true;
                trigerForce=false;
                trigerAngel=true;
                angelList.clear();
            }
        }
        else{
            trigerStop=false;
        }
    }
    if(N<0){
        N=0;
        trigerForce=false;
        angelList.clear();
    }
    return N;
}



float MainWindow::pose(float buf,bool *trigerFront,bool *trigerBack,bool *trigerMax,bool *trigerMin,bool *trigerStop,bool *trigerZero){
    angleList.append(buf);



    if(buf>8 && *trigerFront==false) {
        *trigerBack=true;
        *trigerFront=false;

        if(*trigerBack==true && *trigerStop==false && *trigerMax==false){
            if(angleList.size()>=2){
                if(angleList.last()-angleList.at(angleList.size()-2)<0){
                    *trigerMax = true;
                }
            }
        }
    }



    if(buf<-6 && *trigerBack==false) {
        *trigerFront=true;
        *trigerBack=false;

        if(*trigerFront==true && *trigerStop==false && *trigerMin==false){
            if(angleList.size()>=2){
                if(angleList.last()-angleList.at(angleList.size()-2)>0){
                    *trigerMin = true;
                    qDebug()<<"inMin"<<angleList.last();
                }
            }
        }
    }


    if(angleList.last()<0)
        *trigerZero=true;
    else
        *trigerZero=false;



    if(angleList.size()>=4){
        if(angleList.last()-angleList.at(angleList.size()-3)<0.04 && angleList.last()-angleList.at(angleList.size()-3)>-0.04){
            countN++;            
            if(countN==2){
                countN=0;
                *trigerStop=true;
                angleList.clear();
            }
        }
        else{
            *trigerStop=false;
        }
    }
    return generateForce(&tFr,&tB,&tMa,&tMi,&tS,&tZ,&tFo);
}

float MainWindow::generateForce(bool *trigerFront,bool *trigerBack,bool *trigerMax,bool *trigerMin,bool *trigerStop,bool *trigerZero,bool *trigerForce){

    if(*trigerMin==true && *trigerStop==false && *trigerForce==false){
        c1=-(angleList.last());
        k1=200/c1;
        c2=angleList.last();
        k2=200/c2;
        *trigerForce=true;
    }

    if(*trigerMin==true && *trigerForce==true){
        if(*trigerZero==false){
            N=k1*(angleList.last()+c1);
            if(N<0) N=0;
        }
        else{
            N=k2*(angleList.last()+c2);
        }
    }

    if(*trigerMax==true && *trigerStop==false && *trigerForce==false){
        c1=-(angleList.last());
        k1=200/c1;
        c2=angleList.last()/4;
        k2=200/c2;
        *trigerForce=true;
    }

    if(*trigerMax==true && *trigerForce==true && *trigerStop==false){
        if(*trigerZero==false){
            N=k1*(angleList.last()+c1);
            if(N<0) N=0;
        }
        else{
            N=k2*(angleList.last()+c2);
        }
    }

    if(N<0 || *trigerStop==true){
        N=0;
        intial_imu(&tFr,&tB,&tMa,&tMi,&tS,&tZ,&tFo);
        angleList.clear();
    }
    return N;
}


void MainWindow::intial_imu(bool *trigerFront,bool *trigerBack,bool *trigerMax,bool *trigerMin,bool *trigerStop,bool *trigerZero,bool *trigerForce){
    *trigerFront=false;
    *trigerBack=false;
    *trigerMax=false;
    *trigerMin=false;
    *trigerStop=false;
    *trigerZero=false;
    *trigerForce=false;
}
