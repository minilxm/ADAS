using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cmd;

namespace Analyse
{
    public class ParameterManager
    {
        private static ParameterManager m_Manager = null;
        private List<AgingParameter> m_ParaList = null;

        public List<AgingParameter> ParaList
        {
            get {return m_ParaList;}
        }

        private ParameterManager()
        {
            m_ParaList = new List<AgingParameter>();
        }

        public static ParameterManager Instance()
        {
            if (m_Manager == null)
                m_Manager = new ParameterManager();
            return m_Manager;
        }

        public void Delete(string pumpType)
        {
            int index = m_ParaList.FindIndex((x) => { return x.PumpType == pumpType; });
            if (index >= 0)
                m_ParaList.RemoveAt(index);
        }

        public void Clear()
        {
            m_ParaList.Clear();
        }

        public AgingParameter Get(string pumpType)
        {
            return m_ParaList.Find((x) => { return x.PumpType == pumpType; });
        }

        public void Add(AgingParameter para)
        {
            m_ParaList.Add(para);
        }


    }

    public class AgingParameter
    {
        private string m_PumpType = string.Empty;   //此处定义的泵型号与实际的型号有差距，应该是CustomProductID对应的字符串
        private decimal            m_Rate;                      //速率可以是小数
        private decimal            m_Volume;                    
        private decimal            m_ChargeTime;                //老化充电时间可以是小数
        private decimal            m_DischargeTime;             //老化放电时间可以是小数
        private decimal            m_RechargeTime;              //老化补电时间可以是小数
        private OcclusionLevel     m_OcclusionLevel = OcclusionLevel.H;
        private C9OcclusionLevel   m_C9OcclusionLevel = C9OcclusionLevel.Level3;//C9的等级与其他泵不一样
        private CustomProductID    m_CustomPid = CustomProductID.Unknow;//由m_PumpType转换而来
         
        /// <summary>
        /// 泵型号
        /// </summary>
        public string PumpType
        {
            get { return m_PumpType; }
            set { m_PumpType = value; }
        }

        /// <summary>
        /// 泵型号
        /// </summary>
        public CustomProductID CID
        {
            get
            {
                if( m_CustomPid != CustomProductID.Unknow)
                {
                    return m_CustomPid;
                }
                else if (!string.IsNullOrEmpty(m_PumpType))
                {
                    m_CustomPid = ProductIDConvertor.Name2CustomProductID(m_PumpType);
                    return m_CustomPid;
                }
                else
                    return CustomProductID.Unknow;
            }
            set { m_CustomPid = value; }
        }

        /// <summary>
        /// 老化速率
        /// </summary>
        public decimal Rate
        {
            get { return m_Rate; }
            set { m_Rate = value; }
        }

        /// <summary>
        /// 限制量
        /// </summary>
        public decimal Volume
        {
            get { return m_Volume; }
            set { m_Volume = value; }
        }

        /// <summary>
        /// 充电时长
        /// </summary>
        public decimal ChargeTime
        {
            get { return m_ChargeTime; }
            set { m_ChargeTime = value; }
        }

        /// <summary>
        /// 放电时长
        /// </summary>
        public decimal DischargeTime
        {
            get { return m_DischargeTime; }
            set { m_DischargeTime = value; }
        }

        /// <summary>
        /// 补电时长
        /// </summary>
        public decimal RechargeTime
        {
            get { return m_RechargeTime; }
            set { m_RechargeTime = value; }
        }

        /// <summary>
        /// 压力档
        /// </summary>
        public OcclusionLevel OclusionLevel
        {
            get { return m_OcclusionLevel; }
            set { m_OcclusionLevel = value; }
        }

        /// <summary>
        /// C9压力档
        /// </summary>
        public C9OcclusionLevel C9OclusionLevel
        {
            get { return m_C9OcclusionLevel; }
            set { m_C9OcclusionLevel = value; }
        }
        
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pumpType"></param>
        /// <param name="rate"></param>
        /// <param name="volume"></param>
        /// <param name="chargeTime"></param>
        /// <param name="dischargeTime"></param>
        /// <param name="rechargeTime"></param>
        /// <param name="oclusionLevel"></param>
        /// <param name="c9OclusionLevel"></param>
        public AgingParameter(  string     pumpType,
                                decimal    rate,
                                decimal    volume,
                                decimal    chargeTime,
                                decimal    dischargeTime,
                                decimal    rechargeTime,
                                OcclusionLevel oclusionLevel,
                                C9OcclusionLevel c9OclusionLevel = C9OcclusionLevel.Level3
                              )
        {
           m_PumpType      = pumpType;
           m_Rate          = rate;
           m_Volume        = volume;
           m_ChargeTime    = chargeTime;
           m_DischargeTime = dischargeTime;
           m_RechargeTime  = rechargeTime;
           m_OcclusionLevel = oclusionLevel;
           m_C9OcclusionLevel = c9OclusionLevel;
        }

        /// <summary>
        /// 拷贝构造函数
        /// </summary>
        /// <param name="other"></param>
        public AgingParameter(AgingParameter other)
        {
            m_PumpType      = other.m_PumpType;
            m_Rate          = other.m_Rate;
            m_Volume        = other.m_Volume;
            m_ChargeTime    = other.m_ChargeTime;
            m_DischargeTime = other.m_DischargeTime;
            m_RechargeTime  = other.m_RechargeTime;
            m_OcclusionLevel = other.m_OcclusionLevel;
            m_C9OcclusionLevel = other.m_C9OcclusionLevel;
        }


    }


}
