using UnityEngine;
using System.Collections;

namespace AntNet
{
    public class GameNet
    {
        private static GameObject netObject;
        public static TcpObject StartTcp(string name)
        {
            if (netObject == null)
            {
                netObject = new GameObject("Net");
                MonoBehaviour.DontDestroyOnLoad(netObject);
            }
            GameObject go = new GameObject(name);
            MonoBehaviour.DontDestroyOnLoad(go);
            go.transform.parent = netObject.transform;
            return go.AddComponent<TcpObject>();
        }

        public static UdpObject StartUdp(string name)
        {
            if (netObject == null)
            {
                netObject = new GameObject("Net");
                MonoBehaviour.DontDestroyOnLoad(netObject);
            }
            GameObject go = new GameObject(name);
            MonoBehaviour.DontDestroyOnLoad(go);
            go.transform.parent = netObject.transform;
            return go.AddComponent<UdpObject>();
        }

        public static HttpObject StartHttp(string name)
        {
            if (netObject == null)
            {
                netObject = new GameObject("Net");
                MonoBehaviour.DontDestroyOnLoad(netObject);
            }
            GameObject go = new GameObject(name);
            MonoBehaviour.DontDestroyOnLoad(go);
            go.transform.parent = netObject.transform;
            return go.AddComponent<HttpObject>();
        }

    }
}