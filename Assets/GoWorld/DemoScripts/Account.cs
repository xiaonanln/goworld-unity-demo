using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Account : ClientEntity {

	Login login;

	public override void OnCreated() {
		//this.CallServer ("Login", "100");
		if (SceneManager.GetActiveScene ().name != "Login") {
			SceneManager.LoadScene ("Login");
			return;
		}
		
		login = GameObject.Find("Login").GetComponent<Login>();
	}

	public override void OnDestroy() {
		Debug.Log("Account is destroyed");
	}

	public void ShowInfo(string msg) {
		login.showMessage (msg);
	}

	public void ShowError(string msg) {
		login.showMessage (msg);
	}
}
