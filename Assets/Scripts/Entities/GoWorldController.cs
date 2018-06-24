using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoWorldController : MonoBehaviour {

    // Use this for initialization
    public GameObject Player;
    public GameObject ZomBear;
    public GameObject ZomBunny;
    public GameObject Hellephant;
    public GameObject Account;

    void Start () {
        GameObject.DontDestroyOnLoad(gameObject);

        Debug.Log("Register Entity Type Account ...");
        GoWorldUnity3D.GoWorld.RegisterEntity(this.Account);
        Debug.Log("Register Entity Type Player ...");
        GoWorldUnity3D.GoWorld.RegisterEntity(this.Player);
        Debug.Log("Register Entity Type Monster ...");
        GoWorldUnity3D.GoWorld.RegisterEntity(this.ZomBunny);
        Debug.Log("Connecting Serer ...");
        GoWorldUnity3D.GoWorld.Connect("ec2-13-229-128-242.ap-southeast-1.compute.amazonaws.com", 15011);
    }
	
	// Update is called once per frame
	void Update () {
        GoWorldUnity3D.GoWorld.Update();
	}

}
