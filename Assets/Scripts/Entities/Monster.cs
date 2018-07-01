using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoWorldUnity3D;

public class Monster : GameEntity {

	Animator anim;
	LineRenderer lineRenderer;
	float attackTime;

    protected override void OnCreated() {
        anim = gameObject.GetComponent<Animator> ();
		lineRenderer = gameObject.GetComponent<LineRenderer> ();
		Debug.Log ("Monster is created at" + gameObject.transform.position);
	}

    protected override void OnDestroy() {
		Debug.Log ("Monster is destroyed");
	}

	protected override void OnEnterSpace() {
        string action = this.Attrs.GetStr("action");
		anim.SetTrigger (action);
	}

	public void OnAttrChange_action() {
		string action = this.Attrs.GetStr("action");
        Debug.Log (this.ToString() + "'s action is changed to " + action); 

		anim.SetTrigger (action);
	}

	public void DisplayAttack(string playerID) {
		ClientEntity player = GoWorld.GetEntity (playerID);
		//Debug.LogWarning (this.ToString () + " attack " + playerID + " " + player.ToString());
		Vector3 startPos = this.gameObject.transform.position;
		Vector3 endPos = player.gameObject.transform.position;
		startPos.y = 0.5f;
		endPos.y = 0.5f;
		lineRenderer.SetPosition(0, startPos);
		lineRenderer.SetPosition (1, endPos);
		lineRenderer.enabled = true;
		attackTime = Time.time;
	}

	void Update() {
		if (Time.time >= attackTime + 0.2f) {
			lineRenderer.enabled = false;
		}
	}

    protected override void OnBecomeClientOwner()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLeaveSpace()
    {
    }

    public static new GameObject CreateGameObject(MapAttr attrs)
    {
        return GameObject.Instantiate(GameObject.Find("GoWorldController").GetComponent<GoWorldController>().ZomBunny);
    }
}
