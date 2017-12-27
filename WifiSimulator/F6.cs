using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncSocket;
using Cmd;

namespace WifiSimulator
{
    public class F6 : Graseby
    {
        public F6(AsyncClient client)
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
            List<byte> debugBytes = CreatePumpAlarmPackageEx(m_PumpIndexs, 0x00000000, m_PumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 低位报警
        /// </summary>
        public override void SendLowAlarm()
        {
            List<byte> debugBytes = CreatePumpAlarmPackageEx(m_PumpIndexs, 0x00000001, m_PumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 低电压报警
        /// </summary>
        public override void SendLowVoltage()
        {
            List<byte> debugBytes = CreatePumpAlarmPackageEx(m_PumpIndexs, 0x00000100, m_PumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }

        /// <summary>
        /// 耗尽
        /// </summary>
        public override void SendDeplete()
        {
            List<byte> debugBytes = CreatePumpAlarmPackageEx(m_PumpIndexs, 0x00000008, m_PumpCount);
            m_Client.Send(Common.Hex2Char(debugBytes.ToArray()));
        }
    }
}
