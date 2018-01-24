using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net.Sockets;
using AsyncSocket;
using Cmd;

namespace Analyse
{
    public class SendOrReceiveBytesArgs : EventArgs
    {
        private bool       m_IsSend  = true;
        private List<byte> m_Buffer  = new List<byte>();

        /// <summary>
        /// 发送或接受的字节流
        /// </summary>
        public  List<byte> Buffer
        {
            get {return m_Buffer;}
            set {m_Buffer = value;}
        }

        public bool IsSend
        {
            get {return m_IsSend;}
            set {m_IsSend = value;}
        }

        public SendOrReceiveBytesArgs()
        {
        }

        public SendOrReceiveBytesArgs(List<byte> buffer, bool bSend=true)
        {
            byte []arrBuf = new byte[buffer.Count];
            buffer.CopyTo(arrBuf);
            m_Buffer.AddRange(arrBuf);
            m_IsSend = bSend;
        }

        public SendOrReceiveBytesArgs(byte[] buffer, bool bSend=true)
        {
            m_IsSend = bSend;
            m_Buffer.Clear();
            m_Buffer.AddRange(buffer);
        }

    }

    public class TimeOutArgs : EventArgs
    {
        private BaseCommand  m_Cmd = null;

        public BaseCommand Cmd
        {
            get { return m_Cmd; }
            set { m_Cmd = value; }
        }

        public TimeOutArgs()
        {
        }

        public TimeOutArgs(BaseCommand cmd)
        {
            m_Cmd = cmd;
        }

    }

    public class ParameterArgs : EventArgs
    {
        public string m_PumpType;
        public string m_Rate;
        public string m_Volume;
        public string m_ChargeTime;
        public string m_DischargeTime;
        public string m_RechargeTime;
        public string m_OccLevel;
         
        public ParameterArgs()
        {
        }

        public ParameterArgs(string pumpType,
                             string rate,
                             string volume,
                             string chargeTime,
                             //string dischargeTime,
                             string rechargeTime,
                             string occLevel
            )
        {
            m_PumpType      =       pumpType;
            m_Rate          =           rate;
            m_Volume        =         volume;
            m_ChargeTime    =     chargeTime;
            //m_DischargeTime =  dischargeTime;
            m_RechargeTime  =   rechargeTime;
            m_OccLevel      = occLevel;
        }

    }


    public class ConfigrationArgs : EventArgs
    {
        private Hashtable m_DockParameter = new Hashtable();    //存放每个货架的配置信息（int 货架号，DefaultParameter）
        public Hashtable DockParameter
        {
            get { return m_DockParameter;}
        }
        
        public ConfigrationArgs()
        {
        }

        public ConfigrationArgs(Hashtable dockParameter)
        {
           m_DockParameter = dockParameter;
        }
    }

    /// <summary>
    /// 连接到ADAS货架上的泵列表
    /// </summary>
    public class SelectedPumpsArgs : EventArgs
    {
        private Hashtable m_SelectedPumps = new Hashtable();    //存放每个货架的泵位置信息（int 货架号，List<Tuple<int,int,int,string>>(int pumpLocation,int rowNo,int colNo,serialNo）)
        public Hashtable SelectedPumps
        {
            get { return m_SelectedPumps; }
        }

        public SelectedPumpsArgs()
        {
        }

        public SelectedPumpsArgs(Hashtable selectedPumps)
        {
            m_SelectedPumps = selectedPumps;
        }
    }

    /// <summary>
    /// 新连接的SOCKET还是刚刚关闭的SOCKET
    /// </summary>
    public class SocketConnectArgs : EventArgs
    {
        private bool m_Connected = true;
        //private Socket m_ConnectedSocket;
        private AsyncSocketUserToken m_ConnectedSocket;

        public bool Connected
        {
            get { return m_Connected; }
        }

        //public Socket ConnectedSocket
        //{
        //    get { return m_ConnectedSocket; }
        //}
        public AsyncSocketUserToken ConnectedSocket
        {
            get { return m_ConnectedSocket; }
        }

        public SocketConnectArgs()
        {
        }

        //public SocketConnectArgs(bool connected, Socket connectedSocket)
        //{
        //    m_Connected = connected;
        //    m_ConnectedSocket = connectedSocket;
        //}
        public SocketConnectArgs(bool connected, AsyncSocketUserToken connectedSocket)
        {
            m_Connected = connected;
            m_ConnectedSocket = connectedSocket;
        }
    }


    public class SinglePumpArgs : EventArgs
    {
        private int  m_DockNO             = 0;           //货架编号
        private int  m_PumpLocation       = 0;           //泵位置
        private int  m_RowNo              = 0;           //从1开始
        private int  m_ColNo               = 0;          //从1 开始
        private bool m_EnableCheckBoxClick = false;      //是否允许点击事件，当双道泵时，选择第一道泵时，第二道泵默认勾上
        private string m_SerialNo = string.Empty;        //泵的序列号，通过条码枪获得

        /// <summary>
        /// 泵的序列号
        /// </summary>
        public string SerialNo
        {
            get { return m_SerialNo; }
            set { m_SerialNo = value; }
        }

        /// <summary>
        /// 货架编号
        /// </summary>
        public int DockNO
        {
            get { return m_DockNO; }
            set { m_DockNO = value; }
        }

        /// <summary>
        /// 所在货架行号
        /// </summary>
        public int RowNo
        {
            get { return m_RowNo; }
            set { m_RowNo = value; }
        }

        /// <summary>
        /// 所在货架列号，对应协议中的通道号
        /// </summary>
        public int ColNo
        {
            get { return m_ColNo; }
            set { m_ColNo = value; }
        }

        /// <summary>
        /// 泵位置
        /// </summary>
        public int PumpLocation
        {
            get { return m_PumpLocation; }
            set { m_PumpLocation = value; }
        }

        /// <summary>
        /// 是否允许点击事件
        /// </summary>
        public bool EnableCheckBoxClick
        {
            get { return m_EnableCheckBoxClick; }
            set { m_EnableCheckBoxClick = value; }
        }


        public SinglePumpArgs()
        {
        }

        public SinglePumpArgs(int dockNO,
                                int pumpLocation,
                                int rowNo,
                                int colNo,
                                bool enableCheckBoxClick,
                                string serialNo = "")
        {
            m_DockNO = dockNO;
            m_PumpLocation = pumpLocation;
            m_RowNo = rowNo;
            m_ColNo = colNo;
            m_EnableCheckBoxClick = enableCheckBoxClick;
            m_SerialNo = serialNo;
        }
    }

    /// <summary>
    /// 完成序列号的输入事件
    /// </summary>
    public class SerialNoInputArgs : EventArgs
    {
        private int m_PumpLocation = 0;           //泵位置
        private string m_SerialNo = string.Empty; //泵的序列号，通过条码枪获得

        /// <summary>
        /// 泵位置
        /// </summary>
        public int PumpLocation
        {
            get { return m_PumpLocation; }
            set { m_PumpLocation = value; }
        }

        public string SerialNo
        {
            get { return m_SerialNo; }
            set { m_SerialNo = value; }
        }

        public SerialNoInputArgs(int pumpLocation, string serialNo)
        {
            m_PumpLocation = pumpLocation;
            m_SerialNo = serialNo;
        }
    }
}
