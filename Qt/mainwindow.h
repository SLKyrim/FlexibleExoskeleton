#ifndef MAINWINDOW_H
#define MAINWINDOW_H


#include <QtWidgets/QWidget>
#include <QChartView>
#include <QtWidgets/QGridLayout>
#include <QtWidgets/QFormLayout>
#include <QtWidgets/QMainWindow>
#include <QtCharts/QPieSeries>
#include <QtCharts/QPieSlice>
#include <QtCharts/QAbstractBarSeries>
#include <QtCharts/QPercentBarSeries>
#include <QtCharts/QStackedBarSeries>
#include <QtCharts/QBarSeries>
#include <QtCharts/QBarSet>
#include <QtCharts/QLineSeries>
#include <QtCharts/QSplineSeries>
#include <QtCharts/QScatterSeries>
#include <QtCharts/QAreaSeries>
#include <QtCharts/QLegend>
#include <QtCore/QTime>
#include <QDebug>
#include <QTimer>
#include <qdatetime.h>
#include <QTime>
#include <QtSerialPort/QSerialPort>
#include <QtSerialPort/QSerialPortInfo>
#include<QPushButton>
#include <QtWidgets/QLabel>
#include <QtWidgets/QComboBox>
#include <QAbstractSeries>
#include <QList>
#include <QTextEdit>
#include <QLineEdit>
#include <QPointF>
#include <QtCharts>
#include <QtCore/qmath.h>
QT_CHARTS_USE_NAMESPACE

namespace Ui {
class MainWindow;
}

class MainWindow : public QWidget
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

private:
    Ui::MainWindow *ui;
    QList<int> numbersList;
    QTimer *m_timer;
    QTimer *timer_flash_axis;
    QGridLayout *baseLayout;
    QSplineSeries *splineseries;
    QSplineSeries *splineseries1;
    QValueAxis* axisX;
    QValueAxis* axisY;
    QValueAxis* axisY1;
    QChart *splineChart;
    int time_clock_x;
    int time_clock_y;
    int counter;
    QString str_m1;
    int int_str_m1;
    void splinechart();
    QSerialPort *serial;
    QList<float> data;
    QList<float> data_time;
    QList<float> angelList;
    QPushButton *createStartButton();
    QPushButton *createStopButton();
    QComboBox *createPortBox();
    QComboBox *createBaudBox();
    QComboBox *createBitNumBox();
    QComboBox *createParityBox();
    QComboBox *createStopBox();
    QLineEdit *creatLineEdit();

    QPushButton *button_start;
    QPushButton *button_stop;
    QComboBox *PortBox;
    QComboBox *BaudBox;
    QComboBox *BitNumBox;
    QComboBox *ParityBox;
    QComboBox *StopBox;

    QPushButton *StartButton;
    QPushButton *StopButton;
    QComboBox *PortBox1;
    QComboBox *BaudBox1;
    QComboBox *BitNumBox1;
    QComboBox *ParityBox1;
    QComboBox *StopBox1;

    QLineEdit *text_maxSize;
    QLineEdit *text_maxSize1;

    float buf_max_min;
    float x_max;
    float x_min;
    float y_max;
    float y_min;
    float y1_max;
    float y1_min;
    QString maxSize;
    QList<QPointF> point_list;
    QList<QPointF> point_listF;
    QPointF point;
    QPointF pointF;

    void initial();
    float generateForce1(float buf);

    int a;
    int b;
    int countAngel;
    int countIntial;
    float intialAngel;
    bool trigerForce;
    bool trigerAngel;
    bool trigerStop;
    //力参数
//    float k1;
//    float c1;
//    float k2;
//    float c2;
    float N;
    QList<float> bufN;
    QList<float> angle;
    int countN;


    float pose(float buf,bool *trigerFront,bool *trigerBack,bool *trigerMax,bool *trigerMin,bool *trigerStop,bool *trigerZero);
    float generateForce(bool *trigerFront,bool *trigerBack,bool *trigerMax,bool *trigerMin,bool *trigerStop,bool *trigerZero,bool *trigerForce);
    QList<float> angleList;
    void intial_imu(bool *trigerFront,bool *trigerBack,bool *trigerMax,bool *trigerMin,bool *trigerStop,bool *trigerZero,bool *trigerForce);
    float k1;
    float c1;
    float k2;
    float c2;
    bool tFr;
    bool tB;
    bool tMa;
    bool tMi;
    bool tS;
    bool tZ;
    bool tFo;





    //QDateTimeAxis dataAxisX;//时间类型轴(用作X轴)
private slots:
    void timer1();
    void timer_flash();
    void on_openButton_clicked();
    void Read_Data();

};


#endif // MAINWINDOW_H
