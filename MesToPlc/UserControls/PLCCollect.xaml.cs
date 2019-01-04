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
using ProtocolFamily.ChangShaChuangYan;
using System.Threading;
using Services.DataBase;
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
        DianJiZhiJia dian = new DianJiZhiJia();
        string plcPort;
        string plcIp;
        bool blColor = false; //当与plc通讯史通过Color的变化改变矢量字体的颜色
        public PLCCollect()
        {
            InitializeComponent();
            this.Loaded += PLCCollect_Loaded;
        }

        private void PLCCollect_Loaded(object sender, RoutedEventArgs e)
        {
            SimpleLogHelper.Instance.WriteLog(LogType.Info, "连接数据库：" + ConnectToDataBase());
            this.txtPort.Text  = ini.ReadIni(this.txbTitle.Text, SetModel.Port);
            this.txtIP.Text = ini.ReadIni(this.txbTitle.Text, SetModel.IP);
            ConnectToPlc();
            PLCGetTimer.Interval = TimeSpan.FromSeconds(1);
            PLCGetTimer.Tick += PLCGetTimer_Tick;
            PLCGetTimer.Start();
        }

        private void PLCGetTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                //检测连接是否成功
                if (socPlc.IsConnected)
                {
                    if (this.IsIpOrPortChange())
                    {
                        socPlc.Disconnect();
                        this.ConnectToPlc();
                    }
                    if(socPlc.IsConnected)
                    {
                        SetConnectState(ConnectResult.Success);
                        string senddata = ini.ReadIni("Config", "SendData");  //01 E8 00 00 00 06 01 03 9D 08 00 7C
                        if(senddata.Length!= 24)
                        {
                            SimpleLogHelper.Instance.WriteLog(LogType.Info, "SendData:" + senddata + " Length: " + senddata.Length);
                        }
                        CharacterConversion cc = new CharacterConversion();
                        socPlc.Send(socPlc.ClientSocket, cc.HexConvertToByte(senddata));
                    }
                    else
                    {
                        SetConnectState(ConnectResult.Fail);
                    }
                }
                else
                {
                    socPlc.NewMessageEvent -= SocPlc_NewMessageEvent;
                    if (ConnectToPlc())
                    {
                        SetConnectState(ConnectResult.Success);
                    }
                    else
                    {
                        SetConnectState(ConnectResult.Fail);
                        SetConnectState(imgAutoRun, "0");
                        SetConnectState(imgCheckState, "0");
                        SetConnectState(imgErrorState, "0");
                        SetConnectState(imgWaitState, "0");
                    }
                }
                //检测数据库连接状态
                if (!sql.TestConn)
                {
                    if(ConnectToDataBase())
                    {
                        this.txbCaiJiXinXi.Foreground = new SolidColorBrush(Colors.White); //红色表示与数据库没有连接成功
                    }
                    else
                    {
                        this.txbCaiJiXinXi.Foreground = new SolidColorBrush(Colors.Red); //红色表示与数据库没有连接成功
                    }
                }
                
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// 连接到数据库
        /// </summary>
        /// <returns></returns>
        private bool ConnectToDataBase()
        {
            sql.Open();
            return sql.TestConn;
        }

        private void SetConnectState(BitmapImage connectResult)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ConnectState.Source = connectResult;
            }));
        }

        private void SetConnectState(Image image, string result)
        {
            BitmapImage connectResult;
            if (result == "0")
            {
                connectResult = ConnectResult.Normal;
            }
            else
            {
                connectResult = ConnectResult.Success;
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                image.Source = connectResult;
            }));
        }

        /// <summary>
        /// 判断IP地址或端口号是否有变化 有变化返回true
        /// </summary>
        /// <returns></returns>
        private bool IsIpOrPortChange()
        {
            if (plcPort != ini.ReadIni(this.txbTitle.Text, SetModel.Port) || plcIp != ini.ReadIni(this.txbTitle.Text, SetModel.IP))
            {
                return true;
            }
            return false;
        }

        private bool ConnectToPlc()
        {
            plcPort = ini.ReadIni(this.txbTitle.Text, SetModel.Port);
            plcIp = ini.ReadIni(this.txbTitle.Text, SetModel.IP);
            socPlc = new SocketClientEx();
            socPlc.NewMessageEvent += SocPlc_NewMessageEvent;
            socPlc.IsConnected = socPlc.Connnect(plcPort, plcIp);
            return socPlc.IsConnected;
        }

        private void SocPlc_NewMessageEvent(Socket socket, byte[] Message)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var model = ProtocolHelper.CreateProtocol(this.txbTitle.Text);
                    string data = socPlc.ByteConvertToString(Message);
                    SimpleLogHelper.Instance.WriteLog(LogType.Info, this.txbTitle.Text + ":   " + data);
                    if (data.Length < 494) return;
                    if (model.AnalysisData(data.Substring(18, data.Length - 18)))
                    {
                        this.txtLeiJiShangDianShiJian.Text = model.PowerTime;
                        this.txtLeiJiShouQuanShiJian.Text = model.AuthorizeTimer;
                        this.txtLeiJiGuZhangShiJian.Text = model.ErrorTimer;
                        this.txtLeiJiJianXiuShiJian.Text = model.CheckTimer;
                        this.txtLeiJiDaiJiShiJian.Text = model.WaitTimer;
                        this.txtLeiJiYunXingShiJian.Text = model.RunTimer;
                        this.txtJiePaiZhouQiShiJian.Text = model.CycleTimer;
                        this.txtLeiJiChanLiang.Text = model.SumCount;
                        this.txtDangBanChanLiang.Text = model.ClassCount;
                        SetConnectState(imgAutoRun, model.AutoRun);
                        SetConnectState(imgCheckState, model.CheckState);
                        SetConnectState(imgErrorState, model.ErrorState);
                        SetConnectState(imgWaitState, model.WaitState);
                    }
                    ChangColor();
                    //存储到数据库
                    if(sql.TestConn)
                    {
                        string sqlcommand = string.Format("INSERT INTO  PlcData" +
                            "(Name,PowerTimer,AuthorizeTimer,ErrorTimer,CheckTimer,WaitTimer," +
                            "RunTimer,CycleTimer,SumCount,ClassCount) values ('{0}','{1}','{2}','{3}'," +
                            "'{4}','{5}','{6}','{7}','{8}','{9}')", this.txbTitle.Text,model.PowerTime
                            ,model.AuthorizeTimer,model.ErrorTimer,model.CheckTimer,model.WaitTimer,
                            model.RunTimer,model.CycleTimer,model.SumCount,model.ClassCount);
                        sql.Execute(sqlcommand);
                    }
                }));
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, ex);
            }
        }



        /// <summary>
        ///  改变采集信息矢量图标颜色
        /// </summary>
        private void ChangColor()
        {
            if (blColor)
            {
                this.txbXinXi.Foreground = new SolidColorBrush(Colors.DarkGreen);
                this.txbState.Foreground = new SolidColorBrush(Colors.DarkGreen);
            }
            else
            {
                this.txbXinXi.Foreground = new SolidColorBrush(Colors.White);
                this.txbState.Foreground = new SolidColorBrush(Colors.White);
            }
            blColor = !blColor;
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

        //private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    this.spState.Visibility = Visibility.Collapsed;
        //}

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
                ini.WriteIni(this.txbTitle.Text,SetModel.IP , this.txtIP.Text);
                ini.WriteIni(this.txbTitle.Text, SetModel.Port, this.txtPort.Text);
                MessageBox.Show("保存成功");
            }
        }
    }
}
