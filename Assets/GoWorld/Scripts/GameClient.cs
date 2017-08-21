using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

public class GameClient : MonoBehaviour {
	const int SIZE_FIELD_SIZE = 4;
	const int MAX_PAYLOAD_LEN = 1*1024*1024;

	const int CLIENTID_LENGTH = 16;
	const int ENTITYID_LENGTH = 16;

	const UInt16 MT_INVALID = 0;
		// Server Messages
	const UInt16 MT_SET_GAME_ID = 1;
	const UInt16 MT_SET_GATE_ID = 2;
	const UInt16 MT_NOTIFY_CREATE_ENTITY = 3;
	const UInt16 MT_NOTIFY_DESTROY_ENTITY = 4;
	const UInt16 MT_DECLARE_SERVICE = 5;
	const UInt16 MT_UNDECLARE_SERVICE = 6;
	const UInt16 MT_CALL_ENTITY_METHOD = 7;
	const UInt16 MT_CREATE_ENTITY_ANYWHERE = 8;
	const UInt16 MT_LOAD_ENTITY_ANYWHERE = 9;
	const UInt16 MT_NOTIFY_CLIENT_CONNECTED = 10;
	const UInt16 MT_NOTIFY_CLIENT_DISCONNECTED = 11;
	const UInt16 MT_CALL_ENTITY_METHOD_FROM_CLIENT = 12;
	const UInt16 MT_SYNC_POSITION_YAW_FROM_CLIENT = 13;
	const UInt16 MT_NOTIFY_ALL_GAMES_CONNECTED = 14;
	const UInt16 MT_NOTIFY_GATE_DISCONNECTED = 15;
	const UInt16 MT_START_FREEZE_GAME = 16;
	const UInt16 MT_START_FREEZE_GAME_ACK = 17;
	// Message types for migrating
	const UInt16 MT_MIGRATE_REQUEST = 18;
	const UInt16 MT_REAL_MIGRATE = 19;

	const UInt16 MT_GATE_SERVICE_MSG_TYPE_START = 1000;
	const UInt16 MT_REDIRECT_TO_GATEPROXY_MSG_TYPE_START = 1001; // messages that should be redirected to client proxy
		const UInt16 MT_CREATE_ENTITY_ON_CLIENT = 1002;
	const UInt16 MT_DESTROY_ENTITY_ON_CLIENT = 1003;
		const UInt16 MT_NOTIFY_MAP_ATTR_CHANGE_ON_CLIENT = 1004;
	const UInt16 MT_NOTIFY_MAP_ATTR_DEL_ON_CLIENT = 1005;
	const UInt16 MT_NOTIFY_LIST_ATTR_CHANGE_ON_CLIENT = 1006;
	const UInt16 MT_NOTIFY_LIST_ATTR_POP_ON_CLIENT = 1007;
	const UInt16 MT_NOTIFY_LIST_ATTR_APPEND_ON_CLIENT = 1008;
	const UInt16 MT_CALL_ENTITY_METHOD_ON_CLIENT = 1009;
	const UInt16 MT_UPDATE_POSITION_ON_CLIENT = 1010;
	const UInt16 MT_UPDATE_YAW_ON_CLIENT = 1011;
	const UInt16 MT_SET_CLIENTPROXY_FILTER_PROP = 1012;
	const UInt16 MT_CLEAR_CLIENTPROXY_FILTER_PROPS = 1013;

		// add more ...

	const UInt16 MT_REDIRECT_TO_GATEPROXY_MSG_TYPE_STOP = 1500;

	const UInt16 MT_CALL_FILTERED_CLIENTS = 1501;
	const UInt16 MT_SYNC_POSITION_YAW_ON_CLIENTS = 1502;

		// add more ...

	const UInt16 MT_GATE_SERVICE_MSG_TYPE_STOP = 2000;

	GoWorldManager manager;
	bool connecting;
	TcpClient tcpClient;

	enum RecvState {
		RecvPayloadLen, 
		RecvPayload, 
	} ; 
	RecvState recvState;
	byte[] recvPayloadLenBuff;
	byte[] payloadBuff;
	UInt32 recvPayloadLen;

	void Awake() {
		Debug.Log ("GameClient is awake ...");
		manager = GetComponent<GoWorldManager> ();
		recvState = RecvState.RecvPayloadLen;
		recvPayloadLenBuff = new byte[SIZE_FIELD_SIZE];
		payloadBuff = new byte[MAX_PAYLOAD_LEN];
		tcpClient = new TcpClient ();
		// BitConverter.IsLittleEndian = true;
	}

	void onConnected(System.IAsyncResult result) {
		Debug.Log("Connected");
		tcpClient.ReceiveBufferSize = MAX_PAYLOAD_LEN;
		connecting = false;
	}

	// Update is called once per frame
	void Update () {
		if (connecting) {
			return;
		}

		if (!tcpClient.Connected && !connecting) {
			Debug.Log ("Connecting to server ...");
			connecting = true;
			tcpClient.BeginConnect ("localhost", 15011, onConnected, 0);
			return;
		}

		// Connection is ok
		while (tryHandleNextPacket ()) {
		}
	}

	bool tryHandleNextPacket()  {
		if (recvState == RecvState.RecvPayloadLen) {
			if (tcpClient.Available < 4) {
				return false;
			}

			tcpClient.Client.Receive (recvPayloadLenBuff);
			recvPayloadLen = BitConverter.ToUInt32 (recvPayloadLenBuff, 0);
			recvState = RecvState.RecvPayload;
			Debug.Log ("Packet payload length: " + recvPayloadLen);
		}

		// recvState == RecvState.RecvPayload
		if (tcpClient.Available < recvPayloadLen) {
			return false;
		}

		tcpClient.Client.Receive (payloadBuff, (int)recvPayloadLen, SocketFlags.None);
		recvState = RecvState.RecvPayloadLen;

		int payloadLen = (int)recvPayloadLen;
		recvPayloadLen = 0;

		handlePacket (payloadBuff, payloadLen);
		return true;
	}

	void handlePacket(byte[] payload, int payloadLen) {
		int readPos = 0;
		UInt16 msgtype = readUint16 (payload, readPos, out readPos);
		Debug.Log ("Received packet of size " + payloadLen + " msgtype=" + msgtype);

		if (msgtype != MT_CALL_FILTERED_CLIENTS && msgtype != MT_SYNC_POSITION_YAW_ON_CLIENTS) {
			readUint16 (payload, readPos, out readPos);
			readBytes (payload, readPos, out readPos, CLIENTID_LENGTH);
		}

		if (msgtype == MT_SYNC_POSITION_YAW_ON_CLIENTS) {
			this.handleSyncPositionYawOnClients (payload, payloadLen, readPos);
		} else if (msgtype == MT_NOTIFY_MAP_ATTR_CHANGE_ON_CLIENT) {
			this.handleNotifyMapAttrChangeOnClient (payload, readPos);
		} else if (msgtype == MT_NOTIFY_MAP_ATTR_DEL_ON_CLIENT) {
			this.handleNotifyMapAttrDelOnClient (payload, readPos);
		} else if (msgtype == MT_NOTIFY_LIST_ATTR_APPEND_ON_CLIENT) {
			this.handleNotifyListAttrAppendOnClient (payload, readPos);
		} else if (msgtype == MT_NOTIFY_LIST_ATTR_POP_ON_CLIENT) {
			this.handleNotifyListAttrPopOnClient (payload, readPos);
		} else if (msgtype == MT_NOTIFY_LIST_ATTR_CHANGE_ON_CLIENT) {
			this.handleNotifyListAttrChangeOnClient (payload, readPos);
		}
		else if (msgtype == MT_CREATE_ENTITY_ON_CLIENT) {
			this.handleCreateEntityOnClient (payload, readPos);
		} else if (msgtype == MT_CALL_ENTITY_METHOD_ON_CLIENT) {
			this.handleCallEntityMethodOnClient (payload, readPos);
		} else if (msgtype == MT_DESTROY_ENTITY_ON_CLIENT) {
			this.handleDestroyEntityOnClient (payload, readPos);
		} 
		else {
			Debug.LogError ("unknown message type: " + msgtype);
		}
	}

	byte readByte(byte[] payload, int readPos, out int newReadPos) {
		byte v = payload [readPos];
		newReadPos = readPos + 1;
		return v; 
	}

	UInt16 readUint16(byte[] payload, int readPos, out int newReadPos) {
		UInt16 v = BitConverter.ToUInt16 (payload, readPos);
		newReadPos = readPos + 2;
		return v; 
	}

	UInt32 readUint32(byte[] payload, int readPos, out int newReadPos) {
		UInt32 v = BitConverter.ToUInt32 (payload, readPos);
		newReadPos = readPos + 4;
		return v; 
	}

	bool readBool(byte[] payload, int readPos, out int newReadPos) {
		byte b = readByte (payload, readPos, out newReadPos);
		return b != 0;
	}

	float readFloat32(byte[] payload, int readPos, out int newReadPos) {
		float v = BitConverter.ToSingle(payload, readPos);
		newReadPos = readPos + 4;
		return v; 
	}

	byte[] readBytes(byte[] payload, int readPos, out int newReadPos, int length) {
		byte[] b = new byte[length];
		for (int i = 0; i < length; i++) {
			b [i] = payload [readPos + i];
		}
		newReadPos = readPos + length;
		//Debug.Log("readBytes: length="+length+", b="+b + ", newReadPos="+newReadPos);
		return b; 
	}

	string readEntityID(byte[] payload, int readPos, out int newReadPos) {
		byte[] b = readBytes(payload, readPos, out newReadPos, ENTITYID_LENGTH);
		return System.Text.Encoding.ASCII.GetString (b);
	}

	byte[] readVarBytes(byte[] payload, int readPos, out int newReadPos) {
		UInt32 length = readUint32 (payload, readPos, out readPos); 
		return readBytes (payload, readPos, out newReadPos, (int)length); 
	}

	string readVarStr(byte[] payload, int readPos, out int newReadPos) {
		byte[] b = readVarBytes (payload, readPos, out newReadPos);
		return System.Text.Encoding.ASCII.GetString (b);
	}

	object readData(byte[] payload, int readPos, out int newReadPos) {
		byte[] b = readVarBytes (payload, readPos, out newReadPos);
		return unpackData (b);
	}

	object[] readArgs(byte[]payload, int readPos, out int newReadPos) {
		int argcount = (int) readUint16 (payload, readPos, out readPos);
		object[] args = new object[argcount];
		for (int i = 0; i < argcount; i++) {
			args[i] = readData (payload, readPos, out readPos);
		}
		newReadPos = readPos;
		return args;
	}


	int appendByte(int writePos, byte b) {
		payloadBuff [writePos] = b;
		return writePos + 1;
	}

	int appendUint16(int writePos, UInt16 v) {
		return appendBytes (writePos, BitConverter.GetBytes (v));
	}

	int appendUint32(int writePos, UInt32 v) {
		return appendBytes (writePos, BitConverter.GetBytes (v));
	}

	int appendFloat32(int writePos, float v) {
		return appendBytes (writePos, BitConverter.GetBytes (v));
	}

	int appendBytes(int writePos, byte[] bytes) {
		bytes.CopyTo (payloadBuff, writePos);
		return writePos + bytes.Length;
	}

	int appendEntityID(int writePos, string entityID) {
		byte[] bytes = System.Text.Encoding.ASCII.GetBytes (entityID);
		return appendBytes (writePos, bytes);
	}

	int appendVarBytes(int writePos, byte[] bytes) {
		writePos = appendUint32 (writePos, (UInt32)(bytes.Length));
		return appendBytes (writePos, bytes);
	}

	int appendVarStr(int writePos, string s) {
		byte[] bytes = System.Text.Encoding.ASCII.GetBytes (s);
		return appendVarBytes (writePos, bytes);
	}

	int appendData(int writePos, object data) {
		byte[] b = packData (data);
		return appendVarBytes (writePos, b);
	}

	int appendArgs(int writePos, object[] args) {
		writePos = appendUint16 (writePos, (UInt16) (args.Length));
		foreach (object arg in args) {
			writePos = appendData (writePos, arg);
		}
		return writePos;
	}

	void handleSyncPositionYawOnClients(byte[] payload, int payloadLen, int readPos) {
		Debug.Log ("handleSyncPositionYawOnClients: unread payload len: " + (payloadLen - readPos));
		while (readPos < payloadLen) {
			string entityID = readEntityID (payload, readPos, out readPos);
			float x = readFloat32 (payload, readPos, out readPos);
			float y = readFloat32 (payload, readPos, out readPos);
			float z = readFloat32 (payload, readPos, out readPos);
			float yaw = readFloat32 (payload, readPos, out readPos);
			manager.OnSyncEntityInfo (entityID, x, y, z, yaw);
		}
	}

	void handleCreateEntityOnClient(byte[] payload, int readPos) {
		bool isPlayer = readBool (payload, readPos, out readPos);
		//Debug.Log ("isPlayer = " + isPlayer + ", readPos=" + readPos);
		string entityID = readEntityID(payload, readPos, out readPos);
		//Debug.Log ("entityID = " + entityID + ", readPos=" + readPos);
		string typeName = readVarStr (payload, readPos, out readPos);
		//Debug.Log ("typeName = " + typeName + ", readPos=" + readPos);

		float x = readFloat32 (payload, readPos, out readPos);
		float y = readFloat32 (payload, readPos, out readPos);
		float z = readFloat32 (payload, readPos, out readPos);
		float yaw = readFloat32 (payload, readPos, out readPos);
		Hashtable clientAttrs = readData (payload, readPos, out readPos) as Hashtable;
		Debug.Log ("handleCreateEntityOnClient: isPlayer=" + isPlayer + ", entityID=" + entityID + ", typeName=" + typeName + ", position=" + x + "," + y + "," + z + ", yaw=" + yaw + ", clientAttrs=" + clientAttrs);
		manager.OnCreateEntity(typeName, entityID, isPlayer, x,y,z,yaw, clientAttrs);
	}

	void handleCallEntityMethodOnClient(byte[] payload, int readPos) {
		string entityID = readEntityID(payload, readPos, out readPos);
		string method = readVarStr (payload, readPos, out readPos);
		object[] args = readArgs (payload, readPos, out readPos);
		Debug.Log ("Call " + entityID + "." + method + "(" + args.Length + " args)");

		manager.OnCallEntityMethod (entityID, method, args);
	}

	void handleDestroyEntityOnClient(byte[] payload, int readPos) {
		string typeName = readVarStr (payload, readPos, out readPos);
		string entityID = readEntityID (payload, readPos, out readPos);

		Debug.Log ("Destroy " + typeName + "." + entityID);
		manager.OnDestroyEntity (typeName, entityID);
	}

	void handleNotifyMapAttrChangeOnClient(byte[] payload, int readPos) {
		string entityID = readEntityID (payload, readPos, out readPos);
        ArrayList path = readData (payload, readPos, out readPos) as ArrayList;
		string key = readVarStr (payload, readPos, out readPos);
		object val = readData (payload, readPos, out readPos);
		manager.OnMapAttrChange (entityID, path, key, val);
	}

	void handleNotifyMapAttrDelOnClient(byte[] payload, int readPos) {
		string entityID = readEntityID (payload, readPos, out readPos);
        ArrayList path = readData (payload, readPos, out readPos) as ArrayList;
		string key = readVarStr (payload, readPos, out readPos);
		manager.OnMapAttrDel (entityID, path, key);
	}

	void handleNotifyListAttrAppendOnClient(byte[] payload, int readPos) {
		string entityID = readEntityID (payload, readPos, out readPos);
        ArrayList path = readData (payload, readPos, out readPos) as ArrayList;
		object val = readData (payload, readPos, out readPos);
		manager.OnListAttrAppend (entityID, path, val);
	}

	void handleNotifyListAttrPopOnClient(byte[] payload, int readPos) {
		string entityID = readEntityID (payload, readPos, out readPos);
        ArrayList path = readData (payload, readPos, out readPos) as ArrayList;
		manager.OnListAttrPop (entityID, path);
	}
	void handleNotifyListAttrChangeOnClient(byte[] payload, int readPos) {
		string entityID = readEntityID (payload, readPos, out readPos);
        ArrayList path = readData (payload, readPos, out readPos) as ArrayList;
		int index = (int)readUint32(payload, readPos, out readPos);
		object val = readData (payload, readPos, out readPos);
		manager.OnListAttrChange (entityID, path, index, val);
	}

	object unpackData(byte[] data) { 
		MsgPack.MessagePackObject mpobj = MsgPack.Unpacking.UnpackObject (data).Value;
		return convertFromMsgPackObject (mpobj);
	}

	byte[] packData(object v) {
		MsgPack.MessagePackObject mpobj = convertToMsgPackObject (v);
		System.IO.MemoryStream stream = new System.IO.MemoryStream ();
		MsgPack.Packer packer = MsgPack.Packer.Create (stream);
		mpobj.PackToMessage (packer, null);
		stream.Flush ();
		return stream.GetBuffer ();
	}

	MsgPack.MessagePackObject convertToMsgPackObject(object v) {
		Type t = v.GetType ();
		if (t.Equals (typeof(Hashtable))) {
			Hashtable ht = v as Hashtable;
			IDictionaryEnumerator e = ht.GetEnumerator ();
			MsgPack.MessagePackObjectDictionary d = new MsgPack.MessagePackObjectDictionary ();
			while (e.MoveNext ()) {
				d.Add (new MsgPack.MessagePackObject (e.Key as string), convertToMsgPackObject (e.Value));
			}
			return new MsgPack.MessagePackObject (d);
		} else if (t.Equals (typeof(ArrayList))) {
			ArrayList al = v as ArrayList;
			IEnumerator e = al.GetEnumerator ();
			System.Collections.Generic.IList<MsgPack.MessagePackObject> l = new System.Collections.Generic.List<MsgPack.MessagePackObject> ();
			while (e.MoveNext ()) {
				l.Add (convertToMsgPackObject (e.Current));
			}
			return new MsgPack.MessagePackObject (l);
		} else if (t.Equals (typeof(bool))) {
			return new MsgPack.MessagePackObject ((bool)v);
		} else if (t.Equals (typeof(string))) {
			return new MsgPack.MessagePackObject ((string)v);
		}
		else {
			Debug.AssertFormat (false, "Unknwon type: " + t.Name);
			return new MsgPack.MessagePackObject ();
		}
	}

	object convertFromMsgPackObject(MsgPack.MessagePackObject mpobj) {
		if (mpobj.IsDictionary) {
			return convertFromMsgPackObjectDictionary (mpobj.AsDictionary());
		}
		if (mpobj.IsList) {
			return convertFromMsgPackObjectList (mpobj.AsList ());
		}
		return mpobj.ToObject ();
	}

	Hashtable convertFromMsgPackObjectDictionary(MsgPack.MessagePackObjectDictionary mpobj) {
		Hashtable ht = new Hashtable ();
		MsgPack.MessagePackObjectDictionary.Enumerator e = mpobj.GetEnumerator ();
		while (e.MoveNext ()) {
			MsgPack.MessagePackObject key = e.Current.Key;
			MsgPack.MessagePackObject val = e.Current.Value;
			ht.Add (key.AsString (), convertFromMsgPackObject (val));
		}
		return ht; 
	}

	ArrayList convertFromMsgPackObjectList(IList<MsgPack.MessagePackObject> mpobj) {
		ArrayList list = new ArrayList (mpobj.Count);
		IEnumerator<MsgPack.MessagePackObject> e = mpobj.GetEnumerator ();
		while (e.MoveNext ()) {
			list.Add (convertFromMsgPackObject (e.Current));
		}
		return list;
	}

	public void CallServer(string entityID, string method, object[] args) {
		int writePos = 0;
		writePos = appendUint16 (writePos, MT_CALL_ENTITY_METHOD_FROM_CLIENT);
		writePos = appendEntityID (writePos, entityID);
		writePos = appendVarStr (writePos, method);
		writePos = appendArgs (writePos, args);
		sendPacket (writePos);
	}

	void sendPacket(int payloadLen) {
		tcpClient.Client.Send (BitConverter.GetBytes ((UInt32) (payloadLen)));
		tcpClient.Client.Send (payloadBuff, payloadLen, SocketFlags.None);
	}

	public void SyncEntityInfoToServer(string entityID, float x, float y, float z, float yaw) {
		int writePos = 0;
		writePos = appendUint16 (writePos, MT_SYNC_POSITION_YAW_FROM_CLIENT);
		writePos = appendEntityID (writePos, entityID);
		writePos = appendFloat32 (writePos, x);
		writePos = appendFloat32 (writePos, y);
		writePos = appendFloat32 (writePos, z);
		writePos = appendFloat32 (writePos, yaw);
		sendPacket (writePos);
	}
}
