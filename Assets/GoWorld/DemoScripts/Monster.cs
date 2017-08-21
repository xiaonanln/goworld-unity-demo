using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Monster : ClientEntity {

	public override void OnCreated() {
		Debug.Log ("Monster is created");
	}

	public override void OnDestroy() {
		Debug.Log ("Monster is destroyed");
	}

	public override void OnEnterSpace() {
	}
}
