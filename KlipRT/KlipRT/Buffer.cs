using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Buffer
    {
        public List<byte> buffer = new List<byte>();
        public int pos = 0;

        public byte Read()
        {
            byte ret = buffer[pos];
            pos++;
            return ret;
        }

        public char ReadChar()
        {
            char ret = (char)buffer[pos];
            pos++;
            return ret;
        }

        public int ReadInt32()
        {
            int ret = BitConverter.ToInt32(buffer.ToArray(), pos);
            pos += 4;
            return ret;
        }

        public string ReadString()
        {
            byte length = Read();
            string ret = "";

            for (int i = 0; i < length; i++)
            {
                ret += ReadChar();
            }

            return ret;
        }

        public void Write(byte data)
        {
            buffer.Add(data);
        }

        public void Write(char data)
        {
            buffer.Add((byte)data);
        }

        public void Write(int data)
        {
            byte[] size = BitConverter.GetBytes(data);

            buffer.Add(size[0]);
            buffer.Add(size[1]);
            buffer.Add(size[2]);

            try
            {
                buffer.Add(size[3]);
            }
            catch
            {
                buffer.Add(0);
            }
        }

        public void Write(string data)
        {
            Write((byte)data.Length);

            foreach (char c in data)
            {
                Write(c);
            }
        }
    }
}
