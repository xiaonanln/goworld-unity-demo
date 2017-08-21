using System;
using System.Collections;
using UnityEngine;

public class SpaceInfo
{
	public int kind;
	public string ID;


	public SpaceInfo () {
		this.kind = 0;
		this.ID = "";
	}

	public SpaceInfo (string spaceID, Hashtable attrs)
	{
		this.ID = spaceID;
		IDictionaryEnumerator e = attrs.GetEnumerator ();
		while (e.MoveNext ()) {
			string key = e.Key as string;
			object val = e.Value;
			Debug.Log ("Space attr: " + key + " = " + val+" T " + val.GetType().Name);
			if (key == "_K") {
				this.kind = (int)(byte)val;
			}
		}
	}

	public void Leave() {
		this.kind = 0;
		this.ID = "";
	}

	public bool IsNil() {
		return this.kind == 0;
	}
}


