using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoWorldUnity3D;

public class Account : GoWorldUnity3D.ClientEntity{

	Login login;

    protected override void OnCreated() {
		//this.CallServer ("Login", "100");
		if (SceneManager.GetActiveScene ().name != "Login") {
			SceneManager.LoadScene ("Login");
			return;
		}
		
		login = GameObject.Find("Login").GetComponent<Login>();
	}

	protected override void OnDestroy() {
		Debug.Log("Account is destroyed");
	}

	public void ShowInfo(string msg) {
		login.showMessage (msg);
	}

	public void ShowError(string msg) {
		login.showMessage (msg);
	}

    protected override void OnBecomeClientOwner()
    {
        login.showMessage("Ready To Register / Login");
    }

    protected override void OnEnterSpace()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLeaveSpace()
    {
        throw new System.NotImplementedException();
    }

    void Update()
    {
    }

    public static new GameObject CreateGameObject(MapAttr attrs)
    {
        return GameObject.Instantiate(GameObject.Find("GoWorldController").GetComponent<GoWorldController>().Account);
    }

    protected override void Tick()
    {
    }
}
