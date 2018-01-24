using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncSocket;
using Cmd;

namespace WifiSimulator
{
    public class Graseby
    {
        public AsyncClient m_Client = null;
        public List<int> m_PumpIndexs = new List<int>();
        public int m_PumpCount = 6;
        public int m_SendFlag = 0;

        /// <summary>
        /// 带有报警位的泵ID
        /// </summary>
        public List<int> PumpIndexs
        {
            get { return m_PumpIndexs; }
            set { m_PumpIndexs = value; }
        }

        /// <summary>
        /// 上报的泵总量
        /// </summary>
        public int PumpCount
        {
            get { return m_PumpCount; }
            set { m_PumpCount = value; }
        }

        /// <summary>
        /// 发送报警类型 1,低位，2低电，3耗尽  
        /// </summary>
        public int SendFlag
        {
            get { return m_SendFlag; }
            set { m_SendFlag = value; }
        }


        public Graseby(AsyncClient client)
        {
            m_Client = client;
        }

        public List<byte> CreateSinglePumpAlarm(byte chanel)
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

        /// <summary>
        /// 一个泵的报警
        /// </summary>
        /// <param name="chanel"></param>
        /// <param name="alarm"></param>
        /// <returns></returns>
        public List<byte> CreateSinglePumpAlarm(byte chanel, uint alarm)
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


            single.Add((byte)(alarm & 0xFF));
            single.Add((byte)(alarm >> 8 & 0xFF));
            single.Add((byte)(alarm >> 16 & 0xFF));
            single.Add((byte)(alarm >> 24 & 0xFF));

            single.Add(0xEE);
            return single;
        }

        public List<byte> CreatePumpAlarmPackage(uint alarm, int pumpCount = 6)
        {
            List<byte> package = new List<byte>();
            package.Add(0x00);
            package.Add(0x07);
            package.Add(0x16);
            //数据长度
            package.Add((byte)(pumpCount * 22));
            package.Add(0x00);

            //数据长度取反
            package.Add((byte)((byte)0xFF - (byte)(pumpCount * 22)));
            package.Add(0xFF);

            for (int i = 0; i < pumpCount; i++)
                package.AddRange(CreateSinglePumpAlarm((byte)(i + 1), alarm));
            //泵电源状态
            package.Add(0x02);
            package.Add(0x00);
            package.Add(0x00);
            package.Add(0xEE);
            return package;
        }

        /// <summary>
        /// 构建报警的泵数据
        /// </summary>
        /// <param name="pumpIndexs">带有报警的泵ID，其余的没有报警</param>
        /// <param name="alarm">具体是哪项报警</param>
        /// <param name="pumpCount">上报泵的总数</param>
        /// <returns></returns>
        public List<byte> CreatePumpAlarmPackageEx(List<int> pumpIndexs, uint alarm, int pumpCount = 6)
        {
            List<byte> package = new List<byte>();
            package.Add(0x00);
            package.Add(0x07);
            package.Add(0x16);

            //数据长度
            package.Add((byte)(pumpCount * 22));
            package.Add(0x00);

            //数据长度取反
            package.Add((byte)((byte)0xFF - (byte)(pumpCount * 22)));
            package.Add(0xFF);

            for (int i = 0; i < pumpCount; i++)
            {
                if (pumpIndexs.Contains(i + 1))
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

        public List<byte> CreatePumpAlarmPackage(int pumpCount = 6)
        {
            List<byte> package = new List<byte>();
            package.Add(0x00);
            package.Add(0x07);
            package.Add(0x16);

            //数据长度
            package.Add((byte)(pumpCount * 22));
            package.Add(0x00);

            //数据长度取反
            package.Add((byte)((byte)0xFF - (byte)(pumpCount * 22)));
            package.Add(0xFF);

            for (int i = 0; i < pumpCount; i++)
                package.AddRange(CreateSinglePumpAlarm((byte)(i + 1)));
            //泵电源状态
            package.Add(0x02);
            package.Add(0x00);
            package.Add(0x00);
            package.Add(0xEE);
            return package;
        }

        /// <summary>
        /// 构建F8双道泵报警的泵数据
        /// </summary>
        /// <param name="pumpIndexs">带有报警的泵ID，其余的没有报警</param>
        /// <param name="alarm">具体是哪项报警</param>
        /// <param name="pumpCount">上报泵的总数</param>
        /// <returns></returns>
        public List<byte> CreateF8AlarmPackage(List<int> pumpIndexs, uint alarm, int pumpCount = 12)
        {
            List<byte> package = new List<byte>();
            package.Add(0x00);
            package.Add(0x07);
            package.Add(0x16);//一个泵数据22字节

            //数据长度
            ushort packageLength = (ushort)(pumpCount * 22);
            package.Add((byte)(packageLength & 0x00FF));
            package.Add((byte)(packageLength >> 8 & 0x00FF));

            ushort packageReverseLength = (ushort)(0xFFFF - packageLength);

            //数据长度取反
            package.Add((byte)(packageReverseLength & 0x00FF));
            package.Add((byte)(packageReverseLength >> 8 & 0x00FF));
            byte channel = 1;

            for (int i = 0; i < pumpCount; i++)
            {
                if (i % 2==0)
                {
                    channel |= 0x10;//第一道泵高4位为1
                }
                else
                {
                    channel |= 0x20;//第二道泵高4位为2
                }

                //if (pumpIndexs.Contains(i + 1))
                package.AddRange(CreateSinglePumpAlarm(channel, alarm));
                //else
                //    package.AddRange(CreateSinglePumpAlarm(channel, 0));
                channel &= 0x0F;

                if (i % 2 == 1)
                {
                    channel += 1;
                }
            }
            //泵电源状态
            package.Add(0x02);
            package.Add(0x00);
            package.Add(0x00);
            package.Add(0xEE);
            return package;
        }

        /// <summary>
        /// 发送报警
        /// </summary>
        public virtual void SendAlarm()
        {
            switch (m_SendFlag)
            {
                case 0:
                    SendNoAlarm();
                    break;
                case 1:
                    SendLowAlarm();
                    break;
                case 2:
                    SendLowVoltage();
                    break;
                case 3:
                    SendDeplete();
                    break;
                default: 
                    SendNoAlarm();
                    break;
            }
        }

        /// <summary>
        /// 无报警
        /// </summary>
        public virtual void SendNoAlarm()
        {

        }

        /// <summary>
        /// 低位报警
        /// </summary>
        public virtual void SendLowAlarm()
        {

        }

        /// <summary>
        /// 低电压报警
        /// </summary>
        public virtual void SendLowVoltage()
        {

        }

        /// <summary>
        /// 耗尽
        /// </summary>
        public virtual void SendDeplete()
        {

        }

    }
}
