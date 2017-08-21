using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : ClientEntity {

	Animator anim;

	public override void OnCreated() {
		anim = GetComponent<Animator> ();
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

	public void OnAttrChange_action() {
		string action = this.Attrs["action"] as string;
		Debug.Log (this.ToString() + "'s action is changed to " + action); 

		anim.SetTrigger (action);
	}

}
