using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine.Events;

namespace AntNet
{
    public class HttpObject : MonoBehaviour
    {
        protected static float time;

        void Awake()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
        }

        IEnumerator SendGet(string _url, float endTime, Action<ushort, JsonData, WWW> onFinish, Action<float> processUpdate)
        {
            time = Time.unscaledTime;
            WWW www = new WWW(_url);
            while (!www.isDone)
            {
                if (endTime > 0 && Time.realtimeSinceStartup >= endTime)
                {
                    onFinish(Error.ErrNetTimeout, null, null);
                    yield break;
                }
                if (processUpdate != null)
                {
                    processUpdate(www.progress);
                }
                yield return null;
            }
            
            Log.Info("http use time:{0} url:{1}", Time.unscaledTime - time, www.url);
            ushort err = Error.ErrOk;
            JsonData json = null;
            try
            {
                json = JsonMapper.ToObject(www.text);
                if (json.IsObject && json.Contains("error"))
                {
                    err = (ushort)(int)json["error"];
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                err = Error.ErrJsonUnPack;
            }

            if (err != Error.ErrOk)
            { 
                Log.Warn(string.Format("服务器返回错误码 ： {0}", err));
            }

            onFinish(err, json, www); 
        }


        public void Get(string url, Action<ushort, JsonData, WWW> onFinish, float timeout = 0, Action<float> processUpdate = null)
        { 
            float endTime = timeout > 0 ? Time.unscaledTime + timeout : 0;
            StartCoroutine(SendGet(url, endTime, onFinish, processUpdate));
        }


    }
}