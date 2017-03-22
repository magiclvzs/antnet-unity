using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Collections;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
namespace AntNet
{

public class TcpObject : NetObject 
{
    internal string ip;
    internal uint port;

    protected int totalRead = 0;                                //读取数据大小
    protected TcpClient tcpClient = null;                       //连接
    protected MessageHead head = null;                          //保存协议头
    protected bool connTimeout = false;

    protected Action<TcpObject> connCallback = (obj) => { };                   //连接回调
    protected Action<TcpObject> disconnCallback = (obj) => { };            //连接断开回调
     
    protected IAsyncResult asyncWriter = null;                   
    protected IAsyncResult asyncConnect = null;
    protected IAsyncResult asyncReader = null;
   
    protected byte[] headData = new byte[MessageHead.Length];
    protected byte[] bodyData = new byte[1024 * 1024 * 2];


    public bool isConnTimeout
    { 
        get 
        { 
            return connTimeout;
        }
    }

    protected IEnumerator OnConnectTimeout(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        connTimeout = true;
        if (tcpClient != null)
        {
            Stop(false);
            connCallback(this);
        }
    }
     

    void OnDestroy()
    {
        Stop(false);
    }
    public override void Connect(string ip, uint port, Action<TcpObject> onConnectFinish, Action<TcpObject> onDisConnect, float timeout = 0)
    {
        Stop(false);
        connTimeout = false;
        this.ip = ip;
        this.port = port;
        tcpClient = new TcpClient();
        tcpClient.NoDelay = true;
        connCallback = onConnectFinish;
        disconnCallback = onDisConnect;
        asyncConnect = tcpClient.BeginConnect(ip, (int)port, null, null);
        
        if(timeout > 0)
        {
            StartCoroutine(OnConnectTimeout(timeout));
        }        
    }

    public override void Stop(bool destroy)
    {
        if (tcpClient != null)
        {
            StopAllCoroutines();

            tcpClient.Close();
            tcpClient = null;
            
            head = null;
            asyncWriter = null;
            asyncReader = null;
            asyncConnect = null; 
            Log.Info ("net close {0}", gameObject.name);
        }

        if (destroy)
        {
            Destroy(gameObject);
            Log.Info("net object destroy {0}", gameObject.name);
        }
    }

    protected void OnRecvHead()
    {
        int read = tcpClient.GetStream().EndRead(asyncReader);
        byte[] data = (byte[])asyncReader.AsyncState;
        asyncReader.AsyncWaitHandle.Close();
        asyncReader = null;
        if (read > 0)
        {
            totalRead += read;            
            if (totalRead == MessageHead.Length)
            {
                totalRead = 0;
                head = MessageHead.Parse(data);
				if (head.error > 0)
				{
                    if (head.error != Error.ErrOk)
                    {
                        if (onRecvError != null) 
                        {
                            onRecvError(head.error);
                        }

                        Log.Warn("服务器返回错误码 ： {0}", head.error);
                    }
				}
                if (head.len > MessageHead.MaxRecvLength)
                {
                    Stop(false);
                    disconnCallback(this);
                    return;
                }

                if (head.len == 0)
                {
                    CallBack(head, new RecvData(head));
                    head = null;
                }

                ProcessRecv();
            }
            else
            {
               asyncReader = tcpClient.GetStream().BeginRead(data, totalRead, MessageHead.Length - totalRead, null, data);
            }
        }
        else if(read == 0)
        {
            Stop(false);
            disconnCallback(this);
        }
    }


    protected void OnRecvData()
    {
        int read = tcpClient.GetStream().EndRead(asyncReader);
        byte[] data = (byte[])asyncReader.AsyncState;
        asyncReader.AsyncWaitHandle.Close();
        asyncReader = null;
        if (read > 0)
        {
            totalRead += read;
            if (totalRead == head.len)
            {
                totalRead = 0;

                uint oldlen = (head.flags & (ushort)MessageHead.Flags.FlagCompress) > 0 ? BitConverter.ToUInt32(data, 0) : 0;
                if (oldlen > 0)
                {
                    byte[] xdata = new byte[oldlen];
                    GZipInputStream gzi = new GZipInputStream(new MemoryStream(data, 4, (int)head.len - 4));
                    gzi.Read(xdata, 0, xdata.Length);
                    CallBack(head, new RecvData(head, xdata));
                }
                else
                {
                    CallBack(head, new RecvData(head, data));
                }

                head = null;
                ProcessRecv();
            }
            else
            {
                asyncReader = tcpClient.GetStream().BeginRead(data, totalRead, (int)(head.len - totalRead), null, data);
            }
        }
        else if (read == 0)
        {
            Stop(false);
            disconnCallback(this);
        }
    }
     
    protected void ProcessSend()
    {
        if (asyncWriter == null)
        {
            SendData send = GetSendData();
            if (send != null)
            {
                try
                { 
                    var data = send.GetData();
                    asyncWriter = tcpClient.GetStream().BeginWrite(data, 0, data.Length, null, null);
                    if (asyncWriter.IsCompleted)
                    {
                        tcpClient.GetStream().EndWrite(asyncWriter);
                        asyncWriter.AsyncWaitHandle.Close();
                        asyncWriter = null;
                        ProcessSend();
                    }
                }
                catch
                {
                    disconnCallback(this);
                }
            }            
        }
        else if (asyncWriter.IsCompleted)
        {
            tcpClient.GetStream().EndWrite(asyncWriter);
            asyncWriter.AsyncWaitHandle.Close();
            asyncWriter = null;
            ProcessSend();
        }
    }
    
    protected void ProcessRecv()
    {
        if (head == null)
        {
            if (asyncReader == null)
            {
                asyncReader = tcpClient.GetStream().BeginRead(headData, 0, MessageHead.Length, null, headData);
            }
            else if (asyncReader.IsCompleted)
            {
                OnRecvHead();
            }
        }
        else
        {
            if (asyncReader == null)
            {
                asyncReader = tcpClient.GetStream().BeginRead(bodyData, 0, (int)head.len, null, bodyData);
            }
            else if (asyncReader.IsCompleted)
            {
                OnRecvData();
            }
        }
    }

    protected override void LateUpdate()
    {
        if (asyncConnect != null)
        {
            if (asyncConnect.IsCompleted)
            {
                try
                {
                    tcpClient.EndConnect(asyncConnect);
                }
                catch (Exception e)
                {
                     Log.Error(e.ToString());
                }
                asyncConnect = null;
                StopAllCoroutines();
                connCallback(this);
            }
        }

        if (Available)
        {
            ProcessRecv();
            ProcessSend();
        }
        base.LateUpdate();
	}

    public override void SendMsg(SendData send, Action<ushort, RecvData> callback = null)
    {
        if (tcpClient == null)
        {
            if (callback != null)
            { 
                callback(Error.ErrNetTimeout, null);
            }
        }
        else
        {
            if (send.endTime == 0)
            {
                send.endTime = Time.realtimeSinceStartup + 6; 
            }
            base.SendMsg(send, callback);
            ProcessSend();
        }
    }
 
    override public bool Available
    { 
        get 
        {
            return tcpClient != null && tcpClient.Connected;
        }
    } 

}
}