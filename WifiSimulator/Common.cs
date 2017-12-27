using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WifiSimulator
{
    public class Common
    {
        /// <summary>
        /// 接收到的字符转成
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static List<byte> Char2Hex(List<byte> buffer)
        {
            List<byte> charBuffer = buffer;
            List<byte> hexBuffer = new List<byte>();
            int headIndex = charBuffer.IndexOf(0x02);
            int tailIndex = charBuffer.IndexOf(0x03);
            //0x02和0x03之间的数据必须是偶数个的
            if (headIndex < 0 || tailIndex < 0)
            {
                Logger.Instance().ErrorFormat("Char2Hex 错误，长度不足 headIndex={0}, tailIndex={1}", headIndex, tailIndex);
                return null;
            }
            if (tailIndex - headIndex < 10 || (tailIndex - headIndex - 1) % 2 != 0)
            {
                charBuffer.RemoveRange(headIndex, tailIndex - headIndex + 1);
                return null;
            }
            byte[] temp = new byte[tailIndex - headIndex - 1];
            charBuffer.CopyTo(headIndex + 1, temp, 0, temp.Length);
            charBuffer.RemoveRange(headIndex, tailIndex - headIndex + 1);
            int length = temp.Length;
            int iLoop = 0;
            byte byteHigh = 0x00;
            byte byteLow = 0x00;
            while (iLoop + 1 < length)
            {
                if (temp[iLoop] >= 0x30 && temp[iLoop] <= 0x39)
                    byteHigh = (byte)((temp[iLoop] - 0x30) << 4);
                else if (temp[iLoop] >= 0x41 && temp[iLoop] <= 0x46)
                    byteHigh = (byte)((temp[iLoop] - 0x37) << 4);
                else if (temp[iLoop] >= 0x61 && temp[iLoop] <= 0x66)
                    byteHigh = (byte)((temp[iLoop] - 0x57) << 4);
                else
                {
                    Logger.Instance().ErrorFormat("Char2Hex 错误，出现0~9，A~F以外的字符 temp[iLoop]={0}， iLoop={1}", temp[iLoop], iLoop);
                    return null;
                }

                if (temp[iLoop + 1] >= 0x30 && temp[iLoop + 1] <= 0x39)
                    byteLow = (byte)(temp[iLoop + 1] - 0x30);
                else if (temp[iLoop + 1] >= 0x41 && temp[iLoop + 1] <= 0x46)
                    byteLow = (byte)(temp[iLoop + 1] - 0x37);
                else if (temp[iLoop + 1] >= 0x61 && temp[iLoop + 1] <= 0x66)
                    byteLow = (byte)(temp[iLoop + 1] - 0x57);
                else
                {
                    Logger.Instance().ErrorFormat("Char2Hex 错误，出现0~9，A~F以外的字符 temp[iLoop]={0}， iLoop={1}", temp[iLoop], iLoop);
                    return null;
                }
                byteHigh &= 0xF0;
                byteLow &= 0x0F;
                hexBuffer.Add((byte)(byteHigh + byteLow));
                iLoop = iLoop + 2;
            }
            Logger.Instance().InfoFormat("Char2Hex() Char bytes={0}", BufferToString(temp));
            Logger.Instance().InfoFormat("Hex2Char() Raw bytes={0}", BufferToString(hexBuffer));
            return hexBuffer;
        }

        public static byte[] Hex2Char(byte[] buffer)
        {
            List<byte> charBuffer = new List<byte>();
            charBuffer.Add(0x02);
            //charBuffer[length-1] = 0x03;
            byte byteHigh = 0x00;
            byte byteLow = 0x00;
            for (int iLoop = 0; iLoop < buffer.Length; iLoop++)
            {
                byteHigh = (byte)(buffer[iLoop] >> 4 & 0x0F);
                byteLow = (byte)(buffer[iLoop] & 0x0F);

                if (byteHigh >= 0x00 && byteHigh <= 0x09)
                    charBuffer.Add((byte)(byteHigh + 0x30));
                else if (byteHigh >= 0x0A && byteHigh <= 0x0F)
                    charBuffer.Add((byte)(byteHigh + 0x37));
                else
                    break;

                if (byteLow >= 0x00 && byteLow <= 0x09)
                    charBuffer.Add((byte)(byteLow + 0x30));
                else if (byteLow >= 0x0A && byteLow <= 0x0F)
                    charBuffer.Add((byte)(byteLow + 0x37));
                else
                    break;
            }
            charBuffer.Add(0x03);
            Logger.Instance().InfoFormat("Hex2Char() Raw bytes={0}", BufferToString(buffer));
            Logger.Instance().InfoFormat("Hex2Char() Converted Char bytes={0}", BufferToString(charBuffer));
            return charBuffer.ToArray();
        }

        public static string BufferToString(List<byte> buffer)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));
                sb.Append(b.ToString(" "));
            }
            sb.Append("\r\n");
            return sb.ToString();
        }

        public static string BufferToString(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));
                sb.Append(b.ToString(" "));
            }
            sb.Append("\r\n");
            return sb.ToString();
        }
    }
}
