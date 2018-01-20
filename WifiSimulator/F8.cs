using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncSocket;
using Cmd;

namespace WifiSimulator
{
    public class F8 : Graseby
    {

        public F8(AsyncClient client)
            : base(client)
        {

        }


        public override void SendAlarm()
        {
            base.SendAlarm();
        }

        /// <summary>
        /// 无报警
        /// </summary>
        public override void SendNoAlarm()
        {
            List<byte> debugBytes = CreateF8AlarmPackage(m_PumpIndexs, 0x00000000, m_PumpCount*2);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 低位报警
        /// </summary>
        public override void SendLowAlarm()
        {
            List<byte> debugBytes = CreateF8AlarmPackage(m_PumpIndexs, 0x00000002, m_PumpCount*2);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 低电压报警
        /// </summary>
        public override void SendLowVoltage()
        {
            List<byte> debugBytes = CreateF8AlarmPackage(m_PumpIndexs, 0x00000001, m_PumpCount*2);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 耗尽
        /// </summary>
        public override void SendDeplete()
        {
            List<byte> debugBytes = CreateF8AlarmPackage(m_PumpIndexs, 0x00010000, m_PumpCount*2);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }
    }
}
