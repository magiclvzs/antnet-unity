using UnityEngine;
using System.Collections;

namespace AntNet
{
    public class Log
    {
        static public bool enable = true;
        static public float time = 0;
        static public void Info(string format, params object[] args)
        {
            if (enable)
            {
                string s = string.Format(format, args);
                Debug.LogFormat(time + " : " + s);
            }
        }
        static public void Info(object message)
        {
            if (enable)
            {
                Debug.Log(time + " : " + message);
            }
        }

        static public void Warn(string format, params object[] args)
        {
            if (enable)
            {
                string s = string.Format(format, args);
                Debug.LogWarningFormat(time + " : " + s);
            }
        }
        static public void Warn(object message)
        {
            if (enable)
            {
                Debug.LogWarning(time + " : " + message);
            }
        }

        static public void Error(string format, params object[] args)
        {
            if (enable)
            {
                string s = string.Format(format, args);
                Debug.LogErrorFormat(time + " : " + s);
            }
        }
        static public void Error(object message)
        {
            if (enable)
            {
                Debug.LogError(time + " : " + message);
            }
        }

    }
}