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
using Analyse;

namespace  AgingSystem
{
    /// <summary>
    /// Interaction logic for SinglePump.xaml
    /// </summary>
    public partial class SinglePump : UserControl
    {
        public event EventHandler<SinglePumpArgs> OnClickCheckBox;

        private int  m_DockNO                   = 0;            //货架编号
        private int  m_PumpLocation             = 0;            //泵位置
        private int  m_RowNo                    = 0;            //从1开始
        private int  m_ColNo                    = 0;            //从1 开始
        private bool m_EnableCheckBoxClick      = false; //是否允许点击事件，当双道泵时，选择第一道泵时，第二道泵默认勾上

        /// <summary>
        /// 货架编号
        /// </summary>
        public int DockNO
        {
            get { return m_DockNO; }
            set { m_DockNO = value;}
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



        public SinglePump()
        {
            InitializeComponent();
        }

        public SinglePump(int dockNO)
        {
            InitializeComponent();
            m_DockNO = dockNO;
        }

        public SinglePump(int dockNO, int pumpLocation)
        {
            InitializeComponent();
            m_DockNO = dockNO;
            m_PumpLocation = pumpLocation;
        }

        public SinglePump(int dockNO,int pumpLocation,int rowNo,int colNo)
        {
            InitializeComponent();
            m_DockNO = dockNO;
            m_PumpLocation = pumpLocation;
            m_RowNo = rowNo;
            m_ColNo = colNo;
        }

        public void SetPump(int pumpLocation, string strPumpType, string strSerialNo = "")
        {
            chNo.Content = m_DockNO.ToString() + "-" + pumpLocation.ToString();
            lbPumpType.Content = strPumpType;
            lbSerialNo.Content = strSerialNo;
        }

        /// <summary>
        /// 当点击checkbox的时候触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if(m_EnableCheckBoxClick)
            {
                if(OnClickCheckBox!=null)
                {
                    OnClickCheckBox(this, new SinglePumpArgs(m_DockNO, m_PumpLocation, m_RowNo, m_ColNo, m_EnableCheckBoxClick));
                }
            }
        }

    }
}
