using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GoWorldUnity3D
{
    class Packet
    {
        const int INITIAL_PAYLOAD_LEN = 512;
        internal byte[] payload;
        public UInt16 MsgType { get; private set; }
        public int UnreadPayloadLen {
            get {
                return this.payload.Length - this.readPos;
            }
        }

        public int UnwrittenPayloadLen
        {
            get
            {
                return this.payload.Length - this.writePos;
            }
        }

        internal int readPos, writePos;

        public Packet(byte[] payload)
        {
            this.payload = payload;
            this.MsgType = BitConverter.ToUInt16(this.payload, 0);
            this.readPos = 2;
        }

        public Packet(UInt16 msgtype)
        {
            this.payload = new byte[INITIAL_PAYLOAD_LEN];
            byte[] b =  BitConverter.GetBytes(msgtype);
            System.Array.Copy(b, payload, b.Length);
            this.writePos = 2;
        }

        public override string ToString()
        {
            return "Packet<" + this.MsgType + "|" + this.payload.Length + ">";
        }

        internal UInt16 ReadUInt16()
        {
            this.assureUnreadPayloadLen(sizeof(UInt16));
            UInt16 res = BitConverter.ToUInt16(this.payload, this.readPos);
            this.readPos += sizeof(UInt16);
            return res; 
        }

        internal uint ReadUInt32()
        {
            this.assureUnreadPayloadLen(sizeof(UInt32));
            UInt32 res = BitConverter.ToUInt32(this.payload, this.readPos);
            this.readPos += sizeof(UInt32);
            return res;
        }

        internal void AppendUInt16(UInt16 v)
        {
            this.AppendBytes(BitConverter.GetBytes(v));
        }

        internal void AppendUInt32(UInt32 v)
        {
            this.AppendBytes(BitConverter.GetBytes(v));
        }

        internal float ReadFloat32()
        {
            this.assureUnreadPayloadLen(sizeof(float));
            float v = BitConverter.ToSingle(payload, readPos);
            this.readPos += sizeof(float);
            return v;
        }

        internal void AppendFloat32(float v)
        {
            this.AppendBytes(BitConverter.GetBytes(v));
        }

        internal byte[] ReadBytes(int len)
        {
            this.assureUnreadPayloadLen(len);
            byte[] bytes = new byte[len];
            System.Array.Copy(this.payload, this.readPos, bytes, 0, len);
            this.readPos += len;
            return bytes; 
        }

        private void AppendBytes(byte[] b)
        {
            this.assureUnwrittenPayloadLen(b.Length);
            System.Array.Copy(b, 0, this.payload, this.writePos, b.Length);
            this.writePos += b.Length;
        }

        private void assureUnwrittenPayloadLen(int len)
        {
            if (this.UnwrittenPayloadLen < len)
            {
                int nextPayloadLen = this.payload.Length * 2;
                while(nextPayloadLen - this.writePos < len)
                {
                    nextPayloadLen *= 2;
                }
                byte[] newPayload = new byte[nextPayloadLen];
                Array.Copy(this.payload, newPayload, this.payload.Length);
                this.payload = newPayload;

                Debug.Assert(this.UnwrittenPayloadLen >= len);
            }
        }

        internal void AppendVarBytes(byte[] b)
        {
            this.AppendUInt32((UInt32)(b.Length));
            this.AppendBytes(b);
        }

        internal void AppendVarStr(string s)
        {
            this.AppendVarBytes(ASCIIEncoding.ASCII.GetBytes(s));
        }

        internal object[] ReadArgs()
        {
            int nargs = this.ReadUInt16();
            object[] args = new object[nargs];
            for (int i = 0; i < nargs; i++)
            {
                args[i] = this.ReadData();
            }
            return args; 
        }

        internal void AppendArgs(object[] args)
        {
            this.AppendUInt16((UInt16)args.Length);
            foreach (object arg in args)
            {
                this.AppendData(arg);
            }
        }

        internal string ReadStr(int len)
        {
            byte[] bytes = this.ReadBytes(len);
            return ASCIIEncoding.ASCII.GetString(bytes);
        }

        private void AppendStr(string s)
        {
            this.AppendBytes(ASCIIEncoding.ASCII.GetBytes(s));
        }

        private void assureUnreadPayloadLen(int len)
        {
            Debug.Assert(this.UnreadPayloadLen >= len);
        }

        internal bool ReadBool()
        {
            return this.ReadByte() != 0;
        }

        private byte ReadByte()
        {
            this.assureUnreadPayloadLen(1);
            byte b = this.payload[this.readPos];
            this.readPos += 1;
            return b;
        }

        internal string ReadEntityID()
        {
            return this.ReadStr(Proto.ENTITYID_LENGTH);
        }

        internal void AppendEntityID(string entityID)
        {
            Debug.Assert(entityID.Length == Proto.ENTITYID_LENGTH);
            this.AppendStr(entityID);
        }

        internal string ReadVarStr()
        {
            UInt32 len = this.ReadUInt32();
            return this.ReadStr((int)(len));
        }

        internal byte[] ReadVarBytes()
        {
            UInt32 len = this.ReadUInt32();
            return this.ReadBytes((int)len);
        }

        internal object ReadData()
        {
            byte[] b = this.ReadVarBytes();
            return DataPacker.UnpackData(b);
        }

        internal void AppendData(object data)
        {
            byte[] b = DataPacker.PackData(data);
            this.AppendVarBytes(b);
        }
    }
}
