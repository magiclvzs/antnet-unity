using UnityEngine;
using System;
using System.IO; 
using System.Collections;
using System.Collections.Generic;
 
namespace AntNet
{
    /// <summary>
    /// 接收到的数据
    /// </summary>
    public class RecvData
    {
        public byte[] data = null;                      //数据
        public MessageHead head = null;                 //协议头
        public object user = null;                      //用户自定义，可用于数据解析后的结果

        //public delegate void OnParseMsg(RecvData recv);
        protected static Dictionary<int, Action<RecvData>> parseDict = new Dictionary<int, Action<RecvData>>();
        public RecvData()
        {
            head = new MessageHead();
        }
        
        public RecvData(MessageHead head)
        {
            this.head = head;
        }

        public RecvData(MessageHead head, byte[] data)
        {
            this.head = head;
            this.data = data;
            int key = head.cmd * 1024 + head.act;
            if (parseDict.ContainsKey(key))
            {
                parseDict[key](this);
            }
        }

        public static void SetParser(byte cmd, byte act, Action<RecvData> parser)
        {
            int key = cmd * 1024 + act;
            parseDict[key] = parser;
        }


    }
}