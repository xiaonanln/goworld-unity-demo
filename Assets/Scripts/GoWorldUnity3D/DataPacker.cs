using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GoWorldUnity3D
{
    class DataPacker
    {
        internal static object UnpackData(byte[] data)
        {
            MsgPack.MessagePackObject mpobj = MsgPack.Unpacking.UnpackObject(data).Value;
            return convertFromMsgPackObject(mpobj);
        }

        internal static byte[] PackData(object v)
        {
            MsgPack.MessagePackObject mpobj = convertToMsgPackObject(v);
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            MsgPack.Packer packer = MsgPack.Packer.Create(stream);
            mpobj.PackToMessage(packer, null);
            stream.Flush();
            return stream.GetBuffer();
        }

        static MsgPack.MessagePackObject convertToMsgPackObject(object v)
        {
            Type t = v.GetType();
            if (t.Equals(typeof(MapAttr)))
            {
                MapAttr ht = v as MapAttr;
                IDictionaryEnumerator e = ht.GetEnumerator();
                MsgPack.MessagePackObjectDictionary d = new MsgPack.MessagePackObjectDictionary();
                while (e.MoveNext())
                {
                    d.Add(new MsgPack.MessagePackObject(e.Key as string), convertToMsgPackObject(e.Value));
                }
                return new MsgPack.MessagePackObject(d);
            }
            else if (t.Equals(typeof(ListAttr)))
            {
                ListAttr al = v as ListAttr;
                IEnumerator e = al.GetEnumerator();
                System.Collections.Generic.IList<MsgPack.MessagePackObject> l = new System.Collections.Generic.List<MsgPack.MessagePackObject>();
                while (e.MoveNext())
                {
                    l.Add(convertToMsgPackObject(e.Current));
                }
                return new MsgPack.MessagePackObject(l);
            }
            else if (t.Equals(typeof(bool)))
            {
                return new MsgPack.MessagePackObject((bool)v);
            }
            else if (t.Equals(typeof(string)))
            {
                return new MsgPack.MessagePackObject((string)v);
            }
            else
            {
                Debug.Assert(false, "Unknwon type: " + t.Name);
                return new MsgPack.MessagePackObject();
            }
        }

        static object convertFromMsgPackObject(MsgPack.MessagePackObject mpobj)
        {
            if (mpobj.IsDictionary)
            {
                return convertFromMsgPackObjectDictionary(mpobj.AsDictionary());
            }
            if (mpobj.IsList)
            {
                return convertFromMsgPackObjectList(mpobj.AsList());
            }

            object obj = mpobj.ToObject();
            if (typeof(int).IsInstanceOfType(obj))
            {
                return (Int64)(int)obj;
            } else if (typeof(byte).IsInstanceOfType(obj))
            {
                return (Int64)(byte)obj;
            }
            else if (typeof(short).IsInstanceOfType(obj))
            {
                return (Int64)(short)obj;
            }
            else if (typeof(ushort).IsInstanceOfType(obj))
            {
                return (Int64)(ushort)obj;
            }
            else if (typeof(char).IsInstanceOfType(obj))
            {
                return (Int64)(char)obj;
            }
            else if (typeof(uint).IsInstanceOfType(obj))
            {
                return (Int64)(uint)obj;
            }
            else if (typeof(ulong).IsInstanceOfType(obj))
            {
                return (Int64)(ulong)obj;
            }
            return obj;
        }

        static MapAttr convertFromMsgPackObjectDictionary(MsgPack.MessagePackObjectDictionary mpobj)
        {
            MapAttr t = new MapAttr();
            MsgPack.MessagePackObjectDictionary.Enumerator e = mpobj.GetEnumerator();
            while (e.MoveNext())
            {
                MsgPack.MessagePackObject key = e.Current.Key;
                MsgPack.MessagePackObject val = e.Current.Value;
                t.put(key.AsString(), convertFromMsgPackObject(val));
            }
            return t;
        }

        static ListAttr convertFromMsgPackObjectList(IList<MsgPack.MessagePackObject> mpobj)
        {
            ListAttr list = new ListAttr();
            IEnumerator<MsgPack.MessagePackObject> e = mpobj.GetEnumerator();
            while (e.MoveNext())
            {
                list.append(convertFromMsgPackObject(e.Current));
            }
            return list;
        }

        internal static void ValidateDataType(object v)
        {
            Debug.Assert(typeof(MapAttr).IsInstanceOfType(v) ||
                typeof(ListAttr).IsInstanceOfType(v) ||
                typeof(string).IsInstanceOfType(v) ||
                typeof(Int64).IsInstanceOfType(v) ||
                typeof(bool).IsInstanceOfType(v) ||
                typeof(double).IsInstanceOfType(v) ||
                v == null);
        }
    }
}
