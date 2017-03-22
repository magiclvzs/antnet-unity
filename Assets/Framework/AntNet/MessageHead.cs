using UnityEngine; 
using System.IO;
using System.Text;
using System.Collections; 
using System.Collections.Generic;

namespace AntNet
{
    /// <summary>
    /// 消息协议头
    /// </summary>
    public class MessageHead
    {
        public class Flags
        {
           public static ushort  FlagEncrypt = 1 << 0;          //数据是经过加密的
           public static ushort FlagCompress = 1 << 1;          //数据是经过压缩的
           public static ushort FlagContinue = 1 << 2;          //有后续数据
           public static ushort FlagNeedAck = 1 << 3;           //数据需要确认
           public static ushort FlagAck = 1 << 4;               //确认包
           public static ushort FlagReSend = 1 << 5;            //数据是重发的
           public static ushort FlagServer = 1 << 6;            //数据来自服务器
        };

        public uint len = 0;                //数据长度
        public ushort error = 0;            //错误码
        public byte cmd = 0;                //命令
        public byte act = 0;                //动作
        public ushort index = 0;            //序号
        public ushort flags = 0;            //标记

        //超时处理 
        internal MessageHead nextTimeout = null;

        public static int Length            //获取协议头长度
        {
            get { return 12; }
        }

        public static int MaxRecvLength     //最大接收数据长度
        {
            get { return 1024 * 1024; }
        }

        public static int MaxSendLength     //最大发送数据长度
        {
            get { return 16384; }
        }
        public bool needAck
        {
            get { return (flags & Flags.FlagNeedAck) > 0; }
            set
            {
                if (value)
                {
                    flags |= Flags.FlagNeedAck;
                }
                else if(needAck)
                {
                    flags -= Flags.FlagNeedAck;
                }
            }
        }
        public int CmdAct                   //命令和活动联合key
        {
            get { return cmd * 1024 + act; }
        }
        /// <summary>
        /// 解析协议头
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>解析好的协议头</returns>
        public static MessageHead Parse(byte[] data)
        {
            MessageHead head = new MessageHead();
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            head.len = reader.ReadUInt32();
            head.error = reader.ReadUInt16();
            head.cmd = reader.ReadByte();
            head.act = reader.ReadByte();
            head.index = reader.ReadUInt16();
            head.flags = reader.ReadUInt16();

            return head;
        }
        /// <summary>
        /// 序列化协议头
        /// </summary>
        /// <returns>序列化后的数据</returns>
        public byte[] Serialize()
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(MessageHead.Length);
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);
            writer.Write(len);
            writer.Write(error);
            writer.Write(cmd);
            writer.Write(act);
            writer.Write(index);
            writer.Write(flags);
            return stream.ToArray();
        }
    }
    
}













