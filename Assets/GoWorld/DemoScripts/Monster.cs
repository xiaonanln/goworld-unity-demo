using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Monster : ClientEntity {

	Animator anim;

	public override void OnCreated() {
		anim = GetComponent<Animator> ();
		Debug.Log ("Monster is created");
	}

	public override void OnDestroy() {
		Debug.Log ("Monster is destroyed");
	}

	public override void OnEnterSpace() {
	}

	public void OnAttrChange_action() {
		string action = this.Attrs["action"] as string;
		Debug.Log (this.ToString() + "'s action is changed to " + action); 

		anim.SetTrigger (action);
	}

}
