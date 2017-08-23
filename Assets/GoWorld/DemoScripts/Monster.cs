using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Monster : ClientEntity {

	Animator anim;
	LineRenderer lineRenderer;
	float attackTime;

	public override void OnCreated() {
		anim = GetComponent<Animator> ();
		lineRenderer = GetComponent<LineRenderer> ();
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

	public void DisplayAttack(string playerID) {
		ClientEntity player = this.goworld.GetEntity (playerID);
		//Debug.LogWarning (this.ToString () + " attack " + playerID + " " + player.ToString());
		Vector3 startPos = transform.position;
		Vector3 endPos = player.transform.position;
		startPos.y = 0.5f;
		endPos.y = 0.5f;
		lineRenderer.SetPosition(0, startPos);
		lineRenderer.SetPosition (1, endPos);
		lineRenderer.enabled = true;
		attackTime = Time.time;
	}

	protected override void Update() {
		base.Update ();
		if (Time.time >= attackTime + 0.2f) {
			lineRenderer.enabled = false;
		}
	}

}
