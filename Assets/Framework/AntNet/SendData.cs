using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
namespace AntNet
{
    /// <summary>
    /// 发送数据
    /// </summary>
    public class SendData
    {
        public ushort error = Error.ErrOk;                  //错误码
        public MessageHead head = new MessageHead();                //消息头 

        internal float endTime = 0;                                 //超时时间
        protected static ushort _index = 1;                         //发送数据索引
        protected byte[] data = null;                               //发送的数据
        protected Func<byte[]> dataGenCallback = null;              //序列化回调

        protected bool isSerializeHead = false;

       

        /// <summary>
        /// 获取需要发送数据
        /// </summary>
        /// <returns>需要发送的数据</returns>
        public byte[] GetData()
        {
            if (dataGenCallback != null)
            {
                byte[] wdata = dataGenCallback();
                data = new byte[MessageHead.Length + wdata.Length];
                head.len = (uint)wdata.Length;
                wdata.CopyTo(data, MessageHead.Length);
                dataGenCallback = null;
            }

            if (!isSerializeHead)
            {
                isSerializeHead = true;
                head.Serialize().CopyTo(data, 0);
            }
            return data;
        }

        /// <summary>
        /// 发送数据，仅包括协议头，不带任何数据
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="act">活动</param>
        /// <param name="flags">标记</param>
        /// <param name="index">序号</param>
        public SendData(byte cmd, byte act, ushort flags, ushort index)
        {
            head.flags = flags;
            head.index = index;
            head.cmd = cmd;
            head.act = act;
            data = new byte[MessageHead.Length];
        }
        /// <summary>
        /// 发送多个确认消息
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="act">活动</param>
        /// <param name="ackList">需要确认的消息</param>
        public SendData(byte cmd, byte act, List<MessageHead> ackList)
        {
            data = new byte[MessageHead.Length * (ackList.Count + 1)];
            head.flags = (ushort)(MessageHead.Flags.FlagAck | MessageHead.Flags.FlagContinue);
            head.cmd = cmd;
            head.act = act;
            head.len = (uint)(MessageHead.Length * ackList.Count);

            int i = 1;
            foreach (var msgHead in ackList)
            {
                msgHead.Serialize().CopyTo(data, MessageHead.Length * i++);
            }
        }
        /// <summary>
        /// 发送数据，包括协议头和有效数据
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="act">活动</param>
        /// <param name="wdata">数据</param>
        /// <param name="timeout">超时</param>
        public SendData(byte cmd, byte act, byte[] wdata, float timeout = 0)
        {
            if (wdata.Length > MessageHead.MaxSendLength)
            {
                Log.Error("data size too looger length:{0}", wdata.Length);
            }
            data = new byte[MessageHead.Length + wdata.Length];
            head.len = (uint)wdata.Length;
            head.cmd = cmd;
            head.act = act;
            head.index = _index++;
            wdata.CopyTo(data, MessageHead.Length);
            if (timeout > 0)
            {
                this.endTime = Time.realtimeSinceStartup + timeout;
            }
        }
        /// <summary>
        /// 发送数据，包括协议头和有效数据
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="act">活动</param>
        /// <param name="str">数据</param>
        /// <param name="timeout">超时</param>
        public SendData(byte cmd, byte act, string str, float timeout = 0)
            : this(cmd, act, Encoding.Default.GetBytes(str), timeout)
        {
        }
        /// <summary>
        /// 发送数据，包括协议头和有效数据
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="act">活动</param>
        /// <param name="stream">数据</param>
        /// <param name="timeout">超时</param>
        public SendData(byte cmd, byte act, MemoryStream stream, float timeout = 0)
            : this(cmd, act, stream.ToArray(), timeout)
        {
        }
        /// <summary>
        /// 发送数据，包括协议头和产生有效数据的回调
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="act">活动</param>
        /// <param name="callback">回调</param>
        /// <param name="timeout">超时</param>
        public SendData(byte cmd, byte act, Func<byte[]> callback, float timeout = 0)
        {
            head.index = _index++;
            head.cmd = cmd;
            head.act = act;
            this.dataGenCallback = callback;
            if (timeout > 0)
            {
                this.endTime = Time.realtimeSinceStartup + timeout;
            }
        }

    }
}
