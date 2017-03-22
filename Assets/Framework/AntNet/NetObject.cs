using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AntNet
{
    public class NetObject : MonoBehaviour
    {
        protected Queue<SendData> sendque = new Queue<SendData>();//发送队列
        protected Dictionary<int, Action<ushort, RecvData>> onceCallbackDict = new Dictionary<int, Action<ushort, RecvData>>();          //一次回调，cs模式交互
        protected Dictionary<int, Action<ushort, RecvData>> foreverCallbackDict = new Dictionary<int, Action<ushort, RecvData>>();      //永远回调，用于服务器通知等

        //超时处理
        protected Dictionary<int, MessageHead> indexDict = new Dictionary<int, MessageHead>();        //用于删除
        protected SortedList<float, MessageHead> timeoutDict = new SortedList<float, MessageHead>(); //用于计算

        protected Action<ushort> onRecvError = (err) => { };//错误统一处理回调

        public void SetOnRecvError(Action<ushort> callback)
        {
            onRecvError = callback;            
        }

        public void SetForeverCallBack(int cmd, Action<ushort, RecvData> callback)
        {
            if (callback == null)
            {
                if (foreverCallbackDict.ContainsKey(cmd))
                {
                    foreverCallbackDict.Remove(cmd);
                }
            }
            else
            {
                foreverCallbackDict[cmd] = callback;
            }
        }
        public void SetForeverCallBack(int cmd, int act, Action<ushort, RecvData> callback)
        {
            int key = cmd * 1024 + act;
            SetForeverCallBack(key, callback);
        }

        public virtual void SendMsg(SendData send, Action<ushort, RecvData> callback = null)
        {
            sendque.Enqueue(send);
            if (callback != null)
            {
                onceCallbackDict[send.head.index] = callback;
            }

            if (send.endTime > 0)
            {
                if (timeoutDict.ContainsKey(send.endTime))
                {
                    send.head.nextTimeout = timeoutDict[send.endTime];
                    timeoutDict[send.endTime] = send.head;
                }
                else
                {
                    timeoutDict[send.endTime] = send.head;
                }
                 
                indexDict[send.head.index] = send.head;
            }
        }

        protected void CallBack(MessageHead head, RecvData recv)
        {
            int key = recv.head.CmdAct;
            if (head.error == Error.ErrNetTimeout)
            {
                
            }

            if (foreverCallbackDict.ContainsKey(key))
            {
                foreverCallbackDict[key](head.error, recv);
            }

            if (foreverCallbackDict.ContainsKey(recv.head.cmd))
            {
                foreverCallbackDict[recv.head.cmd](head.error, recv);
            }

            if (indexDict.ContainsKey(recv.head.index))
            {
                indexDict.Remove(recv.head.index);
            }
            
            if (onceCallbackDict.ContainsKey(recv.head.index))
            {
                onceCallbackDict[recv.head.index](head.error, recv);                   
            }
           
        }
        public virtual void Stop(bool destroy) 
        { 
        }

        protected virtual void LateUpdate()
        {
            while (timeoutDict.Count > 0)
            {
                float time = timeoutDict.Keys[0];
                if (Time.realtimeSinceStartup < time)
                {
                    break;
                }

              
                var head = timeoutDict[time];
                while (head != null)
                {
                    if (indexDict.ContainsKey(head.index))
                    {
                        indexDict.Remove(head.index);
                        CallBack(head, null);                        
                    }

                    head = head.nextTimeout;
                }

                timeoutDict.Remove(time);
                 
            }
        }

        virtual public bool Available
        { 
            get
            {
                return false;
            }
        }
        virtual public SendData GetSendData()
        {
            return sendque.Count > 0 ? sendque.Dequeue() : null;
        }
        virtual public void Connect(string ip, uint port)
        {
        }
        virtual public void Connect(string ip, uint port, Action<TcpObject> onConnectFinish, Action<TcpObject> onDisConnect, float timeout = 0)
        {
        }
         
    }


}