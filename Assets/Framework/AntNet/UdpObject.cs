using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
namespace AntNet
{
    public class UdpObject : NetObject
    {
        UdpClient udpClient = null;
        protected IPEndPoint serverAddr;


        public override void Connect(string ip, uint port)
        {
            if (udpClient == null)
            {
                udpClient = new UdpClient();
            }
            udpClient.Connect(ip, (int)port);
        }
        public override void Stop(bool destroy = true)
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
                if (destroy)
                {
                    Destroy(gameObject);
                }
            }
        }
        protected void ProcessRecv()
        {
            while (udpClient.Available > 0)
            {
                byte[] data = udpClient.Receive(ref serverAddr);
                MessageHead head = MessageHead.Parse(data);

                Log.Info("recv index:{0} needack:{1}", head.index, head.needAck);
                if (head.len == 0)
                {
                    if (head.needAck)
                    {
                        SendMsg(new SendData(head.cmd, head.act, (ushort)(head.flags | (ushort)MessageHead.Flags.FlagAck), head.index));
                    }
                    CallBack(head, new RecvData(head));
                }
                else if (head.len <= MessageHead.MaxRecvLength)
                {
                    uint oldlen = (head.flags & (ushort)MessageHead.Flags.FlagCompress) > 0 ? BitConverter.ToUInt32(data, MessageHead.Length) : 0;
                    uint datalen = oldlen > 0 ? oldlen : head.len;
                    byte[] xdata = new byte[datalen];
                    if (oldlen > 0)
                    {
                        GZipInputStream gzi = new GZipInputStream(new MemoryStream(data, MessageHead.Length + 4, (int)head.len - 4));
                        gzi.Read(xdata, 0, xdata.Length);
                    }
                    else
                    {
                        Array.Copy(data, MessageHead.Length, xdata, 0, datalen);
                    }

                    if (head.needAck)
                    {
                        SendMsg(new SendData(head.cmd, head.act, (ushort)(head.flags | (ushort)MessageHead.Flags.FlagAck), head.index));
                    }
                    CallBack(head, new RecvData(head, xdata));
                }
            }
        }

        public override void SendMsg(SendData send, Action<ushort, RecvData> callback = null)
        {
            base.SendMsg(send, callback);
            var data = send.GetData();
            udpClient.Send(data, data.Length);
        }

        override public bool Available
        { 
            get
            {
                return udpClient != null;
            }
        }

        protected override void LateUpdate()
        { 
            if (Available)
            {
                ProcessRecv();
            }
            base.LateUpdate();
        }
    }
}