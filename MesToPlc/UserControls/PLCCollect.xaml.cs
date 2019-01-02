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
using MesToPlc.Models;
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
        string plcPort;
        string plcIp;
        public PLCCollect()
        {
            InitializeComponent();
            this.Loaded += PLCCollect_Loaded;
        }

        private void PLCCollect_Loaded(object sender, RoutedEventArgs e)
        {
            plcPort = ini.ReadIni("Demo", "Port");
            plcIp = ini.ReadIni("Demo", "Ip");
            ConnectToPlc();
            PLCGetTimer.Interval = TimeSpan.FromSeconds(1);
            PLCGetTimer.Tick += PLCGetTimer_Tick;
            PLCGetTimer.Start();
        }

        private void PLCGetTimer_Tick(object sender, EventArgs e)
        {
            //检测连接是否成功
            if(socPlc.IsConnected)
            {
                SetConnectState(ConnectResult.Success);
                string senddata = "000100000006010300C8007B";
                CharacterConversion cc = new CharacterConversion();
                socPlc.Send(socPlc.ClientSocket, cc.HexConvertToByte(senddata));
            }
            else
            {
                socPlc.NewMessageEvent -= SocPlc_NewMessageEvent;
                ConnectToPlc();
                if(socPlc.IsConnected)
                {
                    SetConnectState(ConnectResult.Success);
                }
                else
                {
                    SetConnectState(ConnectResult.Fail);
                }
            }
        }

        private void SetConnectState(BitmapImage connectResult)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ConnectState.Source = connectResult;
            }));
        }

        private void ConnectToPlc()
        {
            
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

        private string title;
        /// <summary>
        /// 名称
        /// </summary>

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                this.txbTitle.Text = title;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.spState.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 点击状态边框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bdState_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.spState.Visibility == Visibility.Collapsed)
            {
                this.spState.Visibility = Visibility.Visible;
            }
            else
            {
                this.spState.Visibility = Visibility.Collapsed;
            }
        }

        private void bdXinXi_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.spXinXin.Visibility == Visibility.Collapsed)
            {
                this.spXinXin.Visibility = Visibility.Visible;
            }
            else
            {
                this.spXinXin.Visibility = Visibility.Collapsed;
            }
        }

        private void bdSet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.spSet.Visibility == Visibility.Collapsed)
            {
                this.spSet.Visibility = Visibility.Visible;
            }
            else
            {
                this.spSet.Visibility = Visibility.Collapsed;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(this.txtIP.Text) || string.IsNullOrEmpty(this.txtPort.Text))
            {
                MessageBox.Show("IP地址或端口号不能为空");
                return;
            }
            else
            {
                ini.WriteIni(this.txbTitle.Text, "IP", this.txtIP.Text);
                ini.WriteIni(this.txbTitle.Text, "Port", this.txtPort.Text);
            }
        }
    }
}
