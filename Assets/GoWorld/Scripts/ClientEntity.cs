using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class ClientEntity : MonoBehaviour {

	protected GoWorldManager goworld;
	[HideInInspector]
	public string ID;
	[HideInInspector]
	public bool IsPlayer;
	[HideInInspector]
	public SpaceInfo Space;
	[HideInInspector]
	public Hashtable Attrs;

	public virtual void Init(GoWorldManager goworld, string entityID, bool isPlayer, float x, float y, float z, float yaw, Hashtable attrs) {
		this.goworld = goworld;
		this.ID = entityID;
		this.IsPlayer = isPlayer;
		this.transform.position = new Vector3 (x, y, z);
		this.SetYaw (yaw);
		this.Attrs = attrs;
	}

	public virtual void OnCreated () {}
	public virtual void OnDestroy () {}
	public virtual void OnEnterSpace () {}

	public void CallServer(string method, params object[] args) {
		goworld.CallServer (this.ID, method, args);
	}

	public void SyncEntityInfoToServer() {
		Debug.Log ("sync yaw " + this.GetYaw ());
		goworld.SyncEntityInfoToServer (this);
	}

	public void OnSyncEntityInfo(string entityID, float x, float y, float z, float yaw) {
		GameObject obj = this.gameObject;
		obj.transform.position = new Vector3 (x, y, z);
		this.SetYaw (yaw);
	}

	public float GetYaw() {
		return this.transform.rotation.eulerAngles.y;
	}

	public void SetYaw(float yaw) {
		this.transform.rotation = Quaternion.Euler (new Vector3 (0f, yaw, 0f));
	}

}

