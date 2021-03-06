﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using AsyncSocket;
using Cmd;


namespace WifiSimulator
{
    public partial class MainForm : Form
    {
        public delegate void DeleSendBytes();
        public delegate int DeleGetPumpCount(ref List<int> pumpIndexs);

        private static uint counter                    = 1;

        private AsyncClient       m_Client             = null;
        private List<AsyncClient> m_ClientList         = new List<AsyncClient>();     //准备建立多个客户端进行测试
        private List<AsyncClient> m_ClientListSN       = null;
        private List<byte>        m_ReceivedBuffer     = new List<byte>();
        private List<List<byte>>  m_ReceivedBufferList = new List<List<byte>>();      //准备建立多个客户端缓冲区进行测试

        private readonly int      m_SemWaitSeconds     = 2;                           //set wait timeout
        private readonly int      m_SemMaxCount        = 2560;                        //set Semaphore max count
        private const string SEMNOALARMNAME            = "SemNoAlarm";                //set Semaphore name
        private const string SEMLOWBATTERYALARMNAME    = "SemLowBatteryAlarm";        //set Semaphore name
        private const string SEMDEPLETEALARMNAME       = "SemDepleteAlarm";           //set Semaphore name
        private const string SEMBITALARMNAME           = "SemBitAlarm";               //set Semaphore name
        private Semaphore         m_SemNoAlarm         = null;
        private Semaphore         m_SemLowBatteryAlarm = null;
        private Semaphore         m_SemDepleteAlarm    = null;
        private Semaphore         m_SemBitAlarm        = null;
        private bool              m_bKeeping           = true;
        private List<Thread>      m_ThreadPool         = new List<Thread>();
        private System.Timers.Timer m_SendAlarmTimer     = new System.Timers.Timer(); //定时给ADAS发送报警数据
        private int                m_Send_Alarm_Timer_Interval = 0;
        private int                m_SendFlag         = 0;                           //1,低位，2低电，3耗尽  
        private ProductID          m_ProductID        =  ProductID.GrasebyC6;                           //产品ID
        private Graseby            m_Pump             = null;

        private int pumpTypeResponseFlag = 0;
 
        //当前模块的序列号
        private int m_SerialNo = 0;

        public MainForm()
        {
            InitializeComponent();
            Init();
            cbPumpType.Items.AddRange(Enum.GetNames(typeof(ProductID)));
            if (cbPumpType.Items.Count > 0)
                cbPumpType.SelectedIndex = 0;
        }

        public void SetProductID(ProductID id)
        {
            switch(id)
            {
                case ProductID.GrasebyF6:
                    m_ProductID = ProductID.GrasebyF6;
                    m_Pump = new F6(m_Client);
                    break;
                case ProductID.GrasebyC8:
                    m_ProductID = ProductID.GrasebyC8;
                    m_Pump = new C8(m_Client);
                    break;
                case ProductID.GrasebyF8:
                    m_ProductID = ProductID.GrasebyF8;
                    m_Pump = new F8(m_Client);
                    break;
                default:
                    m_ProductID = ProductID.GrasebyC8;
                    m_Pump = new C8(m_Client);
                    break;
            }
        }

        public void SetClient()
        {
            m_Pump.m_Client = m_Client;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            cbPumpCount.SelectedIndex=0;
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!(int.TryParse(config.AppSettings.Settings["SendAlarmTimerInterval"].Value, out m_Send_Alarm_Timer_Interval)))
                m_Send_Alarm_Timer_Interval = 60000;
            m_SendAlarmTimer.Interval = m_Send_Alarm_Timer_Interval;
            m_SendAlarmTimer.Elapsed += OnSendAlarmTimer;
            cbPumpCount.SelectedIndex = 0;
        }

        private int GetPumpCountAndIndex(ref List<int> pumpIndexs)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DeleGetPumpCount(GetPumpCountAndIndex), new object[] { pumpIndexs });
                return 6;
            }
            else
            {
                int iCount = pnlCheckBoxs.Controls.Count;
                foreach (CheckBox col in pnlCheckBoxs.Controls)
                {
                    if (col.Checked)
                        pumpIndexs.Add(Convert.ToInt32(col.Text));
                }
                return Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
            }
        }

        private void OnSendAlarmTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<int> pumpIndexs = new List<int>();
            int pumpCount = GetPumpCountAndIndex(ref pumpIndexs);
            m_Pump.PumpIndexs = pumpIndexs;
            m_Pump.PumpCount = pumpCount;
            m_Pump.SendAlarm();

            //switch(m_SendFlag)
            //{

            //    case 0:
            //        SendNoAlarmSingle();
            //        break;
            //    case 1:
            //        if (m_ProductID == ProductID.GrasebyC6)
            //            SendLowAlarmSingle();
            //        else if (m_ProductID == 8)
            //            SendLowAlarmSingleC8();
            //        else if (m_ProductID == 66)
            //            SendLowAlarmSingleF6();

            //        break;
            //    case 2:
            //        if (m_ProductID == 6)
            //            SendLowVolSingle();
            //        else if (m_ProductID == 8)
            //            SendLowVolSingleC8();
            //        else if (m_ProductID == 66)
            //            SendLowVolSingleF6();

            //        break;
            //    case 3: 
            //         if (m_ProductID == 6)
            //             SendDelepeSingle();
            //        else if (m_ProductID == 8)
            //             SendDelepeSingleC8();
            //         else if (m_ProductID == 66)
            //             SendDelepeSingleF6();
            //        break;
            //    default:break;
            //}
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bKeeping = false;
            try 
            {
                //for (int j = 0; j < m_ThreadPool.Count; j++)
                //{
                //    m_ThreadPool[j].Abort();
                //}
                 //m_SemNoAlarm.Close();        
                 //m_SemLowBatteryAlarm.Close();
                 //m_SemDepleteAlarm.Close();   
                 //m_SemBitAlarm.Close();

                 //m_SemNoAlarm.Dispose();
                 //m_SemLowBatteryAlarm.Dispose();
                 //m_SemDepleteAlarm.Dispose();
                 //m_SemBitAlarm.Dispose();

                 //m_SemNoAlarm = null;
                 //m_SemLowBatteryAlarm = null;
                 //m_SemDepleteAlarm = null;
                 //m_SemBitAlarm = null;
                 
                //for (int iLoop = 0; iLoop < m_ClientList.Count; iLoop++)
                //{ 
                //    if (m_ClientList[iLoop] != null)
                //    { 
                //        m_ClientList[iLoop].Close();
                //        m_ClientList[iLoop] = null;
                //    }
                //}
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //m_ClientList.Clear();
            }
        }

        /// <summary>
        /// 初始化4个信号量
        /// </summary>
        private void Init()
        {
            //create a semaphore
            bool bCreateNew = false;
            String semNoAlarmName = SEMNOALARMNAME + DateTime.Now.Ticks.ToString();           //to be different in threads
            m_SemNoAlarm = new Semaphore(0, m_SemMaxCount, semNoAlarmName, out bCreateNew);
            //if semaphore already existed,then open it.
            if (!bCreateNew)
            {
                m_SemNoAlarm = Semaphore.OpenExisting(semNoAlarmName);
            }

            String semLowBatteryAlarmName = SEMLOWBATTERYALARMNAME + DateTime.Now.Ticks.ToString();           //to be different in threads
            m_SemLowBatteryAlarm = new Semaphore(0, m_SemMaxCount, semLowBatteryAlarmName, out bCreateNew);
            //if semaphore already existed,then open it.
            if (!bCreateNew)
            {
                m_SemLowBatteryAlarm = Semaphore.OpenExisting(semLowBatteryAlarmName);
            }

            String semDepleteAlarmName = SEMDEPLETEALARMNAME + DateTime.Now.Ticks.ToString();           //to be different in threads
            m_SemDepleteAlarm = new Semaphore(0, m_SemMaxCount, semDepleteAlarmName, out bCreateNew);
            //if semaphore already existed,then open it.
            if (!bCreateNew)
            {
                m_SemDepleteAlarm = Semaphore.OpenExisting(semDepleteAlarmName);
            }

            String semBitAlarmName = SEMBITALARMNAME + DateTime.Now.Ticks.ToString();           //to be different in threads
            m_SemBitAlarm = new Semaphore(0, m_SemMaxCount, semBitAlarmName, out bCreateNew);
            //if semaphore already existed,then open it.
            if (!bCreateNew)
            {
                m_SemBitAlarm = Semaphore.OpenExisting(semBitAlarmName);
            }
        }

        private void StartTimer()
        {
            StopTimer();
            m_SendAlarmTimer.Start();
        }

        private void StopTimer()
        {
            m_SendAlarmTimer.Stop();
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mumCreateLink_Click(object sender, EventArgs e)
        {
           // if(IsConnected())
            if(m_Client!=null && m_Client.IsConnected())
                return;
            ConfigForm cfg = new ConfigForm();
            if(DialogResult.OK == cfg.ShowDialog())
            {
                m_SerialNo = cfg.SerialNo;//序列号传入
                m_ReceivedBuffer.Clear();
                m_Client = new AsyncClient(cfg.IP, cfg.Port);
                m_Client.HandleReceivedBuffers += OnReceivedBuffers;
                m_Client.Connect();
                SetClient();
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mumCloseLink_Click(object sender, EventArgs e)
        {
            try 
            { 
                if (m_Client != null && m_Client.IsConnected())
                    m_Client.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnReceivedBuffers(object sender, MessageEventArgs e)
        {
            lock(m_ReceivedBuffer)
            {
                if (e.Buffer != null)
                {
                    m_ReceivedBuffer.Clear();
                    m_ReceivedBuffer.AddRange(e.Buffer);
                    List<byte> hexBuffer = Common.Char2Hex(m_ReceivedBuffer);
                    HandleCommand(hexBuffer, m_Client);
                }
            }
        }

        private void OnReceivedBuffersEx(object sender, MessageEventArgsEx e)
        {
            lock(m_ClientList)
            {
                int index = m_ClientList.FindIndex((x)=>{return x.LocalPort==e.Client.LocalPort;});
                if (e.Buffer != null)
                {
                    m_ReceivedBufferList[index].Clear();
                    m_ReceivedBufferList[index].AddRange(e.Buffer);
                    List<byte> hexBuffer = Common.Char2Hex(m_ReceivedBufferList[index]);
                    HandleCommand(hexBuffer, m_ClientList[index]);
                }
            }
        }
     

        private void HandleCommand(List<byte> hexBuffer, AsyncClient client= null)
        {
            if (hexBuffer!=null && hexBuffer.Count >= 6)
            {
                if (hexBuffer[0] == 0x00 && hexBuffer[1] == 0x02 && hexBuffer[3] == 0x03)//应该是泵类型命令
                {
                    #region
                    //m_Client.s
                    CmdSendPumpType cmdPump = new CmdSendPumpType();
                    cmdPump.Direction = 1;
                    //cmdPump.PumpState = PumpStatus.Run;
                    List<byte> debugBytes = cmdPump.GetBytesDebug();
                    int iCount = debugBytes.Count-4;
                    if (pumpTypeResponseFlag == 1)//全部启动
                    {
                        debugBytes[iCount - 1] = 0x02;
                        debugBytes[iCount - 2] = 0x02;
                        debugBytes[iCount - 3] = 0x02;
                        debugBytes[iCount - 4] = 0x02;
                        debugBytes[iCount - 5] = 0x02;
                        debugBytes[iCount - 6] = 0x02;
                    }
                    else if(pumpTypeResponseFlag ==2)//全部停止
                    {
                        debugBytes[iCount - 1] = 0x01;
                        debugBytes[iCount - 2] = 0x01;
                        debugBytes[iCount - 3] = 0x01;
                        debugBytes[iCount - 4] = 0x01;
                        debugBytes[iCount - 5] = 0x01;
                        debugBytes[iCount - 6] = 0x01;
                    }

                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                    #endregion
                }
                else if(hexBuffer[0] == 0x00 && hexBuffer[1] == 0x03 && hexBuffer[3] == 0x09)//应该是老化开始命令
                {
                    //m_Client.s
                    CmdCharge cmdcharge = new CmdCharge();
                    cmdcharge.Direction = 1;
                    List<byte> debugBytes = cmdcharge.GetBytesDebug();
                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                }
                else if (hexBuffer[0] == 0x00 && hexBuffer[1] == 0x04) //放电命令
                {
                    CmdDischarge cmd = new CmdDischarge();
                    cmd.Direction = 1;
                    List<byte> debugBytes = cmd.GetBytesDebug();
                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                }
                else if (hexBuffer[0] == 0x00 && hexBuffer[1] == 0x05) //补电命令
                {
                    CmdRecharge cmd = new CmdRecharge();
                    cmd.Direction = 1;
                    List<byte> debugBytes = cmd.GetBytesEx();
                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                }
            }
          
        }

      

        /// <summary>
        /// 发送几个假的报警信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResponsePumpType_Click(object sender, EventArgs e)
        {
            List<byte> debugBytes = CreatePumpAlarmPackage(6);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }


        private List<byte> CreateSinglePumpAlarm(byte chanel)
        {
            List<byte> single = new List<byte>();

            single.Add(chanel);
            //泵电源状态
            single.Add(0x55);
            single.Add(0xAA);
            single.Add(0x05);
            single.Add(0x00);
            single.Add(0x00);
            single.Add(0x58);
            single.Add(0x01);
            single.Add(0x00);
            single.Add(0xFF);
            //报警
            single.Add(0x55);
            single.Add(0xAA);
            single.Add(0x05);
            single.Add(0x03);
            single.Add(0x00);
            single.Add(0x57);
            single.Add(0x04);
            single.Add(0xFF);
            single.Add(0xFF);
            single.Add(0xFF);
            single.Add(0xFF);
            single.Add(0xEE);
            return single;
        }

        private List<byte> CreateSinglePumpAlarm(byte chanel, uint alarm)
        {
            List<byte> single = new List<byte>();

            single.Add(chanel);
            //泵电源状态
            single.Add(0x55);
            single.Add(0xAA);
            single.Add(0x05);
            single.Add(0x00);
            single.Add(0x00);
            single.Add(0x58);
            single.Add(0x01);
            single.Add(0x00);
            single.Add(0xFF);
            //报警
            single.Add(0x55);
            single.Add(0xAA);
            single.Add(0x05);
            single.Add(0x03);
            single.Add(0x00);
            single.Add(0x57);
            single.Add(0x04);


            single.Add((byte)( alarm & 0xFF ));
            single.Add((byte)( alarm>>8 & 0xFF));
            single.Add((byte)( alarm>>16 & 0xFF));
            single.Add((byte)( alarm>>24 & 0xFF));

            single.Add(0xEE);
            return single;
        }

        private List<byte> CreatePumpAlarmPackage(uint alarm, int pumpCount = 6)
        {
            List<byte> package = new List<byte>();
            package.Add(0x00);
            package.Add(0x07);
            package.Add(0x16);

            package.Add((byte)(pumpCount * 22));
            package.Add((byte)((byte)0xFF-(byte)(pumpCount * 22)));
            for(int i=0;i<pumpCount;i++)
                package.AddRange(CreateSinglePumpAlarm((byte)(i+1), alarm));
            //泵电源状态
            package.Add(0x02);
            package.Add(0x00);
            package.Add(0x00);
            package.Add(0xEE);
            return package;
        }

        private List<byte> CreatePumpAlarmPackageEx(List<int> pumpIndexs, uint alarm, int pumpCount = 6)
        {
            List<byte> package = new List<byte>();
            package.Add(0x00);
            package.Add(0x07);
            package.Add(0x16);

            package.Add((byte)(pumpCount * 22));
            package.Add((byte)((byte)0xFF - (byte)(pumpCount * 22)));
            for (int i = 0; i < pumpCount; i++)
            { 
                if(pumpIndexs.Contains(i+1))
                    package.AddRange(CreateSinglePumpAlarm((byte)(i + 1), alarm));
                else
                    package.AddRange(CreateSinglePumpAlarm((byte)(i + 1), 0));
            }
            //泵电源状态
            package.Add(0x02);
            package.Add(0x00);
            package.Add(0x00);
            package.Add(0xEE);
            return package;
        }
        
        
         private List<byte> CreatePumpAlarmPackage(int pumpCount = 6)
        {
            List<byte> package = new List<byte>();
            package.Add(0x00);
            package.Add(0x07);
            package.Add(0x16);

            package.Add((byte)(pumpCount * 22));
            package.Add((byte)((byte)0xFF-(byte)(pumpCount * 22)));
            for(int i=0;i<pumpCount;i++)
                package.AddRange(CreateSinglePumpAlarm((byte)(i+1)));
            //泵电源状态
            package.Add(0x02);
            package.Add(0x00);
            package.Add(0x00);
            package.Add(0xEE);
            return package;
        }


        private void btnBitAlarm_Click(object sender, EventArgs e)
        {
            //List<byte> debugBytes = CreatePumpAlarmPackage(counter, 6);
            //counter <<= 1;
            //for(int iLoop=0;iLoop<m_ClientList.Count;iLoop++)
            //    m_ClientList[iLoop].Send(Common.Hex2Char(debugBytes.ToArray()));
            int iCount = Convert.ToInt32(tbCount.Text);
            m_SemBitAlarm.Release(iCount);
        }

        private void btnHighAlarm_Click(object sender, EventArgs e)
        {
            List<byte> debugBytes = CreatePumpAlarmPackage(0xFFFF0000, 6);
            for(int iLoop=0;iLoop<m_ClientList.Count;iLoop++)
                m_ClientList[iLoop].Send(Common.Hex2Char(debugBytes.ToArray()));

        }

        private void btnLowAlarm_Click(object sender, EventArgs e)
        {
            List<byte> debugBytes = CreatePumpAlarmPackage(0x0000FFFF, 6);
            for(int iLoop=0;iLoop<m_ClientList.Count;iLoop++)
                m_ClientList[iLoop].Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        private void btnLowVolC6_Click(object sender, EventArgs e)
        {
            //List<byte> debugBytes = CreatePumpAlarmPackage(0x00000100, 6);
            //for(int iLoop=0;iLoop<m_ClientList.Count;iLoop++)
            //    m_ClientList[iLoop].Send(Common.Hex2Char(debugBytes.ToArray()));
            int iCount = Convert.ToInt32(tbCount.Text);
            m_SemLowBatteryAlarm.Release(iCount);
        }

        private void btnDelepeC6_Click(object sender, EventArgs e)
        {
            //List<byte> debugBytes = CreatePumpAlarmPackage(0x00000008, 6);
            //for(int iLoop=0;iLoop<m_ClientList.Count;iLoop++)
            //    m_ClientList[iLoop].Send(Common.Hex2Char(debugBytes.ToArray()));
            int iCount = Convert.ToInt32(tbCount.Text);
            m_SemDepleteAlarm.Release(iCount);

        }

        private void btnNoAlarm_Click(object sender, EventArgs e)
        {
            //List<byte> debugBytes = CreatePumpAlarmPackage(0x0, 6);
            //for(int iLoop=0;iLoop<m_ClientList.Count;iLoop++)
            //    m_ClientList[iLoop].Send(Common.Hex2Char(debugBytes.ToArray()));
            int iCount = Convert.ToInt32(tbCount.Text);
            m_SemNoAlarm.Release(iCount);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            m_ThreadPool.Clear();
            int iCount = Convert.ToInt32(tbCount.Text);
            int basePort = 30000;
            string szIp = "";
            for(int iLoop=0;iLoop<iCount;iLoop++)
            {
                szIp = string.Format("127.0.0.{0}", iLoop+1);
                AsyncClient client = new AsyncClient(szIp, basePort++, tbIPAddress.Text, Convert.ToInt32(tbPort.Text));
                m_ClientList.Add(client);
                m_ReceivedBufferList.Add(new List<byte>());
                client.HandleReceivedBuffersEx += OnReceivedBuffersEx;
                client.Connect();
                Thread th = new Thread(new ParameterizedThreadStart(ProcSendCommand));
                m_ThreadPool.Add(th);
                th.Start(client);
            }
           
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            m_bKeeping = false;
            for(int j = 0;j<m_ThreadPool.Count;j++)
            {
                m_ThreadPool[j].Abort();
            }
            try 
            { 
                for(int iLoop=0;iLoop<m_ClientList.Count;iLoop++)
                    if (m_ClientList[iLoop] != null)
                        m_ClientList[iLoop].Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                m_ClientList.Clear();
            }
        }

        private void ProcSendCommand(object obj)
        {
            if (obj==null)
                return;
            while(m_bKeeping)
            {
                AsyncClient client = obj as AsyncClient;
                //如果没有连接
                if(!client.IsConnected())
                {
                    continue;
                }
                if(m_SemNoAlarm.WaitOne(1000))
                {
                    //如果有无报警出现，则发送
                    List<byte> debugBytes = CreatePumpAlarmPackage(0x0, 6);
                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                    //发送完成后要睡眠5秒，不能抢了别人的奶酪
                    Thread.Sleep(5000);
                }
                if (m_SemLowBatteryAlarm.WaitOne(1000))
                {
                    //如果有无报警出现，则发送
                    List<byte> debugBytes = CreatePumpAlarmPackage(0x00000100, 6);
                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                    //发送完成后要睡眠5秒，不能抢了别人的奶酪
                    Thread.Sleep(5000);
                }
                if (m_SemDepleteAlarm.WaitOne(1000))
                {
                    //如果有无报警出现，则发送
                    List<byte> debugBytes = CreatePumpAlarmPackage(0x00000008, 6);
                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                    //发送完成后要睡眠5秒，不能抢了别人的奶酪
                    Thread.Sleep(5000);
                }
                if (m_SemBitAlarm.WaitOne(1000))
                {
                    //如果有无报警出现，则发送
                    List<byte> debugBytes = CreatePumpAlarmPackage(counter, 6);
                    counter <<= 1;
                    client.Send(Common.Hex2Char(debugBytes.ToArray()));
                    //发送完成后要睡眠5秒，不能抢了别人的奶酪
                    Thread.Sleep(5000);
                }
                Thread.Sleep(200);
            }
        }

        private void btnCloseSingle_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_Client != null && m_Client.IsConnected())
                    m_Client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateSingle_Click(object sender, EventArgs e)
        {
            // if(IsConnected())
            if (m_Client != null && m_Client.IsConnected())
                return;
            m_ReceivedBuffer.Clear();
            m_Client =  new AsyncClient(tbLocalIP.Text, Convert.ToInt32(tbLocalport.Text), tbIPAddress.Text, 20160);
            m_Client.HandleReceivedBuffers += OnReceivedBuffers;
            m_Client.Connect();
            SetClient();
        }

        private void btnDelepeC6Single_Click(object sender, EventArgs e)
        {
            m_SendFlag = 3;
            StartTimer();
        }

        private void btnLowVolC6Single_Click(object sender, EventArgs e)
        {
            m_SendFlag = 2;
            StartTimer();
        }

        private void btnLowAlarmSingle_Click(object sender, EventArgs e)
        {
            m_SendFlag = 1;
            StartTimer();
        }

        private void SendNoAlarmSingle()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DeleSendBytes(SendNoAlarmSingle), new object[] { });
            }
            else
            {
                List<int> pumpIndexs = new List<int>();
                int iCount = pnlCheckBoxs.Controls.Count;
                foreach (CheckBox col in pnlCheckBoxs.Controls)
                {
                    if (col.Checked)
                        pumpIndexs.Add(Convert.ToInt32(col.Text));
                }
                //int pumpCount = 6;
                int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
                List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000000, pumpCount);
                m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
            }
        }

        /// <summary>
        /// 单个低位报警，可能是系统出错
        /// </summary>
        private void SendLowAlarmSingle()
        {
            List<int> pumpIndexs = new List<int>();
            int iCount = pnlCheckBoxs.Controls.Count;
            foreach (CheckBox col in pnlCheckBoxs.Controls)
            {
                if (col.Checked)
                    pumpIndexs.Add(Convert.ToInt32(col.Text));
            }
            int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
            List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000001, pumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// F6低电
        /// </summary>
        private void SendLowAlarmSingleF6()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DeleSendBytes(SendLowAlarmSingleF6), new object[] { });
            }
            else
            {
                List<int> pumpIndexs = new List<int>();
                int iCount = pnlCheckBoxs.Controls.Count;
                foreach (CheckBox col in pnlCheckBoxs.Controls)
                {
                    if (col.Checked)
                        pumpIndexs.Add(Convert.ToInt32(col.Text));
                }
                int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
                List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000001, pumpCount);
                m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
            }
        }

        /// <summary>
        /// 单个低电压报警
        /// </summary>
        private void SendLowVolSingle()
        {
            List<int> pumpIndexs = new List<int>();
            int iCount = pnlCheckBoxs.Controls.Count;
            foreach (CheckBox col in pnlCheckBoxs.Controls)
            {
                if (col.Checked)
                    pumpIndexs.Add(Convert.ToInt32(col.Text));
            }
            //如果有无报警出现，则发送
            int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
            List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000100, pumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 单个耗尽报警
        /// </summary>
        private void SendDelepeSingle()
        {
            List<int> pumpIndexs = new List<int>();
            int iCount = pnlCheckBoxs.Controls.Count;
            foreach (CheckBox col in pnlCheckBoxs.Controls)
            {
                if (col.Checked)
                    pumpIndexs.Add(Convert.ToInt32(col.Text));
            }
            //如果有无报警出现，则发送
            int pumpCount =  Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
            List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000008, pumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        private void chSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach(CheckBox box in pnlCheckBoxs.Controls)
            {
                box.Checked = chSelectAll.Checked;
            }
        }

        private void btnNoAlarmC6Single_Click(object sender, EventArgs e)
        {
            m_SendFlag = 0;
            StartTimer();
        }

        #region  C8 报警

        /// <summary>
        /// 单个低位报警，可能是系统出错
        /// </summary>
        private void SendLowAlarmSingleC8()
        {
            List<int> pumpIndexs = new List<int>();
            int iCount = pnlCheckBoxs.Controls.Count;
            foreach (CheckBox col in pnlCheckBoxs.Controls)
            {
                if (col.Checked)
                    pumpIndexs.Add(Convert.ToInt32(col.Text));
            }
            int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
            List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000002, pumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }


        private void SendLowVolSingleF6()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DeleSendBytes(SendLowVolSingleF6), new object[] { });
            }
            else
            {
                List<int> pumpIndexs = new List<int>();
                int iCount = pnlCheckBoxs.Controls.Count;
                foreach (CheckBox col in pnlCheckBoxs.Controls)
                {
                    if (col.Checked)
                        pumpIndexs.Add(Convert.ToInt32(col.Text));
                }
                //如果有无报警出现，则发送
                int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
                List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000100, pumpCount);
                m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
            }
        }

        /// <summary>
        /// 单个低电压报警
        /// </summary>
        private void SendLowVolSingleC8()
        {
            List<int> pumpIndexs = new List<int>();
            int iCount = pnlCheckBoxs.Controls.Count;
            foreach (CheckBox col in pnlCheckBoxs.Controls)
            {
                if (col.Checked)
                    pumpIndexs.Add(Convert.ToInt32(col.Text));
            }
            //如果有无报警出现，则发送
            int pumpCount = 6; // Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
            List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000001, pumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 单个耗尽报警
        /// </summary>
        private void SendDelepeSingleC8()
        {
            List<int> pumpIndexs = new List<int>();
            int iCount = pnlCheckBoxs.Controls.Count;
            foreach (CheckBox col in pnlCheckBoxs.Controls)
            {
                if (col.Checked)
                    pumpIndexs.Add(Convert.ToInt32(col.Text));
            }
            //如果有无报警出现，则发送
            int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
            List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00010000, pumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        private void SendDelepeSingleF6()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DeleSendBytes(SendDelepeSingleF6), new object[] { });
            }
            else
            {
                List<int> pumpIndexs = new List<int>();
                int iCount = pnlCheckBoxs.Controls.Count;
                foreach (CheckBox col in pnlCheckBoxs.Controls)
                {
                    if (col.Checked)
                        pumpIndexs.Add(Convert.ToInt32(col.Text));
                }
                //如果有无报警出现，则发送
                int pumpCount = Convert.ToInt32(cbPumpCount.Items[cbPumpCount.SelectedIndex].ToString());
                List<byte> debugBytes = CreatePumpAlarmPackageEx(pumpIndexs, 0x00000008, pumpCount);
                m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
            }
        }

        

        private void btnNoAlarmC8Single_Click(object sender, EventArgs e)
        {
            //m_ProductID=8;
            m_SendFlag = 0;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnC8LowAlarmSingle_Click(object sender, EventArgs e)
        {
            //m_ProductID=8;
            m_SendFlag = 1;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnLowVolC8Single_Click(object sender, EventArgs e)
        {
            //m_ProductID=8;
            m_SendFlag = 2;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnDelepeC8Single_Click(object sender, EventArgs e)
        {
            //m_ProductID=8;
            m_SendFlag = 3;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

       
        #endregion

        private void btnResponsePumpType_Click_1(object sender, EventArgs e)
        {
            pumpTypeResponseFlag = 1;
        }

        private void btnPartPumpType_Click(object sender, EventArgs e)
        {

        }

        private void btnStopAll_Click(object sender, EventArgs e)
        {
            pumpTypeResponseFlag = 2;
        }

        private void btnF6NoAlarmC6Single_Click(object sender, EventArgs e)
        {
            //m_ProductID = 66;
            m_SendFlag = 0;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF6LowAlarmSingle_Click(object sender, EventArgs e)
        {
            //m_ProductID = 66      ;
            m_SendFlag = 1;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF6LowVolC6Single_Click(object sender, EventArgs e)
        {
            //m_ProductID = 66;
            m_SendFlag = 2;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF6DelepeC6Single_Click(object sender, EventArgs e)
        {
            //m_ProductID = 66;
            m_SendFlag = 3;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF6ResponsePumpType_Click(object sender, EventArgs e)
        {
            pumpTypeResponseFlag = 1;
        }

        private void btnF6StopAll_Click(object sender, EventArgs e)
        {
            pumpTypeResponseFlag = 2;
        }

        /// <summary>
        /// 设置泵类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetPump_Click(object sender, EventArgs e)
        {
            if(Enum.IsDefined(typeof(ProductID),cbPumpType.Items[cbPumpType.SelectedIndex].ToString()))
            {
                m_ProductID = (ProductID)Enum.Parse(typeof(ProductID), cbPumpType.Items[cbPumpType.SelectedIndex].ToString());
                SetProductID(m_ProductID);
            }
        }

        #region F8

        private void btnF8LowAlarmSingle_Click(object sender, EventArgs e)
        {
            m_SendFlag = 1;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF8LowVolC8Single_Click(object sender, EventArgs e)
        {
            m_SendFlag = 2;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF8DelepeC8Single_Click(object sender, EventArgs e)
        {
            m_SendFlag = 3;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF8NoAlarmC8Single_Click(object sender, EventArgs e)
        {
            m_SendFlag = 0;
            this.m_Pump.SendFlag = m_SendFlag;
            StartTimer();
        }

        private void btnF8ResponsePumpType_Click(object sender, EventArgs e)
        {
            pumpTypeResponseFlag = 1;
        }

        private void btnF8StopAll_Click(object sender, EventArgs e)
        {
            pumpTypeResponseFlag = 2;
        }
        #endregion

    }
}
