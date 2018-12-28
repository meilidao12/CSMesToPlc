using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunicationServers.Sockets;
using Services;
using ProtocolFamily.Modbus;
using System.Net.Sockets;
using System.Windows.Threading;
using MesToPlc.Models;
using Services.DataBase;
using System.Diagnostics;
using MesToPlc;
namespace MesToPlc.Pages
{
    

    /// <summary>
    /// OperatePage.xaml 的交互逻辑
    /// </summary>
    public partial class OperatePage : Page
    {
        //SqlHelper sql = new SqlHelper();
        //Socket socketClient;
        //SocketServer socketServer;
        //IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        //CharacterConversion characterConversion;
        //DispatcherTimer LinkToMesTimer = new DispatcherTimer();
        //DispatcherTimer LinkToMesStateTimer = new DispatcherTimer();
        //DispatcherTimer LinkToPlcTimer = new DispatcherTimer();
        //DispatcherTimer VerifyTimer = new DispatcherTimer();
        //int PlcDelayCount;
        //int MesDelayCount;
        public OperatePage()
        {
            InitializeComponent();
            this.Loaded += OperatePage_Loaded;
        }

        private void OperatePage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
