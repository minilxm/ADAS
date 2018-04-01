using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AsyncSocket;
using Analyse;

namespace AgingSystem
{
    /// <summary>
    /// 提供佳土比1200系列泵补电后生新启动操作
    /// </summary>
    public class RebootPumpManager
    {
        private List<RebootPumpList> m_RebootPumpQueue = new List<RebootPumpList>();

        public List<RebootPumpList> RebootPumpQueue
        {
            get { return m_RebootPumpQueue; }
        }

        public RebootPumpManager()
        { }

         
        /// <summary>
        /// 有已经补电泵信息，更新到表中
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="channel"></param>
        public void UpdateRebootInfo(long ip, byte channel)
        {
            lock (m_RebootPumpQueue)
            {
                RebootPumpList pumpInfo = m_RebootPumpQueue.Find((x => { return x.ip == ip; }));
                if (pumpInfo != null)
                    pumpInfo.Update(ip, channel);
                else
                {
                    RebootPumpList pumps = new RebootPumpList(ip);
                    pumps.Update(ip, channel);
                    m_RebootPumpQueue.Add(pumps);
                }
            }
        }

        /// <summary>
        /// 清除所有泵
        /// </summary>
        public void Clear()
        {
            lock (m_RebootPumpQueue)
            {
                m_RebootPumpQueue.Clear();
            }
        }

        /// <summary>
        /// 移除一层泵
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="channel"></param>
        public void Remove(long ip)
        {
            lock (m_RebootPumpQueue)
            {
                m_RebootPumpQueue.RemoveAll((x => { return x.ip == ip; }));
            }
        }

        public RebootPumpList GetDepletePumpsByIP(long ip)
        {
            return m_RebootPumpQueue.Find((x) => { return x.ip == ip; });
        }
    }

    public class RebootPumpList
    {
        public long ip;
        public List<byte> channels = new List<byte>();

        public RebootPumpList()
        {
            ip = 0;
        }

        public RebootPumpList(long ip)
        {
            this.ip = ip;
        }

        public void Update(long ip, byte channel)
        {
            if (channels.Count==0)
                channels.Add(channel);
            else
            {
                if(!channels.Contains(channel))
                    channels.Add(channel);
            }
        }

        /// <summary>
        /// 同一个IP下面有多个同时已经补电泵，可以一起发送重启命令,此时需要生成通道编号
        /// </summary>
        /// <returns></returns>
        public byte GenChannel()
        {
            byte channel = 0;
            if (channels.Count==0)
            {
                return 0;
            }
            else
            {
                for(int iLoop = 0;iLoop<channels.Count;iLoop++)
                {
                    channel |= (byte)(1 << (channels[iLoop]-1));
                }
            }
            return channel;
        }

    }
}