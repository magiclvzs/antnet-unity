using UnityEngine;
using System.Collections;

public class Net : MonoBehaviour {

	// Use this for initialization
	void Start () {
        AntNet.GameNet.StartTcp("logic").Connect("192.168.10.208", 8888, (tcp) => { }, (tcp) => { });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
