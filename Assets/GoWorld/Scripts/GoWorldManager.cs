using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

class entityCreationInfo {
	internal readonly string typeName;
	internal readonly string entityID;
	internal readonly bool isPlayer;
	internal readonly float x;
	internal readonly float y;
	internal readonly float z;
	internal readonly float yaw;
	internal readonly Hashtable attrs;

	internal entityCreationInfo(string typeName, string entityID, bool isPlayer, float x, float y, float z,float yaw, Hashtable attrs) {
		this.typeName = typeName;
		this.entityID = entityID;
		this.isPlayer = isPlayer;
		this.x = x;
		this.y = y;
		this.z = z;
		this.yaw = yaw;
		this.attrs = attrs;
	}
}

public class GoWorldManager : MonoBehaviour {
	public ClientEntity[] entityList;
	internal GameClient gameClient;
	internal ClientEntity player;
	internal System.Collections.Generic.Dictionary<string, ClientEntity> entities;
	internal 	SpaceInfo space = new SpaceInfo(); // nil space
	internal IList<entityCreationInfo> entityCreationQueue = new List<entityCreationInfo>();

	// Use this for initialization
	void Awake () {
		Debug.Log ("GoWorldManager is awake ...");
		this.gameObject.AddComponent<GameClient> ();
		gameClient = GetComponent<GameClient> ();
		this.entities = new System.Collections.Generic.Dictionary<string, ClientEntity> ();
		Debug.Log ("GameClient: " + gameClient);
		GameObject.DontDestroyOnLoad (this.gameObject);
		SceneManager.sceneLoaded += this.onSceneLoaded;
	}

	public ClientEntity GetEntity(string id) {
		return this.entities [id];
	}

	public void OnSyncEntityInfo(string entityID, float x, float y, float z, float yaw) {
		ClientEntity entity;
		if (!this.entities.TryGetValue (entityID, out entity)) {
			Debug.LogWarning ("OnSyncEntityInfo: entity not found: " + entityID);
			return;
		}

		entity.OnSyncEntityInfo (entityID, x, y, z, yaw);
	}

	public void OnCreateEntity(string typeName, string entityID, bool isPlayer, float x, float y, float z,float yaw, Hashtable attrs) {
		if (typeName == "__space__") {
			this.OnEnterSpace (entityID, attrs);
			return;
		}

		if (!this.isSpaceReady ()) {
			this.entityCreationQueue.Add (new entityCreationInfo (typeName, entityID, isPlayer, x, y, z, yaw, attrs));
			return;
		}

		this.createEntity(typeName, entityID, isPlayer, x,y,z,yaw, attrs);
	}

	bool isSpaceReady() {
		if (this.space.IsNil()) {
			return true;
		}

		return SceneManager.GetActiveScene ().name == "Level 01";
	}

	public void createEntity(string typeName, string entityID, bool isPlayer, float x, float y, float z,float yaw, Hashtable attrs) {
		for (int i = 0; i < entityList.Length; i++) {
			ClientEntity objModel = entityList [i];

			if (objModel.GetType().Name == typeName) {
				Debug.Log ("GameObject " + objModel.gameObject + " scene" + objModel.gameObject.scene);
				GameObject obj = Instantiate (objModel.gameObject);
				ClientEntity entity = obj.GetComponent (typeName) as ClientEntity;
				Debug.Assert (entity != null);
				Debug.Log ("Entity " + typeName + "." + entityID + " created: " + entity);
				entity.Init (this, entityID, isPlayer, x, y, z, yaw, attrs);
				entity.Space = this.space;

				if (isPlayer) {
					this.player = entity;
					GameObject.DontDestroyOnLoad (entity.gameObject);
				}

				this.entities [entityID] = entity;
				entity.OnCreated ();

				return ;
			}
		}

		Debug.LogAssertionFormat ("Entity type {0} is unknown", typeName);
	}

	public void OnCallEntityMethod(string entityID, string method, object[] args) {
		ClientEntity entity;
		if (!this.entities.TryGetValue (entityID, out entity)) {
			Debug.LogError ("OnCallEntityMethod: entity " + entityID + " is not found when calling " + method);
			return;
		}

		System.Reflection.MethodInfo methodInfo = entity.GetType ().GetMethod (method);
		if (methodInfo == null) {
			Debug.LogErrorFormat("OnCallEntityMethod: entity {0} has no public method: {1}", entity.ToString(), method);
			return ;
		}

		methodInfo.Invoke (entity, args);
	}

	public void OnDestroyEntity(string typeName, string entityID) {
		if (typeName == "__space__") {
			this.OnLeaveSpace ();
		}

		ClientEntity entity;
		if (this.entities.TryGetValue (entityID, out entity)) {
			if (this.player == entity) {
				this.player = null;
			}

			entity.OnDestroy ();
			this.entities.Remove (entityID);
			GameObject.Destroy (entity.gameObject);
		}
	}

	void OnEnterSpace(string spaceID, Hashtable spaceAttrs) {
		this.space = new SpaceInfo (spaceID, spaceAttrs);
		if (this.player) {
			this.player.Space = this.space;
		}
		SceneManager.LoadScene ("Level 01", LoadSceneMode.Single);
	}

	void OnLeaveSpace() {
		this.space.Leave ();
	}

	public void CallServer(string entityID, string method, object[] args) {
		Debug.Log ("Call server: " + entityID + "." + method + "(" + args + ")");
		gameClient.CallServer (entityID, method, args);
	}

	public void SyncEntityInfoToServer(ClientEntity entity) {
		Transform transform = entity.transform;
		gameClient.SyncEntityInfoToServer (entity.ID, transform.position.x, transform.position.y, transform.position.z, entity.GetYaw());
	}

	public ClientEntity GetPlayer() {
		return this.player;
	}

	void onSceneLoaded(Scene scene, LoadSceneMode mode) {
		Debug.Log ("Scene is loaded: " + scene.name);

		IEnumerator<entityCreationInfo> e = this.entityCreationQueue.GetEnumerator ();
		this.entityCreationQueue = new List<entityCreationInfo> ();

		while (e.MoveNext ()) {
			entityCreationInfo ci = e.Current;
			createEntity (ci.typeName, ci.entityID, ci.isPlayer, ci.x, ci.y, ci.z, ci.yaw, ci.attrs);
		}

		if (this.player) {
			this.player.OnEnterSpace ();
		}
	}

    internal void OnMapAttrChange(string entityID, ArrayList path, string key, object val)
    {
        ClientEntity entity;
        if (!this.entities.TryGetValue(entityID, out entity))
        {
            Debug.Log("OnMapAttrChange: Entity " + entityID + " is not found");
            return;
        }

        entity.OnMapAttrChange(path, key, val);
    }

    internal void OnMapAttrDel(string entityID, ArrayList path, string key)
    {
        ClientEntity entity;
        if (!this.entities.TryGetValue(entityID, out entity))
        {
            Debug.Log("OnMapAttrDel: Entity " + entityID + " is not found");
            return;
        }

        entity.OnMapAttrDel(path, key);
    }

    internal void OnListAttrAppend(string entityID, ArrayList path, object val)
    {
        ClientEntity entity;
        if (!this.entities.TryGetValue(entityID, out entity))
        {
            Debug.Log("OnListAttrAppend: Entity " + entityID + " is not found");
            return;
        }

        entity.OnListAttrAppend(path, val);
    }

    internal void OnListAttrPop(string entityID, ArrayList path)
    {
        ClientEntity entity;
        if (!this.entities.TryGetValue(entityID, out entity))
        {
            Debug.Log("OnListAttrPop: Entity " + entityID + " is not found");
            return;
        }

        entity.OnListAttrPop(path);
    }

    internal void OnListAttrChange(string entityID, ArrayList path, int index, object val)
    {
        ClientEntity entity;
        if (!this.entities.TryGetValue(entityID, out entity))
        {
            Debug.Log("OnListAttrChange: Entity " + entityID + " is not found");
            return;
        }

        entity.OnListAttrChange(path, index, val);
    }
}
