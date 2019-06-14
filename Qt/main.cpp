#include "mainwindow.h"
#include <QApplication>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    QMainWindow w;
    MainWindow *widget = new MainWindow();
    w.setCentralWidget(widget);
    w.resize(900, 600);
    w.show();
    return a.exec();
}
