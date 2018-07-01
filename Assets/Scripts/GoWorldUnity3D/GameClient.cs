using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;

namespace GoWorldUnity3D
{
    public class GameClient
    {
        internal static GameClient Instance = new GameClient();

        private TcpClient tcpClient;
        private DateTime startConnectTime = DateTime.MinValue;
        private PacketReceiver packetReceiver;

        internal delegate void OnCreateEntityOnClientHandler(string typeName, string entityID, bool isClientOwner, float x, float y, float z, float yaw, MapAttr attrs);
        internal delegate void OnCallEntityMethodOnClientHandler(string entityID, string method, object[] args);
        internal OnCreateEntityOnClientHandler OnCreateEntityOnClient;
        internal OnCallEntityMethodOnClientHandler OnCallEntityMethodOnClient;

        public string Host { get; private set; }
        public int Port { get; private set; }

        internal GameClient()
        {
        }

        public override string ToString()
        {
            return "GameClient<" + this.Host + ":" + this.Port + ">";
        }

        internal void Connect(string host, int port)
        {
            this.Host = host;
            this.Port = port;
            this.disconnectTCPClient();
        }

        internal void Disconnect()
        {
            this.Host = "";
            this.Port = 0;
            this.disconnectTCPClient();
        }

        private void disconnectTCPClient()
        {
            if (this.tcpClient != null)
            {
                this.tcpClient.Close();
                this.tcpClient = null;
                this.packetReceiver = null;
                this.startConnectTime = DateTime.MinValue;
                GoWorldLogger.Warn(this.ToString(), "Disconnected");
            }
        }

        internal void CallServer(string entityID, string method, object[] args)
        {
            Packet pkt = new Packet(Proto.MT_CALL_ENTITY_METHOD_FROM_CLIENT);
            pkt.AppendEntityID(entityID);
            pkt.AppendVarStr(method);
            pkt.AppendArgs(args);
            this.sendPacket(pkt);
        }

        private void sendPacket(Packet pkt)
        {
            if (this.tcpClient == null)
            {
                GoWorldLogger.Warn("GameClient", "Game Client Is Not Connected, Send Packet Failed: " + pkt);
            }

            Debug.Assert(pkt.writePos >= sizeof(UInt16));
            this.sendAll(BitConverter.GetBytes((UInt32)pkt.writePos), sizeof(UInt32));
            this.sendAll(pkt.payload, pkt.writePos);
        }

        private void sendAll(byte[] b, int len)
        {
            // todo: send async
            int sent = 0;
            while (sent < len)
            {
                int n = this.tcpClient.Client.Send(b, sent, len-sent, SocketFlags.None);
                sent += n;
            }
        }

        internal void Tick()
        {
            if (this.Host == "")
            {
                this.disconnectTCPClient();
            }
            else
            {
                this.assureTCPClientConnected();
                if (this.tcpClient != null && this.tcpClient.Connected && this.tcpClient.Available > 0)
                {
                    this.tryRecvNextPacket();
                }
            }
        }

        internal void syncPositionYawFromClient(string entityID, float x, float y, float z, float yaw)
        {
            Packet pkt = new Packet(Proto.MT_SYNC_POSITION_YAW_FROM_CLIENT);
            pkt.AppendEntityID(entityID);
            pkt.AppendFloat32(x);
            pkt.AppendFloat32(y);
            pkt.AppendFloat32(z);
            pkt.AppendFloat32(yaw);
            this.sendPacket(pkt);
        }

        private void tryRecvNextPacket()
        {
            Packet pkt =  this.packetReceiver.RecvPacket();
            if (pkt != null)
            {
                this.handlePacket(pkt);
            }
        }

        private void handlePacket(Packet pkt)
        {
            UInt16 msgtype = pkt.MsgType;
            if (msgtype != Proto.MT_CALL_FILTERED_CLIENTS && msgtype != Proto.MT_SYNC_POSITION_YAW_ON_CLIENTS)
            {
                UInt16 gateid = pkt.ReadUInt16();
                string clientid = pkt.ReadStr(Proto.CLIENTID_LENGTH);
                //this.debug("Gate ID = " + gateid + ", Client ID = " + clientid + " Ignored");
            }

            switch (msgtype)
            {
                case Proto.MT_CREATE_ENTITY_ON_CLIENT:
                    this.handleCreateEntityOnClient(pkt);
                    break;
                case Proto.MT_CALL_ENTITY_METHOD_ON_CLIENT:
                    this.handleCallEntityMethodOnClient(pkt);
                    break;
                case Proto.MT_DESTROY_ENTITY_ON_CLIENT:
                    this.handleDestroyEntityOnClient(pkt);
                    break;
                case Proto.MT_SYNC_POSITION_YAW_ON_CLIENTS:
                    this.handleSyncPositionYawOnClients(pkt);
                    break;
                case Proto.MT_NOTIFY_MAP_ATTR_CHANGE_ON_CLIENT:
                    this.handleNotifyMapAttrChangeOnClient(pkt);
                    break;
                case Proto.MT_NOTIFY_MAP_ATTR_DEL_ON_CLIENT:
                    this.handleNotifyMapAttrDelOnClient(pkt);
                    break;
                case Proto.MT_NOTIFY_MAP_ATTR_CLEAR_ON_CLIENT:
                    this.handleNotifyMapAttrClearOnClient(pkt);
                    break;
                default:
                    Debug.Assert(false, "Unknown Message Type: " + pkt);
                    break;
            }
        }

        private void handleNotifyMapAttrClearOnClient(Packet pkt)
        {
            string entityID = pkt.ReadEntityID();
            ListAttr path = pkt.ReadData() as ListAttr;
            EntityManager.Instance.OnMapAttrClear(entityID, path);
        }

        private void handleNotifyMapAttrDelOnClient(Packet pkt)
        {
            string entityID = pkt.ReadEntityID();
            ListAttr path = pkt.ReadData() as ListAttr;
            string key = pkt.ReadVarStr();
            EntityManager.Instance.OnMapAttrDel(entityID, path, key);
        }

        private void handleNotifyMapAttrChangeOnClient(Packet pkt)
        {
            string entityID = pkt.ReadEntityID();
            ListAttr path = pkt.ReadData() as ListAttr;
            string key = pkt.ReadVarStr();
            object val = pkt.ReadData();
            EntityManager.Instance.OnMapAttrChange(entityID, path, key, val);
        }

        private void handleSyncPositionYawOnClients(Packet pkt)
        {
            while (pkt.UnreadPayloadLen > 0)
            {
                string entityID = pkt.ReadEntityID();
                float x = pkt.ReadFloat32();
                float y = pkt.ReadFloat32();
                float z = pkt.ReadFloat32();
                float yaw = pkt.ReadFloat32();
                EntityManager.Instance.OnSyncEntityInfo(entityID, x, y, z, yaw);
            }
        }

        private void handleDestroyEntityOnClient(Packet pkt)
        {
            string typeName = pkt.ReadVarStr();
            string entityID = pkt.ReadEntityID();
            EntityManager.Instance.DestroyEntity(entityID);
        }

        private void handleCreateEntityOnClient(Packet pkt)
        {
            bool isClientOwner = pkt.ReadBool();
            string entityID = pkt.ReadEntityID();
            string typeName = pkt.ReadVarStr();
            float x = pkt.ReadFloat32();
            float y = pkt.ReadFloat32();
            float z = pkt.ReadFloat32();
            float yaw = pkt.ReadFloat32();
            MapAttr attrs = pkt.ReadData() as MapAttr;
            // this.debug ("Handle Create Entity On Client: IsClientOwner = {0}, EntityID = {1}, TypeName = {2}, Position = {3},{4},{5}, Yaw = {6}, Attrs = {7}", isClientOwner, entityID, typeName, x,y,z, yaw, attrs);
            if (OnCreateEntityOnClient != null )
            {
                this.OnCreateEntityOnClient(typeName, entityID, isClientOwner, x, y, z, yaw, attrs);
            }
        }

        private void handleCallEntityMethodOnClient(Packet pkt)
        {
            string entityID = pkt.ReadEntityID();
            string method = pkt.ReadVarStr();
            object[] args = pkt.ReadArgs();
            GoWorldLogger.Debug("GameClient", "Handle Call Entity Method On Client: {0}.{1}({2})", entityID, method, args);
            if (OnCallEntityMethodOnClient != null)
            {
                this.OnCallEntityMethodOnClient(entityID, method, args);
            }
            //manager.OnCallEntityMethod(entityID, method, args);
        }

        private void assureTCPClientConnected()
        {
            if (this.tcpClient != null)
            {
                if (!this.tcpClient.Connected)
                {
                    this.checkConnectTimeout();
                }
                return;
            }

            // no tcpClient == not connecting, start new connection ...
            GoWorldLogger.Info( this.ToString(),"Connecting ...");
            this.tcpClient = new TcpClient(AddressFamily.InterNetwork);
            this.tcpClient.NoDelay = true;
            this.tcpClient.SendTimeout = 5000;
            this.tcpClient.ReceiveBufferSize = Proto.MAX_PAYLOAD_LEN + Proto.SIZE_FIELD_SIZE;
            this.startConnectTime = DateTime.Now;
            this.packetReceiver = new PacketReceiver(this.tcpClient);
            this.tcpClient.Connect(this.Host, this.Port);
            this.tcpClient.Client.Blocking = false;
            //this.tcpClient.BeginConnect(this.Host, this.Port, this.onConnected, null); // TODO: BeginConnect fail in Unity3D
            this.onConnected(null);
        }

        private void checkConnectTimeout()
        {
            Debug.Assert(this.tcpClient != null);
            if (DateTime.Now - this.startConnectTime > TimeSpan.FromSeconds(5))
            {
                this.disconnectTCPClient();
            }
        }

        private void onConnected(IAsyncResult ar)
        {
            if (this.tcpClient.Connected)
            {
                GoWorldLogger.Info(this.ToString(), "Connected " + this.tcpClient.Connected);
            }
            else
            {
                GoWorldLogger.Warn(this.ToString(), "Connect Failed!");
                this.disconnectTCPClient();
            }
        }
    }
}
