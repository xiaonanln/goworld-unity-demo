using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoWorldUnity3D
{
    public class ListAttr
    {
        private ArrayList list = new ArrayList();

        public int Count { get
            {
                return this.list.Count;
            }
        }

        public Int64 GetInt(int index)
        {
            object val = this.get(index);
            return (Int64)val;
        }

        public bool GetBool(int index)
        {
            object val = this.get(index);
            return (bool)val; ;
        }

        public string GetStr(int index)
        {
            object val = this.get(index);
            return val != null ? val as string : "";
        }

        public MapAttr GetMapAttr(int index)
        {
            object val = this.get(index);
            return val != null ? val as MapAttr : new MapAttr();
        }

        public ListAttr GetListAttr(int index)
        {
            object val = this.get(index);
            return val != null ? val as ListAttr : new ListAttr();
        }

        internal void append(object val)
        {
            DataPacker.ValidateDataType(val);
            this.list.Add(val);
        }

        internal void pop(int index)
        {
            this.list.RemoveAt(index);
        }

        internal void set(int index, object val)
        {
            DataPacker.ValidateDataType(val);
            this.list[index] = val;
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        internal object get(int index)
        {
            return this.list[index];
        }
    }
}
