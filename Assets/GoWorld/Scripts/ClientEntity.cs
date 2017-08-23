using System;
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

	internal float targetYaw;
	internal Vector3 targetPos;
	internal float targetTime;

	public virtual void Init(GoWorldManager goworld, string entityID, bool isPlayer, float x, float y, float z, float yaw, Hashtable attrs) {
		this.goworld = goworld;
		this.ID = entityID;
		this.IsPlayer = isPlayer;
		this.transform.position = new Vector3 (x, y, z);
		this.SetYaw (yaw);
		this.Attrs = attrs;
	}

	public override string ToString() {
		return this.GetType ().Name + "<" + this.ID + ">";
	}

	public virtual void OnCreated () {}
	public virtual void OnDestroy () {}
	public virtual void OnEnterSpace () {}

	public void CallServer(string method, params object[] args) {
		goworld.CallServer (this.ID, method, args);
	}

	public void SyncEntityInfoToServer() {
		//Debug.Log ("sync yaw " + this.GetYaw ());
		goworld.SyncEntityInfoToServer (this);
	}

	internal void OnSyncEntityInfo(string entityID, float x, float y, float z, float yaw) {
		this.targetTime = Time.time + 0.11f;
		//Debug.LogWarning (this.ToString() + "OnSyncEntityInfo: " + x + "," + y + "," + z + " targetTime="+targetTime);
		this.targetPos = new Vector3 (x, y, z);
		this.targetYaw = yaw;
	}

	protected virtual void Update() {
		if (this.targetTime > 0) {
			float now = Time.time;
			//Debug.LogWarning(this.ToString() + "Update: now="+now + " targetTime="+this.targetTime);
			if (now < this.targetTime) {
				Vector3 distance = this.targetPos - transform.position;
				float leftTime = this.targetTime - now;
				float curyaw = this.GetYaw ();
				float distyaw = this.targetYaw - curyaw;
				float dt = Time.deltaTime;
				float rate = dt / (dt + leftTime);
				transform.position += distance * rate;
				SetYaw (curyaw + distyaw * rate);
				//Debug.LogWarning ("Distance: " + distance + " rate: " + rate);
			} else {
				transform.position = this.targetPos;
				SetYaw (this.targetYaw);
				this.targetTime = 0;
			}
		}
	}

	public float GetYaw() {
		return this.transform.rotation.eulerAngles.y;
	}

	public void SetYaw(float yaw) {
		this.transform.rotation = Quaternion.Euler (new Vector3 (0f, yaw, 0f));
	}

    internal void OnMapAttrChange(ArrayList path, string key, object val)
    {
        Hashtable t = this.getAttrByPath(path) as Hashtable;
        t[key] = val;
		string rootkey = path != null && path.Count > 0 ? (string)path[0] : key;
		System.Reflection.MethodInfo callback = this.GetType ().GetMethod ("OnAttrChange_" + rootkey);
		if (callback != null) {
			callback.Invoke (this, new object[0]);
		}
    }

    internal void OnMapAttrDel(ArrayList path, string key)
    {
        Hashtable t = this.getAttrByPath(path) as Hashtable;
        if (t.ContainsKey(key))
        {
            t.Remove(key);
        }
		string rootkey = path != null && path.Count > 0 ? (string)path[0] : key;
		System.Reflection.MethodInfo callback = this.GetType ().GetMethod ("OnAttrChange_" + rootkey);
		if (callback != null) {
			callback.Invoke (this, new object[0]);
		}
    }

    internal void OnListAttrAppend(ArrayList path, object val)
    {
        ArrayList l = getAttrByPath(path) as ArrayList;
        l.Add(val);
		string rootkey = (string)path[0];
		System.Reflection.MethodInfo callback = this.GetType ().GetMethod ("OnAttrChange_" + rootkey);
		if (callback != null) {
			callback.Invoke (this, new object[0]);
		}
    }

    internal void OnListAttrPop(ArrayList path)
    {
        ArrayList l = getAttrByPath(path) as ArrayList;
        l.RemoveAt(l.Count - 1);
		string rootkey = (string)path[0];
		System.Reflection.MethodInfo callback = this.GetType ().GetMethod ("OnAttrChange_" + rootkey);
		if (callback != null) {
			callback.Invoke (this, new object[0]);
		}
    }

    internal void OnListAttrChange(ArrayList path, int index, object val)
    {
        ArrayList l = getAttrByPath(path) as ArrayList;
        l[index] = val;
		string rootkey = (string)path[0];
		System.Reflection.MethodInfo callback = this.GetType ().GetMethod ("OnAttrChange_" + rootkey);
		if (callback != null) {
			callback.Invoke (this, new object[0]);
		}
    }

    internal object getAttrByPath(ArrayList path)
    {
        object attr = this.Attrs;

		if (path == null) {
			return attr; 
		}

        foreach (object key in path)
        {
            if (key.GetType() == typeof(string))
            {
                attr = (attr as Hashtable)[(string)key];
            }
            else
            {
                attr = (attr as ArrayList)[(int)key];
            }
        }

        Debug.Log("getAttrByPath: " + path.ToString() + " = " + attr);
        return attr;
    }

	public Int64 GetInt(string key) {
		object v = this.Attrs [key];
		if (v.GetType () == typeof(byte)) {
			return (Int64)(byte)(v);
		} else if (v.GetType () == typeof(int)) {
			return (Int64)(int)(v);
		} else {
			return (Int64)(int)(v);
		}
	}

}

