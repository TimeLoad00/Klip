using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Buffer
    {
        public List<object> buffer = new List<object>();
        public int pos = 0;

        public int ReadInt32()
        {
            int ret = (int)buffer[pos];
            pos++;
            return ret;
        }

        public string ReadString()
        {
            string ret = (string)buffer[pos];
            pos++;
            return ret;
        }

        public void Write(int data)
        {
            buffer.Add(data);
        }

        public void Write(string data)
        {
            buffer.Add(data);
        }
    }
}
