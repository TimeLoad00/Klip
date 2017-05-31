using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KlipRT
{
    class Program
    {
        static void Main(string[] args)
        {
            //StreamReader sr = new StreamReader(args[0]);
            FileStream fs = new FileStream(args[0], FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            string code = br.ReadString();
            //string code = sr.ReadToEnd();
            Runtime runtime = new Runtime(code);
        }
    }
}
