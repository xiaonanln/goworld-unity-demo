using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : ClientEntity {

	public override void OnCreated() {
		Debug.Log ("Player is created");
	}

	public override void OnDestroy() {
		Debug.Log ("Player is destroyed");
	}

	public override void OnEnterSpace() {
		if (this.IsPlayer) {
			GameObject.Find ("Main Camera").GetComponent<CameraFollow> ().target = this.gameObject.transform;
		}
		this.GetComponent<PlayerMovement> ().enabled = this.IsPlayer;
	}
}
