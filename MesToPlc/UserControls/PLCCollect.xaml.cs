using CommunicationServers.Sockets;
using Services;
using Services.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
using System.Windows.Threading;
using System.Diagnostics;
namespace MesToPlc.UserControls
{
    /// <summary>
    /// PLCCollect.xaml 的交互逻辑
    /// </summary>
    public partial class PLCCollect : UserControl
    {
        SqlHelper sql = new SqlHelper();
        SocketClientEx socPlc;  //连接plc的socket
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        CharacterConversion characterConversion;
        DispatcherTimer PLCGetTimer = new DispatcherTimer(); //采集plc数据
        public PLCCollect()
        {
            InitializeComponent();
            this.Loaded += PLCCollect_Loaded;
        }

        private void PLCCollect_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectToPlc();
            PLCGetTimer.Interval = TimeSpan.FromSeconds(1);
            PLCGetTimer.Tick += PLCGetTimer_Tick;
            PLCGetTimer.Start();
        }

        private void PLCGetTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("开始发送数据");
            socPlc.Send(socPlc.ClientSocket, "09A80000000601030000007B");
        }

        private void ConnectToPlc()
        {
            string plcPort = ini.ReadIni("Demo", "Port");
            string plcIp = ini.ReadIni("Demo", "Ip");
            socPlc = new SocketClientEx();
            socPlc.NewMessageEvent += SocPlc_NewMessageEvent;
            socPlc.Connnect(plcPort, plcIp);
        }

        private void SocPlc_NewMessageEvent(Socket socket, byte[] Message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.tbxDebbug.Text = socPlc.ByteConvertToString(Message);
            }));
        }


    }
}
